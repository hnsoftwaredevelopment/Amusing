using System.ComponentModel.DataAnnotations;

namespace Amusing.Models;

public partial class UserModel : IValidatableObject
{
    public uint UserId { get; set; }

    [Required( ErrorMessage = "Gebruikersnaam mag niet leeg zijn" )]
    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    [Required( ErrorMessage = "Rol mag niet leeg zijn" )]
    public string Role { get; set; } = null!;

    public string LastLoginDate { get; set; } = "nooit";
    public IEnumerable<ValidationResult> Validate( ValidationContext validationContext )
    {
        if ( UserId == 0 && string.IsNullOrWhiteSpace( Password ) )
        {
            yield return new ValidationResult(
                "Wachtwoord mag niet leeg zijn voor een nieuwe gebruiker",
                new [ ] { nameof( Password ) } );
        }
    }
}
