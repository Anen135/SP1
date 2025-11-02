namespace SP1.Models;

public class Manager : EntityBase
{
    public string Password { get; set; } = "Admin";
    public ManagerRole Role { get; set; } = ManagerRole.Admin;
}

public enum ManagerRole
{
    Admin,
    Manager,
    Reader
}