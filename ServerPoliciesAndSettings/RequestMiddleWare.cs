using System.Collections.Concurrent;
using System.Threading.Tasks;
using ThoamAuth.Helpers.SQL;
using ThoamAuth.Helpers.Logs;
using System.Text;

namespace ThoamAuth.ServerPoliciesAndSettings.MiddleWare;

public class RequestMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HashSet<string> BlockedIps;
    private static readonly ConcurrentDictionary<string, RequestTracker> TempIpRequests = new();

    private const int RequestLimit = 10;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(10);

    public RequestMiddleware(RequestDelegate next)
    {
        _next = next;
        BlockedIps = []; //SQLHelperClass.GetBlockedIPs();
    }

    public async Task Invoke(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString();

        LogHelperClass.GenerateLog($"Request Received From IP: {ip}", Models.Logs.LogStateEnum.Info, Models.Logs.LogImportance.Low);

        if (string.IsNullOrEmpty(ip))
        {
            await _next(context);

            return;
        }
        if (BlockedIps.Contains(ip))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Access-Denied");

            return;
        }

        var tracker = TempIpRequests.GetOrAdd(ip, _ => new RequestTracker
        {
            FirstRequestTime = DateTime.UtcNow,
            Count = 0
        });

        lock (tracker)
        {
            if (DateTime.UtcNow - tracker.FirstRequestTime > Window)
            {
                tracker.FirstRequestTime = DateTime.Now;
                tracker.Count = 0;
            }

            tracker.Count++;
        }

        if (tracker.Count > RequestLimit)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;

            await context.Response.WriteAsync("Too many requests - try again later.");

            LogHelperClass.GenerateLog($"Too many requests from ip: {ip}", Models.Logs.LogStateEnum.Warning, Models.Logs.LogImportance.Medium);

            return;
        }

        // TODO Potentially add sanitization to all requests instead of manually adding everywhere? 
        /*
            Pros:
                Expansive
                consistant
                Human error is removed from new queries
                Easy Maintenance
            Cons:
                Not customisable for instances
                if one area is broken all areas are broken
        */

        await _next(context);
    }

    private class RequestTracker
    {
        public DateTime FirstRequestTime { get; set; }
        public int Count { get; set; }
    }
}
public class ResponseMiddleware
{
    private static readonly Dictionary<string, HashSet<string>> WhiteList = new()
    {
        ["Test"] = new HashSet<string> { "Goon" },
    };
    private readonly RequestDelegate _Next;
    public ResponseMiddleware(RequestDelegate Next)
    {
        _Next = Next;
    }
    public async Task Invoke(HttpContext context)
    {
        await _Next(context);
    }
}