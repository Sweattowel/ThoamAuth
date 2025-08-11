using System.Collections.Concurrent;
using ThoamAuth.Models.Logs;

namespace ThoamAuth.Helpers.Logs;

public class LogHelperClass
{
    public static ConcurrentBag<Models.Logs.Logs> LogList = new ConcurrentBag<Models.Logs.Logs> { };

    public static void GenerateLog(string LogMessage, Models.Logs.LogStateEnum LogState, int LogLevel)
    {
        Models.Logs.Logs newLog = new Models.Logs.Logs()
        {
            LogID = LogList.Count,
            LogMessage = LogMessage,
            LogTime = DateTime.Now,
            LogState = LogState,
            LogLevel = LogLevel
        };
        LogList.Add(newLog);
    }
    public static Models.Logs.Logs[] GetLogs(LogFilterData FilterType, Models.Logs.LogStateEnum ?WantedLogState, int ?WantedLevel)
    {
        List<Models.Logs.Logs> FilteredLogList = new List<Models.Logs.Logs>();
        
        switch (FilterType)
        {
            case LogFilterData.Level: return [.. LogList.Where(L => L.LogState == WantedLogState)];
            case LogFilterData.State: return [.. LogList.Where(L => L.LogLevel == WantedLevel)];
            default: return [];
        };
    }
}