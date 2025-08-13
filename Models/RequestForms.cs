namespace ThoamAuth.Models.RequestForms;

public class UserFormData
{
    public required string UserNameAttempt { get; set; }
    public required string PasswordAttempt { get; set; }
}
public class AdminFormData
{
    public required string UserNameAttempt { get; set; }
    public required string PasswordAttempt { get; set; }
    public required string SecretCode { get; set; }
}