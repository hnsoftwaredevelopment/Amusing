using System.ComponentModel.DataAnnotations;

namespace Amusing.Models;

public class GroupModel
{
    public uint GroupId { get; set; }

    [Display( Name = "naam" )]
    public string Name { get; set; } = null!;

    public uint GenreId { get; set; }

    [Display( Name = "genre" )]
    public string Genre { get; set; } = null!;

    [Display( Name = "plaats" )]
    public string City { get; set; } = null!;

    public string CountryId { get; set; } = null!;

    [Display( Name = "land" )]
    public string Country { get; set; } = null!;

    [Display( Name = "website" )]
    public string Website { get; set; }

    [Display( Name = "e-mailadres" )]
    public string Email { get; set; }

    [Display( Name = "foto" )]
    public byte[] Photo { get; set; }

    [Display( Name = "logo" )]
    public byte[] Logo { get; set; }

    [Display( Name = "omschrijving" )]
    public string Description { get; set; } = null!;

    [Display( Name = "bankrekening" )]
    public string BankAccount { get; set; }

    [Display( Name = "actieve status" )]
    public int Active { get; set; } = 0;

    public bool IsActive { get; set; }
}
