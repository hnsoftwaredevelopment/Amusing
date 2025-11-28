using System.Dynamic;

namespace Amusing.Models;

public class StageScheduleRow : DynamicObject
{
    public int StageId { get; set; }

    // Echte property voor SfGrid binding
    public string StageName { get; set; } = "";

    private readonly Dictionary<string, string> _slots = new();

    public string? this [ string slot ]
    {
        get => _slots.ContainsKey( slot ) ? _slots [ slot ] : null;
        set => _slots [ slot ] = value ?? "";
    }

    public void SetSlot( string slot, string groupName )
    {
        if ( _slots.ContainsKey( slot ) && !string.IsNullOrEmpty( _slots [ slot ] ) )
            _slots [ slot ] += ", " + groupName;
        else
            _slots [ slot ] = groupName;
    }

    // Voor dynamic binding van de tijdslot-kolommen
    public override bool TryGetMember( GetMemberBinder binder, out object? result )
    {
        // Prevent dynamic override for actual properties
        if ( binder.Name == nameof( StageName ) || binder.Name == nameof( StageId ) )
        {
            result = GetType().GetProperty( binder.Name )?.GetValue( this );
            return true;
        }

        // Dynamic slot values
        result = _slots.ContainsKey( binder.Name ) ? _slots [ binder.Name ] : "";
        return true;
    }
}