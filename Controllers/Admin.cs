using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ThoamAuth.Models.User;
using ThoamAuth.Helpers.WebSockets;
using ThoamAuth.Helpers.Notifications;
using ThoamAuth.Helpers.AdminHelper;
using ThoamAuth.Controllers.User;
using ThoamAuth.Models.Notifications;
using ThoamAuth.Models.RequestForms;
using ThoamAuth.Helpers.SQL;

namespace ThoamAuth.Controllers.Admin;

public class AdminRoutes : ControllerBase
{
    [HttpPost("AdminLogin")]
    public async Task<IActionResult> AdminLogIn([FromBody] AdminFormData adminFormData)
    {
        bool CodeSuccess = AdminHelperClass.VerifySecretCode(adminFormData.SecretCode);

        if (!CodeSuccess) { return Unauthorized(); };

        UserModelClass.User ?LoginSuccess = await SQLHelperClass.SQLAdminLogin(adminFormData );

        if (LoginSuccess == null) { return Unauthorized(); };

        bool SignInSuccess = await ServerPoliciesAndSettings.Authorization.AuthorizationHelperClass.SignAdminIn(HttpContext, LoginSuccess);

        return SignInSuccess ? Ok() : Unauthorized();
    }
    
    [HttpPost("AdminLogout")]
    [HttpPost("AdjustUserState")]
    [Authorize(Roles = "Admin")]
    [HttpGet("CollectActiveUsers")]
    public (UserModelClass.User UserData, WebSocketService WS)[] GetActiveUsers()
    {
        ConcurrentDictionary<int, (UserModelClass.User UserData, WebSocketService WS)> CurrentUsers = ThoamAuth.Controllers.User.UserListManipulation.ActiveUsers;
        List<(UserModelClass.User UserData, WebSocketService WS)> Result = new List<(UserModelClass.User UserData, WebSocketService WS)>();

        foreach (var CurrUser in CurrentUsers)
        {
            CurrUser.Value.UserData.UserSalt = "";
            Result.Add((CurrUser.Value.UserData, CurrUser.Value.WS));
        }

        return [.. Result];
    }
    [Authorize(Roles = "Admin")]
    [HttpPost("MessageUsers")]
    public async Task<int> MessageActiveUsers([FromBody] Notifications NewNotification)
    {
        int Count = 0;

        foreach (var User in UserListManipulation.ActiveUsers)
        {
            await NotificationsHelperClass.CreateNotification(
                NewNotification.NotificationMessage,
                NewNotification.NotificationLevel,
                User.Value.UserData.UserID
            );
            await User.Value.WS.SendMessageAndAwaitAsync("New-Notification");
        }

        return Count;
    }
}