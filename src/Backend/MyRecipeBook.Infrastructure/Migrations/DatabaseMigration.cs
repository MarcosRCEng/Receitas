using Dapper;
using FluentMigrator.Runner;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Domain.Enums;
using MyRecipeBook.Domain.Extensions;
using MySqlConnector;
using FluentMigrator;

namespace MyRecipeBook.Infrastructure.Migrations;

public static class DatabaseMigration
{
    public static void Migrate(DatabaseType databaseType, string connectionString, IServiceProvider serviceProvider)
    {
        if (databaseType == DatabaseType.PostgreSql)
            EnsureDataBaseCreated_PG(connectionString);
        else
        if (databaseType == DatabaseType.MySql)
            EnsureDatabaseCreated_MySql(connectionString);
        else
            EnsureDatabaseCreated_SqlServer(connectionString);

        MigrationDatabase(serviceProvider);
    }

    public static void EnsureDataBaseCreated_PG(string connectionString)
    {
        var connectionStringBuilder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = connectionStringBuilder.Database;
        connectionStringBuilder.Remove("Database");

        using var dbConnection = new Npgsql.NpgsqlConnection(connectionStringBuilder.ConnectionString);
        var parameters = new DynamicParameters();
        parameters.Add("name", databaseName);
        var records = dbConnection.Query("SELECT datname FROM pg_database WHERE lower(datname) = lower(@name);", parameters);

        if (records.Any() == false)
            dbConnection.Execute($"CREATE DATABASE \"{databaseName}\"");
    }

    private static void EnsureDatabaseCreated_MySql(string connectionString)
    {
        var connectionStringBuilder = new MySqlConnectionStringBuilder(connectionString);

        var databaseName = connectionStringBuilder.Database;

        connectionStringBuilder.Remove("Database");

        using var dbConnection = new MySqlConnection(connectionStringBuilder.ConnectionString);

        var parameters = new DynamicParameters();
        parameters.Add("name", databaseName);

        var records = dbConnection.Query("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = @name;", parameters);

        if (records.Any().IsFalse())
            dbConnection.Execute($"CREATE DATABASE {databaseName}");
    }

    private static void EnsureDatabaseCreated_SqlServer(string connectionString)
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

        var databaseName = connectionStringBuilder.InitialCatalog;

        connectionStringBuilder.Remove("Database");

        using var dbConnection = new SqlConnection(connectionStringBuilder.ConnectionString);

        var parameters = new DynamicParameters();
        parameters.Add("name", databaseName);

        var records = dbConnection.Query("SELECT * FROM sys.databases WHERE name = @name", parameters);

        if (records.Any().IsFalse())
            dbConnection.Execute($"CREATE DATABASE {databaseName}");
    }

    private static void MigrationDatabase(IServiceProvider serviceProvider)
    {
        var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

        runner.ListMigrations();

        runner.MigrateUp();
    }
}

// C#
[Migration(2026010101)]
public class CreateUsersTable : Migration
{
    public override void Up()
    {
        // substitui criação não idempotente do schema por instrução segura
        Execute.Sql("CREATE SCHEMA IF NOT EXISTS public;");

        Create.Table("users")
            .InSchema("public")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("name").AsString(200);
    }

    public override void Down() => Delete.Table("users").InSchema("public");
}

[Migration(2026010102)]
public class CreateRecipesTable : Migration
{
    public override void Up()
    {
        Create.Table("recipes").InSchema("public")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("userid").AsGuid().NotNullable();

        Create.ForeignKey("fk_recipes_users")
            .FromTable("recipes").InSchema("public").ForeignColumn("userid")
            .ToTable("users").InSchema("public").PrimaryColumn("id");
    }

    public override void Down() => Delete.Table("recipes").InSchema("public");
}
