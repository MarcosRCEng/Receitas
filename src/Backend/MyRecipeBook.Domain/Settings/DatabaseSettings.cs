using MyRecipeBook.Domain.Enums;

namespace MyRecipeBook.Domain.Settings;

public class DatabaseSettings
{
    public const string SectionName = "Database";
    public const string LegacySectionName = "ConnectionStrings";

    public string Provider { get; set; } = string.Empty;
    public string DatabaseType { get; set; } = string.Empty;
    public string MySql { get; set; } = string.Empty;
    public string ConnectionMySQLServer { get; set; } = string.Empty;
    public string SqlServer { get; set; } = string.Empty;
    public string ConnectionSQLServer { get; set; } = string.Empty;
    public string PostgreSql { get; set; } = string.Empty;
    public string ConnectionPostgeSQL { get; set; } = string.Empty;

    public DatabaseType GetDatabaseType()
    {
        var configuredProvider = string.IsNullOrWhiteSpace(Provider)
            ? DatabaseType
            : Provider;

        if (int.TryParse(configuredProvider, out var providerNumber))
            return (DatabaseType)providerNumber;

        return Enum.Parse<DatabaseType>(configuredProvider, ignoreCase: true);
    }

    public string GetConnectionString()
    {
        var databaseType = GetDatabaseType();

        if (databaseType == Enums.DatabaseType.PostgreSql)
            return string.IsNullOrWhiteSpace(PostgreSql) ? ConnectionPostgeSQL : PostgreSql;

        if (databaseType == Enums.DatabaseType.MySql)
            return string.IsNullOrWhiteSpace(MySql) ? ConnectionMySQLServer : MySql;

        return string.IsNullOrWhiteSpace(SqlServer) ? ConnectionSQLServer : SqlServer;
    }
}
