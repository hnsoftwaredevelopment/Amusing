using System.ComponentModel.DataAnnotations;

namespace Amusing.Models;

public class RegisterModel
{
    [Required]
    [EmailAddress]
    [Display( Name = "Email" )]
    public string Email { get; set; }

    [Required]
    [Display( Name = "Gebruikersnaam" )]
    public string Username { get; set; }

    [Required]
    [StringLength( 100, ErrorMessage = "De {0} dient minimaal {2} en maximaal {1} tekens bevatten.", MinimumLength = 6 )]
    [DataType( DataType.Password )]
    [Display( Name = "Wachtwoord" )]
    public string Password { get; set; }

    [DataType( DataType.Password )]
    [Display( Name = "Bevestig wachtwoord" )]
    [Compare( "Password", ErrorMessage = "Hety ingevoerde wachtwoord en het herhaalde wachtwoord komen niet overeen." )]
    public string ConfirmPassword { get; set; }
}
