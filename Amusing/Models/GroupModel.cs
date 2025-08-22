namespace Amusing.Models;

public class GroupModel
{
    public uint GroupId { get; set; }
    public string Name { get; set; } = null!;
    public uint GenreId { get; set; }
    public string Genre { get; set; } = null!;
    public string City { get; set; } = null!;
    public string CountryId { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string Website { get; set; }
    public string Email { get; set; }
    public byte [ ] Photo { get; set; }
    public byte [ ] Logo { get; set; }
    public string Description { get; set; } = null!;
    public string BankAccount { get; set; }
    public int Active { get; set; } = 0;
    public bool IsActive { get; set; }
}
