using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using ThoamAuth.Helpers.Logs;
using ThoamAuth.Helpers.SQL;
using ThoamAuth.Helpers.Notifications;
using ThoamAuth.Models.User;
using ThoamAuth.Models.RequestForms;
using ThoamAuth.Helpers.WebSockets;
using ThoamAuth.ServerPoliciesAndSettings.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace ThoamAuth.Controllers.User;

[ApiController]
[Route("/UserAuthorization")]
public class UserRoutes : ControllerBase
{

    [HttpPost("Login")]
    public async Task<IActionResult> LogIn([FromBody] UserFormData UserForm)
    {
        UserModelClass.User? VerifiedUser = await SQLHelperClass.SQLLoginCheck(UserForm.UserNameAttempt, UserForm.PasswordAttempt);

        if (VerifiedUser == null) { return Unauthorized(); };

        UserListManipulation.RegisterAndHoldUser(VerifiedUser);

        LogHelperClass.GenerateLog("User Log in", Models.Logs.LogStateEnum.Info, Models.Logs.LogImportance.Low);

        bool SignSuccess = await AuthorizationHelperClass.SignUserIn(HttpContext , VerifiedUser);

        return SignSuccess ? Ok() : Unauthorized();
    }
    [Authorize(Roles = "User")]
    [HttpPost("Logout")]
    public async Task<IActionResult> LogOut([FromBody] UserFormData UserForm)
    {
        UserModelClass.User? VerifiedUser = await SQLHelperClass.SQLLoginCheck(UserForm.UserNameAttempt, UserForm.PasswordAttempt);

        if (VerifiedUser == null) { return Unauthorized(); };

        bool LogOutSuccess = await AuthorizationHelperClass.SignUserOut(HttpContext, VerifiedUser);

        return LogOutSuccess ? Ok() : Unauthorized();
    }
    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] UserFormData UserForm)
    {
        var VerifiedUser = await SQLHelperClass.RegisterUser(UserForm.UserNameAttempt, UserForm.PasswordAttempt);
        if (VerifiedUser)
        {
            var Login = await LogIn(UserForm);
            return Login;
        };

        return Unauthorized();
    }
    [Authorize(Roles = "User")]
    [HttpGet("GetNotifications")]
    public Task<Models.Notifications.Notifications[]> GetMyNotifications([FromQuery] int UserID)
    {
        var Notifications = NotificationsHelperClass.RetrieveNotifications(UserID);

        return Notifications;
    }
    [Authorize(Roles = "User")]
    [HttpPatch("UpdateNotification")]
    public async Task<IActionResult> UpdateNotification([FromBody] int UserID, int NotificationID, Models.Notifications.NotificationState IntendedState)
    {
        bool Success = await SQLHelperClass.UpdateNotificationSql(UserID, NotificationID, IntendedState);

        return Success ? Ok() : Conflict();
    }
}
public class UserListManipulation
{
    public static ConcurrentDictionary<int, (UserModelClass.User UserData, WebSocketService WS)> ActiveUsers = [];
    public static async void RegisterAndHoldUser(UserModelClass.User UserData)
    {
        try
        {
            await using var ws = new WebSocketService("wss://echo.websocket.events", UserData);

            await ws.ConnectAsync(UserData);

            ActiveUsers.TryAdd(UserData.UserID, (UserData, ws));

            await ws.WhileUserActive();

            await ws.DisposeAsync();

            ActiveUsers.TryRemove(UserData.UserID, out _);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}