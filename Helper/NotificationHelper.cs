using System.Threading.Tasks;
using ThoamAuth.Routes.User;

namespace ThoamAuth.Helpers.Notifications;

public class Notifications
{
    public static async Task CreateNotification(string notificationText, Models.Notifications.NotificationLevel Level, int RelevantUserID)
    {
        if (notificationText == "") { return; }
        ;

        var newNotification = new Models.Notifications.Notifications
        {
            RelevantUserID = RelevantUserID,
            NotificationMessage = notificationText,
            NotificationState = Models.Notifications.NotificationState.New,
            NotificationLevel = Level,
            NotificationCreatedDate = DateTime.Now
        };

        //EnterNotification();

        if (UserListManipulation.ActiveUsers.TryGetValue(RelevantUserID, out var activeUser))
        {
            await activeUser.WS.SendMessageAndAwaitAsync("New Notification");
        }
    }
    public async static Task<Models.Notifications.Notifications[]> RetrieveNotifications(int UserID)
    {
        List<Models.Notifications.Notifications> UserNotifications = await SQL.SQL.GetNotifications(UserID);

        foreach (var Noti in UserNotifications)
        {
            if (Noti.RelevantUserID != UserID) { UserNotifications.Remove(Noti); }
        }
        return [.. UserNotifications];
    }
}