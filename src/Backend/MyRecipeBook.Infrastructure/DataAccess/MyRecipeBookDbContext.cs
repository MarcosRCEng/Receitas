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

        var recipeEntity = modelBuilder.Entity<Recipe>();

        recipeEntity
            .Ignore(recipe => recipe.Title);

        recipeEntity
            .Property<string>("_title")
            .HasColumnName("Title");

        // EF Core writes directly to backing fields so the aggregate can expose read-only collections.
        recipeEntity.Navigation(recipe => recipe.Ingredients).UsePropertyAccessMode(PropertyAccessMode.Field);
        recipeEntity.Navigation(recipe => recipe.Instructions).UsePropertyAccessMode(PropertyAccessMode.Field);
        recipeEntity.Navigation(recipe => recipe.DishTypes).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
