using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using ThoamAuth.Helpers.Logs;
using ThoamAuth.Helpers.SQL;
using ThoamAuth.Helpers.Notifications;
using ThoamAuth.Models.User;
using ThoamAuth.Models.RequestForms;
using ThoamAuth.Helpers.WebSockets;

namespace ThoamAuth.Controllers.User;

[ApiController]
[Route("/UserAuthorization")]
public class UserRoutes : ControllerBase
{

    [HttpPost("Login")]
    public async Task<IActionResult> LogIn([FromBody] UserFormData UserForm)
    {
        UserModelClass.User? VerifiedUser = await SQL.SQLLoginCheck(UserForm.UserNameAttempt, UserForm.PasswordNameAttempt);

        if (VerifiedUser == null) { return Unauthorized(); }
        ;

        UserListManipulation.RegisterAndHoldUser(VerifiedUser);
        LogHelper.GenerateLog("User Log in", Models.Logs.LogStateEnum.Info, 1);
        return Ok();
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] UserFormData UserForm)
    {
        var VerifiedUser = await SQL.RegisterUser(UserForm.UserNameAttempt, UserForm.PasswordNameAttempt);
        if (VerifiedUser)
        {
            var Login = await LogIn(UserForm);
            return Login;
        }
        ;

        return Unauthorized();
    }
    [HttpGet("GetNotifications")]
    public Task<Models.Notifications.Notifications[]> GetMyNotifications([FromQuery] int UserID)
    {
        var Notifications = Helpers.Notifications.Notifications.RetrieveNotifications(UserID);

        return Notifications;
    }
    [HttpPatch("UpdateNotification")]
    public async Task<IActionResult> UpdateNotification([FromBody] int UserID, int NotificationID, Models.Notifications.NotificationLevel IntendedLevel)
    {
        bool Success = await ThoamAuth.Helpers.SQL.SQL.UpdateNotificationSql(UserID, NotificationID, IntendedLevel);

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