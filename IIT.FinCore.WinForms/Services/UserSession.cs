using IIT.FinCore.WinForms.Models;

namespace IIT.FinCore.WinForms.Services;

public static class UserSession
{
    public static Utilisateur? CurrentUser { get; private set; }
    public static HashSet<string> Permissions { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

    public static bool IsAuthenticated => CurrentUser != null;
    public static bool IsAdmin => string.Equals(CurrentUser?.RoleCode, "ADMIN", StringComparison.OrdinalIgnoreCase);
    public static bool IsTresorier => string.Equals(CurrentUser?.RoleCode, "TRESORIER", StringComparison.OrdinalIgnoreCase);
    public static bool IsConsultant => string.Equals(CurrentUser?.RoleCode, "CONSULTANT", StringComparison.OrdinalIgnoreCase);

    public static void Start(Utilisateur user, IEnumerable<string> permissions)
    {
        CurrentUser = user;
        Permissions = new HashSet<string>(permissions, StringComparer.OrdinalIgnoreCase);
    }

    public static void Clear()
    {
        CurrentUser = null;
        Permissions.Clear();
    }

    public static bool HasPermission(string code) => IsAdmin || Permissions.Contains(code);
}
