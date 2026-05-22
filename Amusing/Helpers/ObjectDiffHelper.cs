using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Amusing.Helpers;

public static class ObjectDiffHelper
{
    // Compare any two objects of the same type and return property changes
    public static List<PropertyChange> GetDifferences<T>( T original, T modified, DiffOptions? options = null )
    {
        var changes = new List<PropertyChange>();

        if ( original == null || modified == null )
            return changes;

        options ??= new DiffOptions();

        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach ( var prop in props )
        {
            if ( !prop.CanRead || prop.GetIndexParameters().Length > 0 )
                continue;

            // Skip excluded properties
            if ( options.ExcludedProperties.Contains( prop.Name ) )
                continue;

            var oldValue = prop.GetValue(original);
            var newValue = prop.GetValue(modified);

            if ( ValuesAreEqual( oldValue, newValue ) )
                continue;

            // Get friendly display name if available
            var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
            var displayName = displayAttr?.Name ?? prop.Name;

            // Handle masked properties (like Password)
            if ( options.MaskedProperties.Contains( prop.Name ) )
            {
                changes.Add( new PropertyChange
                {
                    PropertyName = displayName,
                    OldValue = "********",
                    NewValue = "********"
                } );
                continue;
            }

            // Format some types nicely
            string oldStr = FormatValue(oldValue);
            string newStr = FormatValue(newValue);

            changes.Add( new PropertyChange
            {
                PropertyName = displayName,
                OldValue = oldStr,
                NewValue = newStr
            } );
        }

        return changes;
    }

    private static string FormatValue( object? value )
    {
        return value switch
        {
            null => "(leeg)",
            DateOnly d => d.ToString( "dd-MM-yyyy" ),
            TimeOnly t => t.ToString( "HH:mm" ),
            bool b => b ? "Ja" : "Nee",
            _ => value.ToString() ?? "(leeg)"
        };
    }

    private static bool ValuesAreEqual( object? oldValue, object? newValue )
    {
        if ( IsNoByteValue( oldValue ) && IsNoByteValue( newValue ) )
            return true;

        if ( oldValue is byte[] oldBytes && newValue is byte[] newBytes )
            return oldBytes.SequenceEqual( newBytes );

        return Equals( oldValue, newValue );
    }

    private static bool IsNoByteValue( object? value )
    {
        return value is null || value is byte[] { Length: 0 };
    }
}

public class PropertyChange
{
    public string PropertyName { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
}

public class DiffOptions
{
    // Fields to completely ignore
    public List<string> ExcludedProperties { get; set; } = new();

    // Fields that can be logged as "changed" but without showing old/new values
    public List<string> MaskedProperties { get; set; } = new();
}
