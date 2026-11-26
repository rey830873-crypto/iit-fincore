using Dapper;
using IIT.FinCore.WinForms.Data;
using IIT.FinCore.WinForms.Models;

namespace IIT.FinCore.WinForms.Services;

/// <summary>
/// Service de Notifications intelligentes temps réel (MySQL) :
/// - Détecte automatiquement les dépassements budgétaires (>=80% / >=100%), demandes en attente,
///   cotisations impayées et opérations non rapprochées (sans doublons).
/// - Permet d'émettre des alertes instantanées lors des actions clés (validations, clôtures).
/// </summary>
public class NotificationService
{
    private readonly TreasuryRepository _repo = new();

    public void EnsureSchema()
    {
        using var conn = DatabaseConfig.CreateConnection();
        conn.Execute("""
            CREATE TABLE IF NOT EXISTS notifications (
                id INT AUTO_INCREMENT PRIMARY KEY,
                utilisateur_id INT NULL,
                role_cible VARCHAR(30) NOT NULL DEFAULT 'ALL',
                structure_id INT NULL,
                niveau ENUM('CRITIQUE', 'ALERTE', 'INFO', 'SUCCES') NOT NULL DEFAULT 'INFO',
                cle_unique VARCHAR(120) NULL,
                titre VARCHAR(160) NOT NULL,
                message VARCHAR(400) NOT NULL,
                module_cible VARCHAR(40) NOT NULL DEFAULT 'dashboard',
                est_lue TINYINT(1) NOT NULL DEFAULT 0,
                date_creation DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                INDEX idx_notif_role (role_cible, est_lue, date_creation),
                INDEX idx_notif_cle (cle_unique)
            ) ENGINE=InnoDB;
            """);
    }

    /// <summary>
    /// Analyse l'état financier en base MySQL et génère automatiquement les alertes pertinentes
    /// (dédupliquées par clé unique journalière).
    /// </summary>
    public void SyncAutomaticAlerts()
    {
        try
        {
            EnsureSchema();
            var today = DateTime.Today.ToString("yyyyMMdd");

            // 1. Alertes budgétaires (>= 80% et >= 100%)
            var budgets = _repo.GetSuiviBudgetaire();
            foreach (var b in budgets)
            {
                if (b.TauxExecutionPct >= 100m)
                {
                    InsertIfNotExists(
                        cleUnique: $"BUDGET_OVER_{b.BudgetId}_{today}",
                        roleCible: "ALL",
                        structureId: null,
                        niveau: "CRITIQUE",
                        titre: $"Dépassement budgétaire ({b.TauxExecutionPct:0.#}%)",
                        message: $"Le budget « {b.CentreAnalytique} » ({b.StructureNom}) a dépassé son enveloppe ({b.MontantRealise:N0} / {b.MontantPrevu:N0} FCFA).",
                        moduleCible: "budgets");
                }
                else if (b.TauxExecutionPct >= 80m)
                {
                    InsertIfNotExists(
                        cleUnique: $"BUDGET_WARN_{b.BudgetId}_{today}",
                        roleCible: "ALL",
                        structureId: null,
                        niveau: "ALERTE",
                        titre: $"Seuil budgétaire atteint ({b.TauxExecutionPct:0.#}%)",
                        message: $"Le centre « {b.CentreAnalytique} » ({b.StructureNom}) approche de sa limite ({b.MontantRealise:N0} / {b.MontantPrevu:N0} FCFA).",
                        moduleCible: "budgets");
                }
            }

            // 2. Demandes de dépenses en attente de validation (ciblé ADMIN)
            var pending = _repo.GetDemandesValidation(onlyPending: true);
            if (pending.Count > 0)
            {
                var totalPending = pending.Sum(d => d.Montant);
                InsertIfNotExists(
                    cleUnique: $"PENDING_VAL_{pending.Count}_{today}",
                    roleCible: "ADMIN",
                    structureId: null,
                    niveau: "ALERTE",
                    titre: $"{pending.Count} demande(s) en attente d'approbation",
                    message: $"Montant total à valider : {totalPending:N0} FCFA. Cliquez pour examiner et approuver ou rejeter.",
                    moduleCible: "validations");
            }

            // 3. Cotisations impayées ou partielles
            using var conn = DatabaseConfig.CreateConnection();
            var impayes = conn.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM paiements_cotisations WHERE statut IN ('IMPAYE', 'PARTIEL')");
            if (impayes > 0)
            {
                InsertIfNotExists(
                    cleUnique: $"COTIS_IMP_{impayes}_{today}",
                    roleCible: "ALL",
                    structureId: null,
                    niveau: "INFO",
                    titre: $"{impayes} cotisation(s) impayée(s) ou partielle(s)",
                    message: "Des membres restent à relancer. Ouvrez Cotisations pour envoyer des relances PDF ou WhatsApp.",
                    moduleCible: "cotisations");
            }

            // 4. Opérations validées non rapprochées sur relevé bancaire / Mobile Money
            var nonPointees = conn.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM transactions WHERE statut = 'VALIDEE' AND est_rapprochee = 0");
            if (nonPointees > 0)
            {
                InsertIfNotExists(
                    cleUnique: $"RAPP_PENDING_{nonPointees}_{today}",
                    roleCible: "ALL",
                    structureId: null,
                    niveau: "INFO",
                    titre: $"{nonPointees} opération(s) à rapprocher",
                    message: "Certaines opérations validées ne sont pas encore pointées sur vos relevés bancaires ou Mobile Money.",
                    moduleCible: "transactions");
            }
        }
        catch
        {
            // Tolérant en cas de coupure temporaire
        }
    }

