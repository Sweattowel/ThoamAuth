namespace ThoamAuth.Models.Notifications;

public enum NotificationState
{
    New,
    Seen,
    Deleted
}
public enum NotificationLevel
{
    Info,
    Warn,
    Critical
}
public class Notifications
{
    public int NotificationID { get; set; }
    public int RelevantUserID { get; set; }
    public required string NotificationMessage { get; set; }
    public NotificationState NotificationState { get; set; }
    public NotificationLevel NotificationLevel { get; set; }
    public DateTime NotificationCreatedDate { get; set; }
}