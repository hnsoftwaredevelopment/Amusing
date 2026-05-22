using Amusing.Models;

namespace Amusing.Helpers;

public static class AuditLogMessageBuilder
{
    private static readonly HashSet<string> ValueLessChanges = new( StringComparer.OrdinalIgnoreCase )
    {
        "logo",
        "foto",
        "photo"
    };

    public static string BuildChangeReport( string subject, PropertyChange change )
    {
        string propertyName = change.PropertyName.ToLowerInvariant();

        if ( ValueLessChanges.Contains( propertyName ) )
        {
            return $"<_userName> heeft {GetArticle( propertyName )} {propertyName} aangepast van {subject}.";
        }

        return $"<_userName> heeft {GetArticle( propertyName )} {propertyName} aangepast van {subject} van '{change.OldValue}' naar '{change.NewValue}'.";
    }

    private static string GetArticle( string propertyName )
    {
        return propertyName switch
        {
            "e-mailadres" => "het",
            "huisnummer" => "het",
            "logo" => "het",
            "mobiel nummer" => "het",
            "telefoonnummer" => "het",
            _ => "de"
        };
    }

    public static string BuildPersonName( PersonModel? person )
    {
        if ( person is null )
            return "een onbekende persoon";

        string name = string.Join(
            " ",
            new[] { person.FirstName, person.NameInfix, person.LastName }
                .Where( value => !string.IsNullOrWhiteSpace( value ) ) );

        if ( !string.IsNullOrWhiteSpace( name ) )
            return name;

        return string.IsNullOrWhiteSpace( person.Name ) ? "een onbekende persoon" : person.Name;
    }
}
