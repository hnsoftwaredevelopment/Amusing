namespace Amusing.Services;

public class FieldMappingService
{
    // Map from old DB tokens (with square brackets) -> internal tokens (curly braces, no spaces)
    private readonly Dictionary<string, string> _fieldMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        // Compound rules first
        { "[person.firstname] [person.prefix] [person.lastname]", "{Name}" },
        { "[person.prefix] [person.lastname]", "{Lastname}" },

        // Single person fields
        { "[person.firstname]", "{Firstname}" },
        { "[person.lastname]", "{Lastname}" },
        { "[person.email]", "{Email}" },
        { "[person.role]", "{Role}" },

        // Group fields
        { "[group.name]", "{GroupName}" },
        { "[group.email]", "{GroupEmail}" },

        // Festival / Enrollment
        { "[festival.year]", "{Festival}" },
        { "[enrollment.podiumtype]", "{StageType}" },
        { "[enrollment.review]", "{Judgement}" },
        { "[enrollment.acapellabattle]", "{SingAlong}" },
        { "[enrollment.headcount]", "{Singers}" },

        // Others
        { "[performances]", "{Performances}" },
        { "[Infomailing]", "{Infomailing}" },
        { "[Active]", "{Active}" },
        { "[GroupActive]", "{GroupActive}" },
        { "[Subscribed]", "{Subscribed}" },
        { "[Canceled]", "{Canceled}" },
        { "[Payed]", "{Payed}" },
        { "[Confirmed]", "{Confirmed}" },
        { "[Dressingroom]", "{Dressingroom}" },
        { "[Stand]", "{Stand}" },
        { "[Volunteer]", "{Volunteer}" }
    };

    // Map from internal token name (no braces) -> UI label (Dutch)
    private readonly Dictionary<string, string> _uiFieldMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Firstname", "Voornaam" },
        { "Lastname", "Achternaam" },
        { "Name", "Volledige naam" },
        { "Email", "E-mailadres" },
        { "Role", "Rol" },
        { "GroupName", "Groepsnaam" },
        { "GroupEmail", "Groep e-mail" },
        { "GroupActive", "Groep actief" },
        { "Festival", "Editie" },
        { "StageType", "Podiumtype" },
        { "Judgement", "Beoordeling" },
        { "SingAlong", "SingAlong" },
        { "Singers", "Aantal zangers" },
        { "Performances", "Optredens" },
        { "Infomailing", "Infomailing" },
        { "Active", "Actief" },
        { "Subscribed", "Ingeschreven" },
        { "Canceled", "Afgehaakt" },
        { "Payed", "Betaald" },
        { "Confirmed", "Bevestigd" },
        { "Dressingroom", "Kleedkamer" },
        { "Stand", "Stand" },
        { "Volunteer", "Vrijwilliger" }
    };

    private readonly HashSet<string> _bannedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "GroupActive",
        "Active",
        "PersonId",
        "Stand"
    };

    public FieldMappingService()
    {
        // Parameterless constructor — everything is contained.
    }

    // Normalize whitespace inside braces and square brackets to avoid mismatches
    private string NormalizeBraces( string s )
    {
        if ( string.IsNullOrEmpty( s ) )
        {
            return s;
        }
        // remove spaces after '{' and before '}', same for brackets
        return s.Replace( "{ ", "{" ).Replace( " }", "}" )
                .Replace( "[ ", "[" ).Replace( " ]", "]" );
    }

    /// <summary>
    /// Replace old DB tokens (e.g. [person.firstname]) with internal tokens
    /// (e.g. {Firstname}) and optionally translate internal tokens to Dutch UI
    /// labels (e.g. {Voornaam}).
    /// </summary>
    public string ReplaceKeysWithLabels( string input, bool translate = true )
    {
        if ( string.IsNullOrWhiteSpace( input ) )
        {
            return input;
        }

        string result = NormalizeBraces(input);

        // replace longest DB-keys first so compound rules win
        foreach ( KeyValuePair<string, string> kv in _fieldMappings.OrderByDescending( x => x.Key.Length ) )
        {
            result = result.Replace( kv.Key, kv.Value, StringComparison.OrdinalIgnoreCase );
        }

        if ( translate )
        {
            // internal -> NL label (inside braces)
            foreach ( KeyValuePair<string, string> kv in _uiFieldMappings )
            {
                string from = "{" + kv.Key + "}";
                string to = "{" + kv.Value + "}";
                result = result.Replace( from, to, StringComparison.OrdinalIgnoreCase );
            }
        }

        return result;
    }

    /// <summary>
    /// Reverse: replace UI labels (Dutch) and internal tokens back to DB
    /// tokens. Use this before saving to DB.
    /// </summary>
    public string ReplaceLabelsWithKeys( string input )
    {
        if ( string.IsNullOrWhiteSpace( input ) )
        {
            return input;
        }

        string result = NormalizeBraces(input);

        // 1) dutch UI labels -> internal tokens
        foreach ( KeyValuePair<string, string> kv in _uiFieldMappings )
        {
            string from = "{" + kv.Value + "}";
            string to = "{" + kv.Key + "}";
            result = result.Replace( from, to, StringComparison.OrdinalIgnoreCase );
        }

        // 2) internal tokens -> DB tokens
        foreach ( KeyValuePair<string, string> kv in _fieldMappings.OrderByDescending( x => x.Value.Length ) )
        {
            result = result.Replace( kv.Value, kv.Key, StringComparison.OrdinalIgnoreCase );
        }

        return result;
    }

    /// <summary>
    /// Returns a list of display labels (Dutch, wrapped in braces) for the
    /// given DB keys. The method is defensive: it accepts dbKeys that are
    /// either "person.firstname" or "[person.firstname]".
    /// </summary>
    public List<string> GetAvailableLabels( IEnumerable<string> dbKeys )
    {
        List<string> list = [];

        foreach ( string raw in dbKeys )
        {
            if ( string.IsNullOrWhiteSpace( raw ) )
            {
                continue;
            }

            // make token in bracket form to match _fieldMappings keys
            string token = raw.Trim();
            if ( !token.StartsWith( "[" ) )
            {
                token = "[" + token + "]";
            }

            // Determine internal token (e.g. {Firstname}) or fallback to {rawWithoutBrackets}
            string internalToken = _fieldMappings.ContainsKey(token)
                ? _fieldMappings[token]
                : "{" + token.Trim('[', ']').Trim() + "}";

            string internalName = internalToken.Trim('{', '}').Trim();

            if ( _bannedFields.Contains( internalName ) )
            {
                continue;
            }

            // Map to UI label if available
            string display = _uiFieldMappings.TryGetValue(internalName, out string? nlLabel )
                ? "{" + nlLabel + "}"
                : internalToken;

            list.Add( display );
        }

        return list;
    }

    /// <summary>
    /// Optional helper: translate an internal token like "{Firstname}" into
    /// Dutch label "Voornaam" or return original if no mapping.
    /// </summary>
    public string TranslateField( string internalToken )
    {
        if ( string.IsNullOrWhiteSpace( internalToken ) )
        {
            return internalToken;
        }

        string token = internalToken.Trim();
        if ( token.StartsWith( "{" ) && token.EndsWith( "}" ) )
        {
            string key = token.Trim('{', '}').Trim();
            if ( _uiFieldMappings.TryGetValue( key, out string? nl ) )
            {
                return "{" + nl + "}";
            }
        }
        return internalToken;
    }
}
