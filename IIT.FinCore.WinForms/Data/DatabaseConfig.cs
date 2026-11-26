using System.Text.Json;
using MySqlConnector;

namespace IIT.FinCore.WinForms.Data;

public static class DatabaseConfig
{
    private static string? _connectionString;

    public static string ConnectionString
    {
        get
        {
            if (!string.IsNullOrEmpty(_connectionString))
                return _connectionString;

            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (File.Exists(path))
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(path));
                if (doc.RootElement.TryGetProperty("ConnectionStrings", out var cs) &&
                    cs.TryGetProperty("MySql", out var mysql))
                {
                    _connectionString = mysql.GetString();
                }
            }

            _connectionString ??= "Server=127.0.0.1;Port=3306;Database=iit_fincore;Uid=root;Pwd=;CharSet=utf8mb4;AllowUserVariables=True;";
            return _connectionString;
        }
    }

    public static MySqlConnection CreateConnection() => new(ConnectionString);
}
