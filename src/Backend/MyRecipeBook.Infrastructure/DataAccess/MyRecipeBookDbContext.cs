using Microsoft.EntityFrameworkCore;
using MyRecipeBook.Domain.Entities;

namespace MyRecipeBook.Infrastructure.DataAccess;

public class MyRecipeBookDbContext : DbContext
{
    public MyRecipeBookDbContext(DbContextOptions options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyRecipeBookDbContext).Assembly);

        modelBuilder.Entity<User>()
            .Ignore(user => user.Email);

        modelBuilder.Entity<User>()
            .Property<string>("_email")
            .HasColumnName("Email");

        modelBuilder.Entity<Recipe>()
            .Ignore(recipe => recipe.Title);

        modelBuilder.Entity<Recipe>()
            .Property<string>("_title")
            .HasColumnName("Title");
    }
}
