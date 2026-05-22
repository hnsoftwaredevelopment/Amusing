using System.ComponentModel.DataAnnotations;
using Amusing.Helpers;

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
    [Display( Name = "voornaam" )]
    public string? FirstName { get; set; } = "";
    [Display( Name = "tussenvoegsel" )]
    public string? NameInfix { get; set; } = "";

    [RequiredIfActive( "Active", ErrorMessage = "Achternaam is verplicht" )]
    [Display( Name = "achternaam" )]
    public string? LastName { get; set; } = "";

    [RequiredIfActive( "Active", ErrorMessage = "E-mail adres is verplicht" )]
    [EmailAddress( ErrorMessage = "E-mailadres is ongeldig" )]
    [Display( Name = "e-mailadres" )]
    public string? PersonsEmail { get; set; } = ""; // different e-mail Field for person maintenance where e-mail address is mandetory
    public string? Address { get; set; } = "";
    [Display( Name = "straat" )]
    public string? Street { get; set; } = "";
    [Display( Name = "huisnummer" )]
    public string? HomeNr { get; set; } = "";
    [Display( Name = "huisnummer toevoeging" )]
    public string? HomeNrAddition { get; set; } = "";
    [Display( Name = "postcode" )]
    public string? Zip { get; set; } = "";
    [Display( Name = "plaats" )]
    public string? City { get; set; } = "";
    [Display( Name = "mobiel nummer" )]
    public string? Mobile { get; set; } = "";
    [Display( Name = "telefoonnummer" )]
    public string? Phone { get; set; } = "";
    [Display( Name = "infomailing" )]
    public int InfoMailing { get; set; }
    public bool InfoMailingBool
    {
        get => InfoMailing == 1;
        set => InfoMailing = value ? 1 : 0;
    }
    public string? Roles { get; set; } = "";
    public string? Volunteer { get; set; } = "";
}
