using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Extensions;
using MyRecipeBook.Domain.Repositories.User;
using MyRecipeBook.Exceptions;
using MyRecipeBook.Exceptions.ExceptionsBase;
using System.Security.Claims;

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
        try
        {
            var userIdentifier = UserIdentifier(context);

            var exist = await _repository.ExistActiveUserWithIdentifier(userIdentifier);
            if (exist.IsFalse())
            {
                throw new UnauthorizedException(ResourceMessagesException.USER_WITHOUT_PERMISSION_ACCESS_RESOURCE);
            }
        }
        catch (MyRecipeBookException myRecipeBookException)
        {
            context.HttpContext.Response.StatusCode = (int)myRecipeBookException.GetStatusCode();
            context.Result = new ObjectResult(new ResponseErrorJson(myRecipeBookException.GetErrorMessages()));
        }
        catch
        {
            context.Result = new UnauthorizedObjectResult(new ResponseErrorJson(ResourceMessagesException.USER_WITHOUT_PERMISSION_ACCESS_RESOURCE));
        }
    }

    private static Guid UserIdentifier(AuthorizationFilterContext context)
    {
        var identity = context.HttpContext.User.Identity;
        if (identity is null || identity.IsAuthenticated.IsFalse())
        {
            throw new UnauthorizedException(ResourceMessagesException.NO_TOKEN);
        }

        var userIdentifier = context.HttpContext.User.Claims.First(c => c.Type == ClaimTypes.Sid).Value;

        return Guid.Parse(userIdentifier);
    }
}
