using System.ComponentModel.DataAnnotations;

namespace Amusing.Models;

public class LoginModel
{
    [Required( ErrorMessage = "Gebruikersnaam is verplicht" )]
    public string Username { get; set; } = string.Empty;

    [Required( ErrorMessage = "Wachtwoord is verplicht" )]
    public string Password { get; set; } = string.Empty;

    public int UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    //public bool RememberMe { get; set; }
    //public bool IsAuthenticated { get; set; }
}
