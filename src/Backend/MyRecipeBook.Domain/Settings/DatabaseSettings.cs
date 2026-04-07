using MyRecipeBook.Domain.Enums;

namespace MyRecipeBook.Domain.Settings;

public class DatabaseSettings
{
    public const string SectionName = "ConnectionStrings";

    public string DatabaseType { get; set; } = string.Empty;
    public string ConnectionMySQLServer { get; set; } = string.Empty;
    public string ConnectionSQLServer { get; set; } = string.Empty;
    public string ConnectionPostgeSQL { get; set; } = string.Empty;

    public DatabaseType GetDatabaseType()
    {
        return (DatabaseType)Enum.Parse(typeof(DatabaseType), DatabaseType);
    }

    public string GetConnectionString()
    {
        var databaseType = GetDatabaseType();

        if (databaseType == Enums.DatabaseType.PostgreSql)
            return ConnectionPostgeSQL;

        if (databaseType == Enums.DatabaseType.MySql)
            return ConnectionMySQLServer;

        return ConnectionSQLServer;
    }
}
