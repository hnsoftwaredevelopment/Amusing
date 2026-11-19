namespace Amusing.Mijn.Services;

public static class ChoirRoleLabels
{
    public static string Get(string role, string language) => (role, language) switch
    {
        (ChoirPeople.FirstContactTransfer, "de") => "Übergabe der ersten Kontaktperson",
        (ChoirPeople.FirstContactTransfer, "en") => "Primary contact transfer",
        (ChoirPeople.FirstContactTransfer, _) => "Overdracht eerste contactpersoon",
        ("contactpersoon1", "de") => "Erste Kontaktperson",
        ("contactpersoon1", "en") => "Primary contact",
        ("contactpersoon2", "de") => "Zweite Kontaktperson",
        ("contactpersoon2", "en") => "Secondary contact",
        ("dirigent", "de") => "Chorleitung",
        ("dirigent", "en") => "Conductor",
        ("penningmeester", "de") => "Schatzmeister",
        ("penningmeester", "en") => "Treasurer",
        ("muzikant", "de") => "Musiker",
        ("muzikant", "en") => "Musician",
        ("zanger", "de") => "Sänger",
        ("zanger", "en") => "Singer",
        ("vrijwilliger", "de") => "Freiwilliger",
        ("vrijwilliger", "en") => "Volunteer",
        _ => ChoirPeople.Roles.GetValueOrDefault(role, role)
    };
}
