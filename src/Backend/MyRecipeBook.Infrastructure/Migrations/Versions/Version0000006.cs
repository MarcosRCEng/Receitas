using FluentMigrator;

namespace MyRecipeBook.Infrastructure.Migrations.Versions;

[Migration(DatabaseVersions.TABLE_OUTBOX_MESSAGES, "Create outbox messages table")]
public class Version0000006 : VersionBase
{
    public override void Up()
    {
        CreateTable("OutboxMessages")
            .WithColumn("Type").AsString(200).NotNullable()
            .WithColumn("Payload").AsString(4000).NotNullable()
            .WithColumn("ProcessedOn").AsDateTime().Nullable()
            .WithColumn("RetryCount").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("Error").AsString(4000).Nullable();
    }
}
