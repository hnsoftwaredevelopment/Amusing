namespace Amusing.Models;

public partial class UserModel
{
    public uint UserId { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string LastLoginDate { get; set; } = "nooit";
}
