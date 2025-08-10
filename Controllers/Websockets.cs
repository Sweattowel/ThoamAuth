using System.Net.WebSockets;
using System.Text;
using System.Text.Unicode;
using Microsoft.AspNetCore.Mvc;
using ThoamAuth.Models.User;

namespace ThoamAuth.Routes.WebSockets;

public class WebSocketService : IAsyncDisposable
{
    private readonly ClientWebSocket _websocket;
    private readonly Uri _uri;
    public WebSocketService(string uri, UserModelClass.User userData)
    {
        if (string.IsNullOrWhiteSpace(uri)) { throw new ArgumentException("WebSocket URI is required", nameof(uri)); };
        if (userData == null) { throw new ArgumentException("Failed UserData not present", nameof(userData)); };

        _websocket = new ClientWebSocket();
        _uri = new Uri(uri);
    }
    public async Task ConnectAsync(UserModelClass.User user)
    {
        await _websocket.ConnectAsync(_uri, CancellationToken.None);
        Console.WriteLine($"[WebSocket] Connected to {_uri}, User {user.UserID}:{user.UserName}");
    }
    public async Task<bool> WhileUserActive(int maxPings = 15)
    {
        int _pingCounter = 0;

        while (_websocket.State == WebSocketState.Open && _pingCounter < maxPings)
        {
            var ping = Encoding.UTF8.GetBytes("VerifyConnection");
            
            await _websocket.SendAsync(new ArraySegment<byte>(ping), WebSocketMessageType.Text, true, CancellationToken.None);

            Console.WriteLine($"Ping No*{_pingCounter} sent");

            _pingCounter++;

            await Task.Delay(5000);
        }
        return false;
    }
    public async Task<string> SendMessageAndAwaitAsync(string message)
    {
        var Message = Encoding.UTF8.GetBytes(message);

        await _websocket.SendAsync(new ArraySegment<byte>(Message), WebSocketMessageType.Text, true, CancellationToken.None);

        var buffer = new Byte[1024];
        var response = await _websocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        return Encoding.UTF8.GetString(buffer, 0, response.Count);
    }
    public async ValueTask DisposeAsync()
    {
        if (_websocket.State == WebSocketState.Open)
        {
            Console.WriteLine("Closing connection");
            await _websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing WebSocket", CancellationToken.None);
        }
        _websocket.Dispose();
    }
}