    public void PushNotification(
        string roleCible,
        int? structureId,
        string niveau,
        string titre,
        string message,
        string moduleCible,
        int? utilisateurId = null)
    {
        try
        {
            EnsureSchema();
            using var conn = DatabaseConfig.CreateConnection();
            conn.Execute("""
                INSERT INTO notifications (utilisateur_id, role_cible, structure_id, niveau, titre, message, module_cible, est_lue)
                VALUES (@uid, @role, @sid, @niv, @titre, @msg, @mod, 0)
                """, new { uid = utilisateurId, role = roleCible, sid = structureId, niv = niveau, titre, msg = message, mod = moduleCible });
        }
        catch
        {
            // N'interrompt jamais l'opération métier principale
        }
    }

    private static void InsertIfNotExists(
        string cleUnique, string roleCible, int? structureId,
        string niveau, string titre, string message, string moduleCible)
    {
        using var conn = DatabaseConfig.CreateConnection();
        var exists = conn.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM notifications WHERE cle_unique = @cle", new { cle = cleUnique });
        if (exists > 0) return;

        conn.Execute("""
            INSERT INTO notifications (role_cible, structure_id, niveau, cle_unique, titre, message, module_cible, est_lue)
            VALUES (@role, @sid, @niv, @cle, @titre, @msg, @mod, 0)
            """, new { role = roleCible, sid = structureId, niv = niveau, cle = cleUnique, titre, msg = message, mod = moduleCible });
    }

    public List<NotificationItem> GetNotificationsForCurrentUser(int limit = 40)
    {
        try
        {
            var u = UserSession.CurrentUser;
            if (u == null) return new();

            using var conn = DatabaseConfig.CreateConnection();
            return conn.Query<NotificationItem>("""
                SELECT id AS Id,
                       utilisateur_id AS UtilisateurId,
                       role_cible AS RoleCible,
                       structure_id AS StructureId,
                       niveau AS Niveau,
                       cle_unique AS CleUnique,
                       titre AS Titre,
                       message AS Message,
                       module_cible AS ModuleCible,
                       est_lue AS EstLue,
                       date_creation AS DateCreation
                FROM notifications
                WHERE (utilisateur_id = @uid
                       OR (utilisateur_id IS NULL AND (role_cible = 'ALL' OR role_cible = @role)))
                  AND (structure_id IS NULL OR @role <> 'TRESORIER' OR structure_id = @sid)
                ORDER BY est_lue ASC,
                         FIELD(niveau, 'CRITIQUE', 'ALERTE', 'SUCCES', 'INFO'),
                         date_creation DESC
                LIMIT @lim
                """, new { uid = u.Id, role = u.RoleCode, sid = u.StructureId, lim = limit }).ToList();
        }
        catch
        {
            return new();
        }
    }

    public int GetUnreadCount()
    {
        try
        {
            var u = UserSession.CurrentUser;
            if (u == null) return 0;

            using var conn = DatabaseConfig.CreateConnection();
            return conn.ExecuteScalar<int>("""
                SELECT COUNT(*)
                FROM notifications
                WHERE est_lue = 0
                  AND (utilisateur_id = @uid
                       OR (utilisateur_id IS NULL AND (role_cible = 'ALL' OR role_cible = @role)))
                  AND (structure_id IS NULL OR @role <> 'TRESORIER' OR structure_id = @sid)
                """, new { uid = u.Id, role = u.RoleCode, sid = u.StructureId });
        }
        catch
        {
            return 0;
        }
    }

    public void MarkAsRead(int id)
    {
        try
        {
            using var conn = DatabaseConfig.CreateConnection();
            conn.Execute("UPDATE notifications SET est_lue = 1 WHERE id = @id", new { id });
        }
        catch { }
    }

    public void MarkAllAsRead()
    {
        try
        {
            var u = UserSession.CurrentUser;
            if (u == null) return;
            using var conn = DatabaseConfig.CreateConnection();
            conn.Execute("""
                UPDATE notifications
                SET est_lue = 1
                WHERE est_lue = 0
                  AND (utilisateur_id = @uid
                       OR (utilisateur_id IS NULL AND (role_cible = 'ALL' OR role_cible = @role)))
                  AND (structure_id IS NULL OR @role <> 'TRESORIER' OR structure_id = @sid)
                """, new { uid = u.Id, role = u.RoleCode, sid = u.StructureId });
        }
        catch { }
    }

    public static string FormatRelativeTime(DateTime dt)
    {
        var diff = DateTime.Now - dt;
        if (diff.TotalMinutes < 1) return "À l'instant";
        if (diff.TotalMinutes < 60) return $"Il y a {(int)diff.TotalMinutes} min";
        if (diff.TotalHours < 24 && dt.Date == DateTime.Today) return $"Aujourd'hui à {dt:HH:mm}";
        if (dt.Date == DateTime.Today.AddDays(-1)) return $"Hier à {dt:HH:mm}";
        return dt.ToString("dd/MM/yyyy HH:mm");
    }
}
