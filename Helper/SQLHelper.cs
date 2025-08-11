using Microsoft.Data.SqlClient;
using DotNetEnv;
using ThoamAuth.Helpers.Encryption;
using System.Threading.Tasks;
using ThoamAuth.Models.User;
using System.Data;
namespace ThoamAuth.Helpers.SQL;

public class SQLHelperClass
{
    private static string GetConnectionString()
    {
        Env.Load();

        var server = Environment.GetEnvironmentVariable("DB_SERVER");
        var database = Environment.GetEnvironmentVariable("DB_NAME");
        var user = Environment.GetEnvironmentVariable("DB_USER");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

        var builder = new SqlConnectionStringBuilder
        {
            DataSource = server,
            InitialCatalog = database,
            UserID = user,
            Password = password,
            TrustServerCertificate = true
        };

        return builder.ConnectionString;
    }
    public static SqlConnectionStringBuilder connectionString = new SqlConnectionStringBuilder(GetConnectionString());

    public static async Task<UserModelClass.User?> SQLLoginCheck(string UserNameAttempt, string PassWordAttempt)
    {
        // UserID, UserName, UserPassword, UserSalt, State, LastLoginData, LoginCount

        string SQLquery = "SELECT * FROM UserList WHERE UserName = @UserNameAttempt";

        using var connection = new SqlConnection(connectionString.ConnectionString);
        using var command = new SqlCommand(SQLquery, connection);

        command.Parameters.AddWithValue("@UserNameAttempt", UserNameAttempt);

        connection.Open();

        await using var reader = command.ExecuteReader();

        if (!reader.Read()) { return null; };

        var StoredAccountHash = reader.GetString(reader.GetOrdinal("UserPassword"));
        var StoredAccountSalt = reader.GetString(reader.GetOrdinal("UserSalt"));

        int UserID = reader.GetInt32(reader.GetOrdinal("UserID"));
        var UserName = reader.GetString(reader.GetOrdinal("UserName"));
        var UserSalt = reader.GetString(reader.GetOrdinal("UserSalt"));
        UserModelClass.UserState State = (UserModelClass.UserState)reader.GetInt32(reader.GetOrdinal("State"));
        DateTime LastLoginData = reader.GetDateTime(reader.GetOrdinal("LastLoginData"));
        int LoginCount = reader.GetInt32(reader.GetOrdinal("LoginCount"));

        if (EncryptionHelperClass.Verify(PassWordAttempt, StoredAccountHash, StoredAccountSalt))
        {
            return new UserModelClass.User()
            {
                UserID = UserID,
                UserName = UserName,
                UserSalt = UserSalt,
                State = State,
                LastLoginData = LastLoginData,
                LoginCount = LoginCount,
            };
        }
        ;

        return null;
    }
    public static async Task<bool> RegisterUser(string UserNameAttempt, string PassWordAttempt)
    {
        string SQLqueryConflict = "SELECT COUNT(*) FROM UserList WHERE UserName = @UserNameAttempt";
        string SQLqueryEnter = "INSERT INTO UserList(UserID, UserName, UserPassword, UserSalt, State, LastLoginData, LoginCount) VALUES(@UserIDAttempt, @UserNameAttempt, @UserPasswordAttempt, @UserSaltAttempt, @StateAttempt, @LastLoginDataAttempt, @LoginCountAttempt)";

        using var connection = new SqlConnection(connectionString.ConnectionString);

        await connection.OpenAsync();

        await using (var commandConflict = new SqlCommand(SQLqueryConflict, connection))
        {
            commandConflict.Parameters.Add("@UserNameAttempt", SqlDbType.NVarChar, 255).Value = UserNameAttempt;

            var result = await commandConflict.ExecuteScalarAsync();

            var count = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);

            if (count != 0) { return false; }
            ;
        }


        string[] PassAndSalt = EncryptionHelperClass.GenNewHash(PassWordAttempt);

        await using (var commandEnter = new SqlCommand(SQLqueryEnter, connection))
        {
            commandEnter.Parameters.Add("@UserNameAttempt", SqlDbType.UniqueIdentifier).Value = UserNameAttempt;
            commandEnter.Parameters.Add("@UserPasswordAttempt", SqlDbType.NVarChar).Value = PassAndSalt[0];
            commandEnter.Parameters.Add("@UserSaltAttempt", SqlDbType.NVarChar).Value = PassAndSalt[1];
            commandEnter.Parameters.Add("@StateAttempt", SqlDbType.Int).Value = (int)UserModelClass.UserState.Inactive;
            commandEnter.Parameters.Add("@LastLoginDataAttempt", SqlDbType.DateTime).Value = DateTime.Now;
            commandEnter.Parameters.Add("@LoginCountAttempt", SqlDbType.Int).Value = 0;

            var rowsAffected = await commandEnter.ExecuteNonQueryAsync();
            return rowsAffected == 1;
        }
    }
    public static async Task<List<Models.Notifications.Notifications>> GetNotifications(int UserID)
    {
        string SQLQuery = "SELECT * FROM Notifications WHERE UserID = @UserIDAttempt";

        using var connection = new SqlConnection(connectionString.ConnectionString);

        using var command = new SqlCommand(SQLQuery, connection);

        command.Parameters.Add("@UserIDAttempt", SqlDbType.Int).Value = UserID;

        List<Models.Notifications.Notifications> notifications = [];

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            notifications.Add(new Models.Notifications.Notifications
            {
                NotificationID = reader.GetInt32(reader.GetOrdinal("@NotificationID")),
                RelevantUserID = reader.GetInt32(reader.GetOrdinal("@RelevantUserID")),
                NotificationMessage = reader.GetString(reader.GetOrdinal("@NotificationMessage")),
                NotificationState = (Models.Notifications.NotificationState)reader.GetInt32(reader.GetOrdinal("@NotificationState")),
                NotificationCreatedDate = reader.GetDateTime(reader.GetOrdinal("@NotificationCreatedDate"))
            });
        }

        return notifications;
    }
    public static async Task<bool> UpdateNotificationSql(int UserID, Models.Notifications.NotificationState NotificationStateAttempt)
    {
        string SQLQuery = "UPDATE Notifications SET NotificationState = @NotificationStateAttempt WHERE RelevantUserID = @UserIDAttempt";

        using var connection = new SqlConnection(connectionString.ConnectionString);

        await connection.OpenAsync();

        using var command = new SqlCommand(SQLQuery, connection);

        command.Parameters.Add("@NotificationStateAttempt", SqlDbType.Int).Value = (int)NotificationStateAttempt;
        command.Parameters.Add("@UserIDAttempt", SqlDbType.Int).Value = UserID;

        var Count = await command.ExecuteNonQueryAsync();

        await connection.CloseAsync();

        return Count == 1;
    }
}