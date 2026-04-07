using FluentMigrator;

namespace MyRecipeBook.Infrastructure.Migrations.Versions;

[Migration(DatabaseVersions.REFRESH_TOKEN_ROTATION_AUDIT, "Add refresh token revocation and expiration audit fields")]
public class Version0000005 : VersionBase
{
    public override void Up()
    {
        Alter.Table("RefreshTokens")
            .AddColumn("ExpiresOn").AsDateTime().NotNullable().WithDefaultValue(DateTime.UtcNow)
            .AddColumn("RevokedAt").AsDateTime().Nullable();
    }
}
