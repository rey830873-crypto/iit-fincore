using System.Net;
using System.Net.Sockets;
using Dapper;
using IIT.FinCore.WinForms.Models;

namespace IIT.FinCore.WinForms.Data;

public class AuthRepository
{
    /// <summary>
    /// Remplace les hash placeholders du jeu de données initial par de vrais hash BCrypt au premier démarrage.
    /// Mots de passe de démonstration : admin123 / tresorier123 / consultant123
    /// </summary>
    public void EnsureDemoPasswordsHashed()
    {
        using var conn = DatabaseConfig.CreateConnection();
        conn.Open();

        var defaults = new Dictionary<string, string>
        {
            ["admin@iit.ci"] = "admin123",
            ["tresorier@iit.ci"] = "tresorier123",
            ["consultant@iit.ci"] = "consultant123"
        };

        foreach (var (email, plain) in defaults)
        {
            var currentHash = conn.ExecuteScalar<string?>(
                "SELECT mot_de_passe_hash FROM utilisateurs WHERE email = @Email",
                new { Email = email });

            if (currentHash != null && currentHash.Contains("PLACEHOLDER_HASH"))
            {
                var realHash = BCrypt.Net.BCrypt.HashPassword(plain);
                conn.Execute(
                    "UPDATE utilisateurs SET mot_de_passe_hash = @Hash WHERE email = @Email",
                    new { Hash = realHash, Email = email });
            }
        }
    }

    public (Utilisateur? User, List<string> Permissions, string? ErrorMessage) Authenticate(string email, string password)
    {
        var cleanEmail = email.Trim();
        var (isLocked, secondsLeft) = Services.SecurityService.CheckLockout(cleanEmail);
        if (isLocked)
        {
            return (null, new List<string>(), $"Compte temporairement verrouillé suite à trop d'échecs. Réessayez dans {secondsLeft}s.");
        }

        using var conn = DatabaseConfig.CreateConnection();
        conn.Open();

        const string sql = @"
            SELECT u.id AS Id,
                   u.nom_complet AS NomComplet,
                   u.email AS Email,
                   u.mot_de_passe_hash AS MotDePasseHash,
                   u.telephone AS Telephone,
                   u.role_id AS RoleId,
                   r.code AS RoleCode,
                   r.nom AS RoleNom,
                   u.structure_id AS StructureId,
                   s.nom AS StructureNom,
                   u.est_actif AS EstActif,
                   u.dernier_acces AS DernierAcces
            FROM utilisateurs u
            JOIN roles r ON r.id = u.role_id
            LEFT JOIN structures s ON s.id = u.structure_id
            WHERE u.email = @Email AND u.est_actif = 1
            LIMIT 1;";

        var user = conn.QueryFirstOrDefault<Utilisateur>(sql, new { Email = cleanEmail });
        if (user == null)
        {
            bool nowLocked = Services.SecurityService.RecordFailedAttempt(cleanEmail);
            LogActivity(null, nowLocked ? "Verrouillage anti-brute-force" : "Échec de connexion", "Sécurité", "utilisateurs", null, $"E-mail inconnu ou inactif : {cleanEmail}");
            return (null, new List<string>(), nowLocked
                ? "Trop de tentatives échouées. Compte bloqué pendant 60 secondes."
                : "Identifiants invalides ou compte désactivé.");
        }

        bool valid;
        try
        {
            valid = BCrypt.Net.BCrypt.Verify(password, user.MotDePasseHash);
        }
        catch
        {
            valid = false;
        }

        if (!valid)
        {
            bool nowLocked = Services.SecurityService.RecordFailedAttempt(cleanEmail);
            LogActivity(user.Id, nowLocked ? "Verrouillage anti-brute-force" : "Mot de passe incorrect", "Sécurité", "utilisateurs", user.Id, $"Échec d'authentification pour {cleanEmail}");
            return (null, new List<string>(), nowLocked
                ? "Trop de tentatives échouées. Compte bloqué pendant 60 secondes."
                : "Identifiants invalides ou compte désactivé.");
        }

        Services.SecurityService.ResetAttempts(cleanEmail);
        conn.Execute("UPDATE utilisateurs SET dernier_acces = NOW() WHERE id = @Id", new { user.Id });

        var perms = conn.Query<string>(@"
            SELECT p.code
            FROM role_permissions rp
            JOIN permissions p ON p.id = rp.permission_id
            WHERE rp.role_id = @RoleId;",
            new { user.RoleId }).ToList();

        LogActivity(user.Id, "Connexion réussie", "Authentification", "utilisateurs", user.Id,
            $"Connexion de {user.NomComplet} ({user.RoleNom})");

        return (user, perms, null);
    }

