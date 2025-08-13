using ThoamAuth.Models.User;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace ThoamAuth.ServerPoliciesAndSettings.Authorization;

public class AuthorizationHelperClass
{
    public static async Task<bool> SignUserIn(HttpContext httpContext, UserModelClass.User UserData)
    {
        ClaimsIdentity claimsIdentity = new ClaimsIdentity([], CookieAuthenticationDefaults.AuthenticationScheme);

        claimsIdentity.AddClaims([
            new Claim(ClaimTypes.Role, "User"),

            new Claim("UserID", UserData.UserID.ToString()),
            new Claim("UserName", UserData.UserName.ToString()),
            new Claim("LoginCount", UserData.LoginCount.ToString()),
            new Claim("DateMinted", DateTime.UtcNow.ToString("o"))
        ]);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(2)
        };

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties
        );

        return true;
    }
    public static async Task<bool> SignUserOut(HttpContext httpContext, UserModelClass.User UserData)
    {
        await httpContext.SignOutAsync();

        return true;
    }
    public static async Task<bool> SignAdminIn(HttpContext httpContext, UserModelClass.User UserData)
    {
        ClaimsIdentity claimsIdentity = new ClaimsIdentity([], CookieAuthenticationDefaults.AuthenticationScheme);

        claimsIdentity.AddClaims([
            new Claim(ClaimTypes.Role, "Admin"),

            new Claim("AdminID", UserData.UserID.ToString()),
            new Claim("AdminName", UserData.UserName.ToString()),
            new Claim("LoginCount", UserData.LoginCount.ToString()),
            new Claim("DateMinted", DateTime.UtcNow.ToString("o"))
        ]);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = false,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(2)
        };

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties
        );

        return true;
    }
    public static async Task<bool> SignAdminOut(HttpContext httpContext, UserModelClass.User UserData)
    {
        await httpContext.SignOutAsync();

        return true;
    }
}