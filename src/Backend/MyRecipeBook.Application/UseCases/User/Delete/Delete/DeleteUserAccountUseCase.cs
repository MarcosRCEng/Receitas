using MyRecipeBook.Domain.Repositories;
using MyRecipeBook.Domain.Repositories.User;
using MyRecipeBook.Domain.Services.Storage;

namespace MyRecipeBook.Application.UseCases.User.Delete.Delete;
public class DeleteUserAccountUseCase : IDeleteUserAccountUseCase
{
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IUserDeleteOnlyRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBlobStorageService _blobStorageService;

    public DeleteUserAccountUseCase(
        IUserReadOnlyRepository userReadOnlyRepository,
        IUserDeleteOnlyRepository repository,
        IBlobStorageService blobStorageService,
        IUnitOfWork unitOfWork)
    {
        _userReadOnlyRepository = userReadOnlyRepository;
        _blobStorageService = blobStorageService;
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Execute(Guid userIdentifier)
    {
        var userExists = await _userReadOnlyRepository.ExistActiveUserWithIdentifier(userIdentifier);
        if (userExists.Equals(false))
            return;

        await _blobStorageService.DeleteContainer(userIdentifier);

        await _repository.DeleteAccount(userIdentifier);

        await _unitOfWork.Commit();
    }
}
