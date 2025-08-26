using System.ComponentModel.DataAnnotations;

using static Amusing.Components.Pages.Maintenance_People;

namespace Amusing.Models;

public partial class PersonModel
{
    public uint PersonId { get; set; }
    public string? Name { get; set; } = "";

    [EmailAddress( ErrorMessage = "E-mailadres is ongeldig" )]
    public string? Email { get; set; } = "";

    public int Active { get; set; } = 0;
    public bool IsActive => Active == 1;
    public uint GroupId { get; set; }
    public string? Role { get; set; } = "";
    public string? GroupName { get; set; } = "";

    [RequiredIfActive( "Active", ErrorMessage = "Voornaam of voorletter(s) is verplicht" )]
    public string? FirstName { get; set; } = "";
    public string? NameInfix { get; set; } = "";

    [RequiredIfActive( "Active", ErrorMessage = "Achternaam is verplicht" )]
    public string? LastName { get; set; } = "";

    [RequiredIfActive( "Active", ErrorMessage = "E-mail adres is verplicht" )]
    [EmailAddress( ErrorMessage = "E-mailadres is ongeldig" )]
    public string? PersonsEmail { get; set; } = ""; // different e-mail field for person maintenance where e-mail address is mandetory
    public string? Address { get; set; } = "";
    public string? Street { get; set; } = "";
    public string? HomeNr { get; set; } = "";
    public string? HomeNrAddition { get; set; } = "";
    public string? Zip { get; set; } = "";
    public string? City { get; set; } = "";
    public string? Mobile { get; set; } = "";
    public string? Phone { get; set; } = "";
    public int InfoMailing { get; set; }
    public bool InfoMailingBool
    {
        get => InfoMailing == 1;
        set => InfoMailing = value ? 1 : 0;
    }
    public string? Roles { get; set; } = "";
    public string? Volunteer { get; set; } = "";
}
