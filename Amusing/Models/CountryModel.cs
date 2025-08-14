namespace Amusing.Models;

public partial class CountryModel
{
    public string CountryId { get; set; } = null!;
    public string Country { get; set; } = null!;
    public int Active { get; set; }
}
