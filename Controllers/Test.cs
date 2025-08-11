using ThoamAuth.Models.User;
using ThoamAuth.Helpers.WebSockets;
using Microsoft.AspNetCore.Mvc;

namespace ThoamAuth.Controllers.Testing;
[ApiController]
[Route("/ServerTesting")]
public class Testing : ControllerBase
{
    readonly UserModelClass.User TestUser = new UserModelClass.User
    {
        UserID = -1,
        UserName = "Test",
        UserSalt = "",
        State = UserModelClass.UserState.Inactive,
        LastLoginData = null,
        LoginCount = 0,   
    };

    [HttpGet("TestWebSocket")]
    public async Task<IActionResult> TestWebSocket()
    {
        await using var ws = new WebSocketService("wss://echo.websocket.events", TestUser);

        await ws.ConnectAsync(TestUser);

        await ws.SendMessageAndAwaitAsync($"Ping 1: {DateTime.Now}");
        
        await ws.SendMessageAndAwaitAsync($"Ping 2: {DateTime.Now}");

        await ws.SendMessageAndAwaitAsync($"Ping 3: {DateTime.Now}");

        Console.WriteLine("Sent message to user");

        await ws.DisposeAsync();

        return Ok();
    }
}