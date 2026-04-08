using AutoMapper;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Extensions;
using MyRecipeBook.Domain.Repositories;
using MyRecipeBook.Domain.Repositories.Recipe;
using MyRecipeBook.Domain.Services.LoggedUser;
using MyRecipeBook.Domain.ValueObjects;
using MyRecipeBook.Exceptions;
using MyRecipeBook.Exceptions.ExceptionsBase;

namespace MyRecipeBook.Application.UseCases.Recipe.Update;
public class UpdateRecipeUseCase : IUpdateRecipeUseCase
{
    private readonly ILoggedUser _loggedUser;
    private readonly IRecipeUpdateOnlyRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateRecipeUseCase(
        ILoggedUser loggedUser,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IRecipeUpdateOnlyRepository repository)
    {
        _loggedUser = loggedUser;
        _unitOfWork = unitOfWork;
        _repository = repository;
        _mapper = mapper;
    }

    public async Task Execute(long recipeId, RequestRecipeJson request)
    {
        Validate(request);

        var loggedUser = await _loggedUser.User();

        var recipe = await _repository.GetById(loggedUser, recipeId);

        if (recipe is null)
            throw new NotFoundException(ResourceMessagesException.RECIPE_NOT_FOUND);

        recipe.UpdateDetails(
            new RecipeTitle(request.Title),
            request.CookingTime is null ? null : (Domain.Enums.CookingTime)(int)request.CookingTime.Value,
            request.Difficulty is null ? null : (Domain.Enums.Difficulty)(int)request.Difficulty.Value);
        recipe.ReplaceIngredients(request.Ingredients);
        recipe.ReplaceDishTypes(request.DishTypes.Select(dishType => (Domain.Enums.DishType)(int)dishType));

        var instructions = request.Instructions.OrderBy(i => i.Step).ToList();
        for (var index = 0; index < instructions.Count; index++)
            instructions[index].Step = index + 1;

        recipe.ReplaceInstructions(_mapper.Map<IList<Domain.Entities.Instruction>>(instructions));

        _repository.Update(recipe);

        await _unitOfWork.Commit();
    }

    private static void Validate(RequestRecipeJson request)
    {
        var result = new RecipeValidator().Validate(request);

        if (result.IsValid.IsFalse())
            throw new ValidationException(result.Errors.Select(e => e.ErrorMessage).Distinct().ToList());
    }
}