    public (bool Success, string Message) ResetPasswordVerified(string email, string telephoneVerification, string newPassword)
    {
        var (okPwd, pwdMsg) = Services.SecurityService.ValidatePasswordStrength(newPassword);
        if (!okPwd) return (false, pwdMsg);

        using var conn = DatabaseConfig.CreateConnection();
        conn.Open();

        var row = conn.QueryFirstOrDefault<(int Id, string? Telephone)>(
            "SELECT id AS Id, telephone AS Telephone FROM utilisateurs WHERE email = @Email AND est_actif = 1 LIMIT 1;",
            new { Email = email.Trim() });

        if (row.Id == 0)
            return (false, "Vérification impossible : informations incorrectes.");

        string normStored = new string((row.Telephone ?? "").Where(char.IsDigit).ToArray());
        string normInput = new string((telephoneVerification ?? "").Where(char.IsDigit).ToArray());

        if (normInput.Length < 6 || string.IsNullOrEmpty(normStored) || !normStored.EndsWith(normInput))
        {
            LogActivity(row.Id, "Tentative reset refusée", "Sécurité", "utilisateurs", row.Id, $"Téléphone de vérification invalide pour {email.Trim()}");
            return (false, "Le numéro de téléphone ne correspond pas au compte enregistré.");
        }

        var hash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        conn.Execute("UPDATE utilisateurs SET mot_de_passe_hash = @Hash WHERE id = @Id;",
            new { Hash = hash, Id = row.Id });

        Services.SecurityService.ResetAttempts(email);
        LogActivity(row.Id, "Réinitialisation mot de passe vérifiée", "Sécurité", "utilisateurs", row.Id,
            $"Mot de passe réinitialisé après vérification téléphonique pour {email.Trim()}");
        return (true, "Mot de passe réinitialisé avec succès.");
    }

    public bool ChangePassword(int userId, string currentPassword, string newPassword)
    {
        using var conn = DatabaseConfig.CreateConnection();
        conn.Open();
        var currentHash = conn.ExecuteScalar<string?>(
            "SELECT mot_de_passe_hash FROM utilisateurs WHERE id = @Id;",
            new { Id = userId });

        if (currentHash == null || !BCrypt.Net.BCrypt.Verify(currentPassword, currentHash))
            return false;

        var newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        conn.Execute("UPDATE utilisateurs SET mot_de_passe_hash = @Hash WHERE id = @Id;",
            new { Hash = newHash, Id = userId });

        LogActivity(userId, "Changement de mot de passe", "Profil", "utilisateurs", userId, "Mot de passe modifié par l'utilisateur");
        return true;
    }

    public void LogActivity(int? userId, string action, string module, string? entity = null, int? entityId = null, string? details = null)
    {
        try
        {
            using var conn = DatabaseConfig.CreateConnection();
            conn.Open();
            conn.Execute(@"
                INSERT INTO journal_activites (utilisateur_id, action, module, entite_cible, entite_id, details, adresse_ip)
                VALUES (@UserId, @Action, @Module, @Entity, @EntityId, @Details, @Ip);",
                new
                {
                    UserId = userId,
                    Action = action,
                    Module = module,
                    Entity = entity,
                    EntityId = entityId,
                    Details = details,
                    Ip = GetLocalIpAddress()
                });
        }
        catch
        {
            // Ne bloque jamais l'action métier si l'audit échoue
        }
    }

    private static string GetLocalIpAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }
        }
        catch { }
        return "127.0.0.1";
    }
}
