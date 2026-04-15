using Microsoft.EntityFrameworkCore;
using MyRecipeBook.Domain.Dtos;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Extensions;
using MyRecipeBook.Domain.Repositories.Recipe;

namespace MyRecipeBook.Infrastructure.DataAccess.Repositories;
public sealed class RecipeRepository : IRecipeWriteOnlyRepository, IRecipeReadOnlyRepository, IRecipeUpdateOnlyRepository
{
    private readonly MyRecipeBookDbContext _dbContext;

    public RecipeRepository(MyRecipeBookDbContext dbContext) => _dbContext = dbContext;

    public async Task Add(Recipe recipe) => await _dbContext.Recipes.AddAsync(recipe);

    public async Task<bool> Delete(long recipeId)
    {
        var recipe = await _dbContext.Recipes.FindAsync(recipeId);
        if (recipe is null)
            return false;

        _dbContext.Recipes.Remove(recipe);

        return true;
    }

    public async Task<IList<Recipe>> Filter(User user, FilterRecipesDto filters)
    {
        IQueryable<Recipe> query = ReadRecipesForUser(user)
            .Include(recipe => recipe.Ingredients);

        if (filters.Difficulties.Any())
        {
            query = query.Where(recipe => recipe.Difficulty.HasValue && filters.Difficulties.Contains(recipe.Difficulty.Value));
        }

        if (filters.CookingTimes.Any())
        {
            query = query.Where(recipe => recipe.CookingTime.HasValue && filters.CookingTimes.Contains(recipe.CookingTime.Value));
        }

        if (filters.DishTypes.Any())
        {
            query = query.Where(recipe => recipe.DishTypes.Any(dishType => filters.DishTypes.Contains(dishType.Type)));
        }

        if (filters.RecipeTitle_Ingredient.NotEmpty())
        {
            query = query.Where(
                recipe => EF.Property<string>(recipe, "_title").Contains(filters.RecipeTitle_Ingredient)
                || recipe.Ingredients.Any(ingredient => ingredient.Item.Contains(filters.RecipeTitle_Ingredient)));
        }

        return await query.ToListAsync();
    }

    async Task<Recipe?> IRecipeReadOnlyRepository.GetById(User user, long recipeId)
    {
        return await FullRecipeForRead(user)
            .FirstOrDefaultAsync(recipe => recipe.Id == recipeId);
    }

    async Task<Recipe?> IRecipeUpdateOnlyRepository.GetById(User user, long recipeId)
    {
        return await FullRecipeForUpdate(user)
            .FirstOrDefaultAsync(recipe => recipe.Id == recipeId);
    }

    public void Update(Recipe recipe) => _dbContext.Recipes.Update(recipe);

    public async Task<IList<Recipe>> GetForDashboard(User user)
    {
        return await ReadRecipesForUser(user)
            .Include(recipe => recipe.Ingredients)
            .OrderByDescending(r => r.CreatedOn)
            .Take(5)
            .ToListAsync();
    }

    private IQueryable<Recipe> FullRecipeForRead(User user) =>
        FullRecipeForUpdate(user).AsNoTracking();

    private IQueryable<Recipe> FullRecipeForUpdate(User user) =>
        ActiveRecipesForUser(user)
            .Include(recipe => recipe.Ingredients)
            .Include(recipe => recipe.Instructions)
            .Include(recipe => recipe.DishTypes);

    private IQueryable<Recipe> ReadRecipesForUser(User user) =>
        ActiveRecipesForUser(user).AsNoTracking();

    private IQueryable<Recipe> ActiveRecipesForUser(User user)
    {
        return _dbContext
            .Recipes
            .Where(recipe => recipe.Active && recipe.UserId == user.Id);
    }
}
