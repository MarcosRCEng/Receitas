using Microsoft.IdentityModel.Tokens;
using MyRecipeBook.Domain.Security.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MyRecipeBook.Infrastructure.Security.Tokens.Access.Generator;

public class JwtTokenGenerator : JwtTokenHandler, IAccessTokenGenerator
{
    private readonly uint _expirationTimeMinutes;
    private readonly string _signingKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenGenerator(uint expirationTimeMinutes, string signingKey, string issuer, string audience)
    {
        _expirationTimeMinutes = expirationTimeMinutes;
        _signingKey = signingKey;
        _issuer = issuer;
        _audience = audience;
    }

    public string Generate(Guid userIdentifier)
    {
        var userIdentifierValue = userIdentifier.ToString();

        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, userIdentifierValue),
            new Claim(ClaimTypes.Sid, userIdentifierValue),
            new Claim(JwtRegisteredClaimNames.Sub, userIdentifierValue),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_expirationTimeMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(SecurityKey(_signingKey), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var securityToken = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(securityToken);
    }
}
