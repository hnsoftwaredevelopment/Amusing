using System.ComponentModel.DataAnnotations;

namespace Amusing.Models;

public partial class UserModel
{
    public uint UserId { get; set; }

    [Display( Name = "Gebruikersnaam" )]
    public string Username { get; set; } = null!;

    [Display( Name = "Wachtwoord" )]
    public string Password { get; set; } = null!;

    [Display( Name = "Wachtwoord" )]
    public string PasswordHash { get; set; } = null!;

    [Display( Name = "Rol" )]
    public string Role { get; set; } = null!;

    [Display( Name = "Laatst ingelogd" )]
    public string LastLoginDate { get; set; } = "nooit";
}
