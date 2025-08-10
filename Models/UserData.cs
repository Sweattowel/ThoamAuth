namespace ThoamAuth.Models.User;

public class UserModelClass
{
    public enum UserState
    {
        Active,
        Inactive,
        Banned,
        Suspended,
        Disabled
    }
    public class User
    {
        public int UserID { get; set; }
        public required string UserName { get; set; }
        //public required string UserPassword { get; set; }
        public required string UserSalt { get; set; }
        public UserState State { get; set; }
        public DateTime ?LastLoginData { get; set; }
        public int LoginCount { get; set; }
    }
}