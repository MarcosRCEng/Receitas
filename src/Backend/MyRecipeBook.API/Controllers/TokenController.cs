using Microsoft.AspNetCore.Mvc;
using MyRecipeBook.Application.UseCases.Token.RefreshToken;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using Microsoft.AspNetCore.RateLimiting;

namespace MyRecipeBook.API.Controllers;

public class TokenController : MyRecipeBookBaseController
{
    [HttpPost("refresh-token")]
    [EnableRateLimiting("AuthEndpoints")]
    [ProducesResponseType(typeof(ResponseTokensJson), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RefreshToken(
        [FromServices] IUseRefreshTokenUseCase useCase,
        [FromBody] RequestNewTokenJson request)
    {
        var response = await useCase.Execute(request);

        return Ok(response);
    }
}
