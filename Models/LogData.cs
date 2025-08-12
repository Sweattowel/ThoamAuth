namespace ThoamAuth.Models.Logs;

public enum LogStateEnum
{
    Info,
    Warning,
    Critical,
    StartUp
}
public enum LogImportance
{
    Low,
    Medium,
    High
}
public enum LogFilterData
{
    State,
    Level
}
public class Logs
{
    public int LogID { get; set; }
    public required string LogMessage { get; set; }
    public DateTime LogTime { get; set; }
    public LogStateEnum LogState { get; set; }
    public LogImportance LogImportance { get; set; }
}