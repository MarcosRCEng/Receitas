using MyRecipeBook.Domain.Repositories;
using MyRecipeBook.Domain.Repositories.User;
using MyRecipeBook.Domain.Services.Storage;
using Microsoft.Extensions.Logging;

namespace MyRecipeBook.Application.UseCases.User.Delete.Delete;
public class DeleteUserAccountUseCase : IDeleteUserAccountUseCase
{
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IUserDeleteOnlyRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<DeleteUserAccountUseCase> _logger;

    public DeleteUserAccountUseCase(
        IUserReadOnlyRepository userReadOnlyRepository,
        IUserDeleteOnlyRepository repository,
        IBlobStorageService blobStorageService,
        IUnitOfWork unitOfWork,
        ILogger<DeleteUserAccountUseCase> logger)
    {
        _userReadOnlyRepository = userReadOnlyRepository;
        _blobStorageService = blobStorageService;
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Execute(Guid userIdentifier)
    {
        _logger.LogInformation(
            "Starting permanent user deletion. UserIdentifier: {UserIdentifier}",
            userIdentifier);

        var userExists = await _userReadOnlyRepository.ExistUserWithIdentifier(userIdentifier);
        if (userExists.Equals(false))
        {
            _logger.LogInformation(
                "Permanent user deletion skipped because the user no longer exists. UserIdentifier: {UserIdentifier}",
                userIdentifier);
            return;
        }

        await _blobStorageService.DeleteContainer(userIdentifier);

        await _repository.DeleteAccount(userIdentifier);

        await _unitOfWork.Commit();

        _logger.LogInformation(
            "Permanent user deletion completed. UserIdentifier: {UserIdentifier}",
            userIdentifier);
    }
}
