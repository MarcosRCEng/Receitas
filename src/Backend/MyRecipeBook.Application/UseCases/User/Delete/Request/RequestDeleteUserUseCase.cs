using MyRecipeBook.Domain.Repositories;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Events;
using MyRecipeBook.Domain.Repositories.Outbox;
using MyRecipeBook.Domain.Repositories.User;
using MyRecipeBook.Domain.Services.LoggedUser;
using MyRecipeBook.Domain.ValueObjects;
using System.Text.Json;

namespace MyRecipeBook.Application.UseCases.User.Delete.Request;
public class RequestDeleteUserUseCase : IRequestDeleteUserUseCase
{
    private readonly IUserUpdateOnlyRepository _userUpdateRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly ILoggedUser _loggedUser;
    private readonly IUnitOfWork _unitOfWork;

    public RequestDeleteUserUseCase(
        IUserUpdateOnlyRepository userUpdateRepository,
        IOutboxRepository outboxRepository,
        ILoggedUser loggedUser,
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _outboxRepository = outboxRepository;
        _loggedUser = loggedUser;
        _userUpdateRepository = userUpdateRepository;
    }

    public async Task Execute()
    {
        var loggedUser = await _loggedUser.User();

        var user = await _userUpdateRepository.GetById(loggedUser.Id);

        user.Active = false;
        _userUpdateRepository.Update(user);

        await _outboxRepository.Add(new OutboxMessage
        {
            Type = OutboxMessageTypes.DELETE_USER_REQUESTED,
            Payload = JsonSerializer.Serialize(new DeleteUserRequestedEvent
            {
                UserIdentifier = loggedUser.UserIdentifier
            })
        });

        await _unitOfWork.Commit();
    }
}
