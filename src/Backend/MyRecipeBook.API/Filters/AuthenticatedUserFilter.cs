using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MyRecipeBook.Domain.Extensions;
using MyRecipeBook.Domain.Repositories.User;
using MyRecipeBook.Domain.Security.Tokens;
using MyRecipeBook.Exceptions;
using MyRecipeBook.Exceptions.ExceptionsBase;

namespace MyRecipeBook.API.Filters;

public class AuthenticatedUserFilter : IAsyncAuthorizationFilter
{
    private readonly IUserReadOnlyRepository _repository;

    public AuthenticatedUserFilter(IUserReadOnlyRepository repository)
    {
        _repository = repository;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var userIdentifier = UserIdentifier(context);

        var exist = await _repository.ExistActiveUserWithIdentifier(userIdentifier);
        if (exist.IsFalse())
        {
            throw new ForbiddenException(ResourceMessagesException.USER_WITHOUT_PERMISSION_ACCESS_RESOURCE);
        }
    }

    private static Guid UserIdentifier(AuthorizationFilterContext context)
    {
        var identity = context.HttpContext.User.Identity;
        if (identity is null || identity.IsAuthenticated.IsFalse())
        {
            throw new UnauthorizedException(ResourceMessagesException.NO_TOKEN);
        }

        if (TokenClaimReader.TryGetUserIdentifier(context.HttpContext.User, out var userIdentifier).Equals(false))
            throw new UnauthorizedException(ResourceMessagesException.INVALID_SESSION);

        return userIdentifier;
    }
}
