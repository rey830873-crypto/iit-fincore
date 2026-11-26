using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace IIT.FinCore.WinForms.Services;

/// <summary>
/// Service central de sécurité applicative IIT FINCORE :
/// - Politique de complexité des mots de passe
/// - Protection anti-brute-force (verrouillage temporaire après 5 échecs)
/// - Chiffrement AES-256 pour données sensibles locales
/// </summary>
public static class SecurityService
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromSeconds(60);

    private static readonly ConcurrentDictionary<string, (int Count, DateTime? LockedUntil)> _attempts = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Vérifie si un compte e-mail est temporairement verrouillé suite à trop d'échecs de connexion.
    /// </summary>
    public static (bool IsLocked, int SecondsRemaining) CheckLockout(string email)
    {
        var key = email.Trim().ToLowerInvariant();
        if (_attempts.TryGetValue(key, out var info) && info.LockedUntil.HasValue)
        {
            var remaining = info.LockedUntil.Value - DateTime.UtcNow;
            if (remaining.TotalSeconds > 0)
                return (true, (int)Math.Ceiling(remaining.TotalSeconds));

            // Verrouillage expiré
            _attempts.TryRemove(key, out _);
        }
        return (false, 0);
    }

    public static bool RecordFailedAttempt(string email)
    {
        var key = email.Trim().ToLowerInvariant();
        var updated = _attempts.AddOrUpdate(
            key,
            _ => (1, (DateTime?)null),
            (_, prev) =>
            {
                int next = prev.Count + 1;
                DateTime? locked = next >= MaxFailedAttempts ? DateTime.UtcNow.Add(LockoutDuration) : null;
                return (next, locked);
            });

        return updated.LockedUntil.HasValue;
    }

    public static void ResetAttempts(string email)
    {
        _attempts.TryRemove(email.Trim().ToLowerInvariant(), out _);
    }

    /// <summary>
    /// Valide la robustesse d'un nouveau mot de passe (min 8 car., 1 majuscule, 1 minuscule, 1 chiffre).
    /// </summary>
    public static (bool IsValid, string Message) ValidatePasswordStrength(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8)
            return (false, "Le mot de passe doit contenir au moins 8 caractères.");
        if (!Regex.IsMatch(password, "[A-Z]"))
            return (false, "Le mot de passe doit contenir au moins une lettre majuscule.");
        if (!Regex.IsMatch(password, "[a-z]"))
            return (false, "Le mot de passe doit contenir au moins une lettre minuscule.");
        if (!Regex.IsMatch(password, "[0-9]"))
            return (false, "Le mot de passe doit contenir au moins un chiffre.");
        return (true, "OK");
    }

    /// <summary>
    /// Génère un hash SHA-256 sécurisé (utilisé pour vérifier la clé de récupération administrateur).
    /// </summary>
    public static string Sha256Hex(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
