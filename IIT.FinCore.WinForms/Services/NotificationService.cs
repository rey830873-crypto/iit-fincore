using Dapper;
using IIT.FinCore.WinForms.Data;
using IIT.FinCore.WinForms.Models;

namespace IIT.FinCore.WinForms.Services;

public record NotificationPreferences(bool ToastEnabled, bool SoundEnabled, decimal LowBalanceThreshold);

/// <summary>
/// Service de Notifications intelligentes temps réel (MySQL) :
/// - Détecte automatiquement : dépassements budgétaires (>=80% / >=100%), demandes en attente,
///   comptes à solde bas, échéances récurrentes J-3, cotisations impayées et opérations non rapprochées.
/// - Gère les préférences utilisateur (Toast, son discret, seuil de solde bas) et la suppression/nettoyage.
/// </summary>
public class NotificationService
{
    private readonly TreasuryRepository _repo = new();
    private static volatile bool _schemaEnsured;

    public void EnsureSchema()
    {
        if (_schemaEnsured) return;
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
        _schemaEnsured = true;
    }

    public NotificationPreferences GetPreferences()
    {
        try
        {
            using var conn = DatabaseConfig.CreateConnection();
            var rows = conn.Query<(string Cle, string Val)>(
                "SELECT cle_param, valeur_param FROM parametres_generaux WHERE cle_param IN ('NOTIF_TOAST_ENABLED', 'NOTIF_SOUND_ENABLED', 'NOTIF_LOW_BALANCE_THRESHOLD')")
                .ToDictionary(x => x.Cle, x => x.Val);

            bool toast = !rows.TryGetValue("NOTIF_TOAST_ENABLED", out var tv) || tv != "0";
            bool sound = !rows.TryGetValue("NOTIF_SOUND_ENABLED", out var sv) || sv != "0";
            decimal threshold = rows.TryGetValue("NOTIF_LOW_BALANCE_THRESHOLD", out var lv) && decimal.TryParse(lv, out var parsed)
                ? parsed : 200_000m;

            return new NotificationPreferences(toast, sound, threshold);
        }
        catch
        {
            return new NotificationPreferences(true, true, 200_000m);
        }
    }

    public void SavePreferences(NotificationPreferences prefs)
    {
        using var conn = DatabaseConfig.CreateConnection();
        UpsertParam(conn, "NOTIF_TOAST_ENABLED", prefs.ToastEnabled ? "1" : "0", "Afficher les popups Toast de notification");
        UpsertParam(conn, "NOTIF_SOUND_ENABLED", prefs.SoundEnabled ? "1" : "0", "Jouer un son discret sur les alertes critiques");
        UpsertParam(conn, "NOTIF_LOW_BALANCE_THRESHOLD", ((long)prefs.LowBalanceThreshold).ToString(), "Seuil d'alerte de solde bas par compte (FCFA)");
    }

