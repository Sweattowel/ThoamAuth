namespace ThoamAuth.Models.RequestForms;

public class UserFormData
{
    public required string UserNameAttempt { get; set; }
    public required string PasswordNameAttempt { get; set; }
}