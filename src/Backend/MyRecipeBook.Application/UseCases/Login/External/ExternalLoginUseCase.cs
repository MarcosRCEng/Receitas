using MyRecipeBook.Domain.Repositories;
using MyRecipeBook.Domain.Repositories.User;
using MyRecipeBook.Domain.Security.Tokens;
using MyRecipeBook.Domain.ValueObjects;

namespace MyRecipeBook.Application.UseCases.Login.External;
public class ExternalLoginUseCase : IExternalLoginUseCase
{
    private readonly IUserReadOnlyRepository _repository;
    private readonly IUserWriteOnlyRepository _repositoryWrite;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessTokenGenerator _accessTokenGenerator;

    public ExternalLoginUseCase(
        IUserReadOnlyRepository repository,
        IUserWriteOnlyRepository repositoryWrite,
        IUnitOfWork unitOfWork,
        IAccessTokenGenerator accessTokenGenerator)
    {
        _accessTokenGenerator = accessTokenGenerator;
        _repository = repository;
        _repositoryWrite = repositoryWrite;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> Execute(string name, string email)
    {
        var userEmail = new Email(email);
        var user = await _repository.GetByEmail(userEmail);

        if (user is null)
        {
            user = Domain.Entities.User.Create(name, userEmail, "-");

            await _repositoryWrite.Add(user);
            await _unitOfWork.Commit();
        }

        return _accessTokenGenerator.Generate(user.UserIdentifier);
    }
}
