using AutoMapper;
using MyRecipeBook.Application.Extensions;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Extensions;
using MyRecipeBook.Domain.Repositories;
using MyRecipeBook.Domain.Repositories.Recipe;
using MyRecipeBook.Domain.Services.LoggedUser;
using MyRecipeBook.Domain.Services.Storage;
using MyRecipeBook.Domain.ValueObjects;
using MyRecipeBook.Exceptions;
using MyRecipeBook.Exceptions.ExceptionsBase;

namespace MyRecipeBook.Application.UseCases.Recipe.Register;
public class RegisterRecipeUseCase : IRegisterRecipeUseCase
{
    private readonly IRecipeWriteOnlyRepository _repository;
    private readonly ILoggedUser _loggedUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IBlobStorageService _blobStorageService;

    public RegisterRecipeUseCase(
        ILoggedUser loggedUser,
        IRecipeWriteOnlyRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IBlobStorageService blobStorageService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _loggedUser = loggedUser;
        _blobStorageService = blobStorageService;
    }

    public async Task<ResponseRegiteredRecipeJson> Execute(RequestRegisterRecipeFormData request)
    {
        Validate(request);

        var loggedUser = await _loggedUser.User();

        var instructions = request.Instructions.OrderBy(i => i.Step).ToList();
        for (var index = 0; index < instructions.Count; index++)
            instructions[index].Step = index + 1;

        var recipe = Domain.Entities.Recipe.Create(
            loggedUser.Id,
            new RecipeTitle(request.Title),
            request.CookingTime is null ? null : (Domain.Enums.CookingTime)(int)request.CookingTime.Value,
            request.Difficulty is null ? null : (Domain.Enums.Difficulty)(int)request.Difficulty.Value,
            request.Ingredients,
            _mapper.Map<IList<Domain.Entities.Instruction>>(instructions),
            request.DishTypes.Select(dishType => (Domain.Enums.DishType)(int)dishType));

        if(request.Image is not null)
        {
            var fileStream = request.Image.OpenReadStream();

            (var isValidImage, var extension) = fileStream.ValidateAndGetImageExtension();

            if (isValidImage.IsFalse())
            {
            throw new ValidationException([ResourceMessagesException.ONLY_IMAGES_ACCEPTED]);
            }

            recipe.SetImageIdentifier($"{Guid.NewGuid()}{extension}");
            var imageIdentifier = recipe.ImageIdentifier!;

            await _blobStorageService.Upload(loggedUser, fileStream, imageIdentifier);
        }

        await _repository.Add(recipe);

        await _unitOfWork.Commit();

        return _mapper.Map<ResponseRegiteredRecipeJson>(recipe);
    }

    private static void Validate(RequestRegisterRecipeFormData request)
    {
        var result = new RecipeValidator().Validate(request);

        if (result.IsValid.IsFalse())
            throw new ValidationException(result.Errors.Select(e => e.ErrorMessage).Distinct().ToList());
    }
}
