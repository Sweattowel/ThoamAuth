using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using ThoamAuth.ServerPoliciesAndSettings.Roles;
using ThoamAuth.ServerPoliciesAndSettings.MiddleWare;
using ThoamAuth.Helpers.AdminHelper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;

AdminHelperClass.InitCode();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddHttpsRedirection(Options =>
{
    Options.HttpsPort = 5001;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/Denied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(2);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("User", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("User");
        policy.Requirements.Add(new RoleClass.NotRoleRequirement("Admin"));
    })
    .AddPolicy("Admin", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("AdminUser");
        policy.Requirements.Add(new RoleClass.NotRoleRequirement("User"));
    });

builder.Services.AddSingleton<IAuthorizationHandler, RoleClass.NotRoleHandler>();

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.UseMiddleware<RequestMiddleware>();

app.UseWebSockets();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();


app.Run();