    private static void UpsertParam(System.Data.IDbConnection conn, string key, string val, string desc)
    {
        conn.Execute("""
            INSERT INTO parametres_generaux (cle_param, valeur_param, description)
            VALUES (@k, @v, @d)
            ON DUPLICATE KEY UPDATE valeur_param = @v
            """, new { k = key, v = val, d = desc });
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
            var prefs = GetPreferences();

            using var conn = DatabaseConfig.CreateConnection();
            conn.Open();

            // Charge en 1 seule requête toutes les clés déjà générées aujourd'hui (évite N requêtes SELECT COUNT)
            var existingKeys = new HashSet<string>(
                conn.Query<string>(
                    "SELECT cle_unique FROM notifications WHERE cle_unique LIKE @pat",
                    new { pat = $"%_{today}" }),
                StringComparer.Ordinal);

            void InsertIfMissing(string cleUnique, string roleCible, int? structureId,
                string niveau, string titre, string message, string moduleCible)
            {
                if (!existingKeys.Add(cleUnique)) return;
                conn.Execute("""
                    INSERT INTO notifications (role_cible, structure_id, niveau, cle_unique, titre, message, module_cible, est_lue)
                    VALUES (@role, @sid, @niv, @cle, @titre, @msg, @mod, 0)
                    """, new { role = roleCible, sid = structureId, niv = niveau, cle = cleUnique, titre, msg = message, mod = moduleCible });
            }

            // 1. Alertes budgétaires (>= 80% et >= 100%)
            var budgets = _repo.GetSuiviBudgetaire();
            foreach (var b in budgets)
            {
                if (b.TauxExecutionPct >= 100m)
                {
                    InsertIfMissing(
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
                    InsertIfMissing(
                        cleUnique: $"BUDGET_WARN_{b.BudgetId}_{today}",
                        roleCible: "ALL",
                        structureId: null,
                        niveau: "ALERTE",
                        titre: $"Seuil budgétaire atteint ({b.TauxExecutionPct:0.#}%)",
                        message: $"Le centre « {b.CentreAnalytique} » ({b.StructureNom}) approche de sa limite ({b.MontantRealise:N0} / {b.MontantPrevu:N0} FCFA).",
                        moduleCible: "budgets");
                }
            }

            // 2. Comptes à solde bas (< seuil configuré)
            var comptes = _repo.GetComptes();
            foreach (var c in comptes.Where(x => x.EstActif && x.SoldeActuel < prefs.LowBalanceThreshold))
            {
                InsertIfMissing(
                    cleUnique: $"LOW_BAL_{c.Id}_{today}",
                    roleCible: "ALL",
                    structureId: c.StructureId,
                    niveau: "CRITIQUE",
                    titre: $"Solde bas : {c.NomCompte}",
                    message: $"Solde actuel de {c.SoldeActuel:N0} {c.Devise}, inférieur au seuil d'alerte de {prefs.LowBalanceThreshold:N0} FCFA ({c.StructureNom}).",
                    moduleCible: "comptes");
            }

            // 3. Échéances d'opérations récurrentes dans les 3 prochains jours (J-3)
            var recurrentes = _repo.GetOperationsRecurrentes();
            var limitDate = DateTime.Today.AddDays(3);
            foreach (var op in recurrentes.Where(o => o.EstActif && o.ProchaineEcheance.Date <= limitDate))
            {
                var days = (op.ProchaineEcheance.Date - DateTime.Today).Days;
                var when = days < 0 ? $"en retard de {-days} j" : (days == 0 ? "aujourd'hui" : $"dans {days} j");
                InsertIfMissing(
                    cleUnique: $"REC_DUE_{op.Id}_{today}",
                    roleCible: "ALL",
                    structureId: op.StructureId,
                    niveau: "ALERTE",
                    titre: $"Échéance récurrente {when}",
                    message: $"{op.Libelle} ({op.Montant:N0} FCFA - {op.TypeOperation}) prévue le {op.ProchaineEcheance:dd/MM/yyyy}.",
                    moduleCible: "pilotage");
            }

            // 4. Demandes de dépenses en attente de validation (ciblé ADMIN)
            var pending = _repo.GetDemandesValidation(onlyPending: true);
            if (pending.Count > 0)
            {
                var totalPending = pending.Sum(d => d.Montant);
                InsertIfMissing(
                    cleUnique: $"PENDING_VAL_{pending.Count}_{today}",
                    roleCible: "ADMIN",
                    structureId: null,
                    niveau: "ALERTE",
                    titre: $"{pending.Count} demande(s) en attente d'approbation",
                    message: $"Montant total à valider : {totalPending:N0} FCFA. Cliquez pour examiner et approuver ou rejeter.",
                    moduleCible: "validations");
            }

            // 5. Cotisations impayées ou partielles + 6. Opérations non rapprochées (1 seule requête combinée)
            var counts = conn.QuerySingle<(int Impayes, int NonPointees)>("""
                SELECT
                    (SELECT COUNT(*) FROM paiements_cotisations WHERE statut IN ('IMPAYE', 'PARTIEL')) AS Impayes,
                    (SELECT COUNT(*) FROM transactions WHERE statut = 'VALIDEE' AND est_rapprochee = 0) AS NonPointees;
                """);

            if (counts.Impayes > 0)
            {
                InsertIfMissing(
                    cleUnique: $"COTIS_IMP_{counts.Impayes}_{today}",
                    roleCible: "ALL",
                    structureId: null,
                    niveau: "INFO",
                    titre: $"{counts.Impayes} cotisation(s) impayée(s) ou partielle(s)",
                    message: "Des membres restent à relancer. Ouvrez Cotisations pour envoyer des relances PDF ou WhatsApp.",
                    moduleCible: "cotisations");
            }

            if (counts.NonPointees > 0)
            {
                InsertIfMissing(
                    cleUnique: $"RAPP_PENDING_{counts.NonPointees}_{today}",
                    roleCible: "ALL",
                    structureId: null,
                    niveau: "INFO",
                    titre: $"{counts.NonPointees} opération(s) à rapprocher",
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

    public void DeleteNotification(int id)
    {
        try
        {
            using var conn = DatabaseConfig.CreateConnection();
            conn.Execute("DELETE FROM notifications WHERE id = @id", new { id });
        }
        catch { }
    }

    public void ClearReadNotifications()
    {
        try
        {
            var u = UserSession.CurrentUser;
            if (u == null) return;
            using var conn = DatabaseConfig.CreateConnection();
            conn.Execute("""
                DELETE FROM notifications
                WHERE est_lue = 1
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
