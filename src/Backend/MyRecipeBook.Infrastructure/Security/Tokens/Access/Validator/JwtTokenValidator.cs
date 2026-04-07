using Microsoft.IdentityModel.Tokens;
using MyRecipeBook.Domain.Security.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MyRecipeBook.Infrastructure.Security.Tokens.Access.Validator;

public class JwtTokenValidator : JwtTokenHandler, IAccessTokenValidator
{
    private readonly string _signingKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenValidator(string signingKey, string issuer, string audience)
    {
        _signingKey = signingKey;
        _issuer = issuer;
        _audience = audience;
    }

    public Guid ValidateAndGetUserIdentifier(string token)
    {
        var validationParameter = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = SecurityKey(_signingKey),
            ValidIssuer = _issuer,
            ValidAudience = _audience,
            ClockSkew = new TimeSpan(0)
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var principal = tokenHandler.ValidateToken(token, validationParameter, out _);

        var userIdentifier = principal.Claims.First(c => c.Type == ClaimTypes.Sid).Value;

        return Guid.Parse(userIdentifier);
    }
}
