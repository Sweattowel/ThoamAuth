using ThoamAuth.Models.User;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace ThoamAuth.ServerPoliciesAndSettings.Authorization;

public class AuthorizationHelperClass
{
    public static string GenerateTokenUser(UserModelClass.User UserData)
    {
        ClaimsIdentity claimsIdentity = new ClaimsIdentity();

        claimsIdentity.AddClaims([
            new Claim(ClaimTypes.Role, "User"),

            new Claim("UserID", UserData.UserID.ToString()),
            new Claim("UserName", UserData.UserName.ToString()),
            new Claim("LoginCount", UserData.LoginCount.ToString()),
            new Claim("DateMinted", DateTime.Now.ToString())
        ]);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Key"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var TokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Expires = DateTime.Now.AddHours(1),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(TokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}