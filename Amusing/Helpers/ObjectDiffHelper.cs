using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Amusing.Helpers;

public static class ObjectDiffHelper
{
    // Compare any two objects of the same type and return property changes
    public static List<PropertyChange> GetDifferences<T>( T original, T modified )
    {
        var changes = new List<PropertyChange>();

        if ( original == null || modified == null )
            return changes;

        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach ( var prop in props )
        {
            if ( !prop.CanRead || prop.GetIndexParameters().Length > 0 )
                continue;

            var oldValue = prop.GetValue(original);
            var newValue = prop.GetValue(modified);

            if ( Equals( oldValue, newValue ) )
                continue;

            // Get friendly display name if available
            var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
            var displayName = displayAttr?.Name ?? prop.Name;

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
}

public class PropertyChange
{
    public string PropertyName { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
}
