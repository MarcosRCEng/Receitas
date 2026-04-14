using MyRecipeBook.Domain.Repositories;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Events;
using MyRecipeBook.Domain.Repositories.Outbox;
using MyRecipeBook.Domain.Repositories.User;
using MyRecipeBook.Domain.Services.LoggedUser;
using MyRecipeBook.Domain.ValueObjects;
using MyRecipeBook.Exceptions;
using MyRecipeBook.Exceptions.ExceptionsBase;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MyRecipeBook.Application.UseCases.User.Delete.Request;
public class RequestDeleteUserUseCase : IRequestDeleteUserUseCase
{
    private readonly IUserUpdateOnlyRepository _userUpdateRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly ILoggedUser _loggedUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RequestDeleteUserUseCase> _logger;

    public RequestDeleteUserUseCase(
        IUserUpdateOnlyRepository userUpdateRepository,
        IOutboxRepository outboxRepository,
        ILoggedUser loggedUser,
        IUnitOfWork unitOfWork,
        ILogger<RequestDeleteUserUseCase> logger)
    {
        _unitOfWork = unitOfWork;
        _outboxRepository = outboxRepository;
        _loggedUser = loggedUser;
        _userUpdateRepository = userUpdateRepository;
        _logger = logger;
    }

    public async Task Execute()
    {
        var loggedUser = await _loggedUser.User();

        var user = await _userUpdateRepository.GetById(loggedUser.Id);
        if (user is null)
            throw new UnauthorizedException(ResourceMessagesException.INVALID_SESSION);

        if (user.Active.Equals(false))
        {
            _logger.LogInformation(
                "User deletion request ignored because the user is already inactive. UserId: {UserId}. UserIdentifier: {UserIdentifier}",
                user.Id,
                user.UserIdentifier);
            return;
        }

        var requestId = Guid.NewGuid();
        var requestedOnUtc = DateTime.UtcNow;

        _logger.LogInformation(
            "User deletion requested. UserId: {UserId}. UserIdentifier: {UserIdentifier}. RequestId: {RequestId}",
            user.Id,
            user.UserIdentifier,
            requestId);

        user.Deactivate();
        _userUpdateRepository.Update(user);

        await _outboxRepository.Add(
            OutboxMessage.Create(
                OutboxMessageTypes.DELETE_USER_REQUESTED,
                JsonSerializer.Serialize(new DeleteUserRequestedEvent
                {
                    UserIdentifier = user.UserIdentifier,
                    RequestId = requestId,
                    RequestedOnUtc = requestedOnUtc
                })));

        await _unitOfWork.Commit();

        _logger.LogInformation(
            "User deletion request persisted to outbox. UserId: {UserId}. UserIdentifier: {UserIdentifier}. RequestId: {RequestId}. RequestedOnUtc: {RequestedOnUtc}",
            user.Id,
            user.UserIdentifier,
            requestId,
            requestedOnUtc);
    }
}
