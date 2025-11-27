using System;
using System.Data.Common;

namespace Amusing.DataReaderExtensions;

public static class ReaderExtensions
{
    // --------------------------
    //  String
    // --------------------------
    // Reads a string safely and returns empty string if value is null.
    public static string GetMyString( this DbDataReader reader, string column )
    {
        return reader [ column ] == DBNull.Value
            ? string.Empty
            : reader [ column ].ToString() ?? string.Empty;
    }


    // --------------------------
    //  Int32
    // --------------------------
    // Reads an int safely.
    public static int GetMyInt( this DbDataReader reader, string column )
    {
        var value = reader[column];
        if ( value == DBNull.Value )
            return 0;

        return value switch
        {
            int i => i,
            long l => ( int ) l,
            string s => int.TryParse( s, out var result ) ? result : 0,
            _ => Convert.ToInt32( value )
        };
    }


    // --------------------------
    //  UInt32
    // --------------------------
    // Reads an unsigned int safely.
    public static uint GetMyUInt( this DbDataReader reader, string column )
    {
        var value = reader[column];
        if ( value == DBNull.Value )
            return 0;

        return value switch
        {
            uint ui => ui,
            int i => ( uint ) i,
            long l => ( uint ) l,
            string s => uint.TryParse( s, out var result ) ? result : 0,
            _ => Convert.ToUInt32( value )
        };
    }


    // --------------------------
    //  Boolean
    // --------------------------
    // Reads a boolean safely.
    public static bool GetMyBool( this DbDataReader reader, string column )
    {
        var value = reader[column];
        if ( value == DBNull.Value )
            return false;

        return value switch
        {
            bool b => b,
            int i => i != 0,
            string s => bool.TryParse( s, out var result ) ? result : s == "1",
            _ => Convert.ToBoolean( value )
        };
    }


    // --------------------------
    //  DateTime
    // --------------------------
    // Reads a DateTime. Accepts DATE, DATETIME, string.
    public static DateTime GetMyDateTime( this DbDataReader reader, string column )
    {
        var value = reader[column];
        if ( value == DBNull.Value )
            return default;

        return value switch
        {
            DateTime dt => dt,
            string s => DateTime.Parse( s ),
            _ => DateTime.Parse( value.ToString()! )
        };
    }


    // --------------------------
    //  DateOnly
    // --------------------------
    // Reads only the date part.
    public static DateOnly GetMyDate( this DbDataReader reader, string column )
    {
        var value = reader[column];
        if ( value == DBNull.Value )
            return default;

        if ( value is DateTime dt )
            return DateOnly.FromDateTime( dt );

        if ( value is string s )
            return DateOnly.Parse( s );

        return DateOnly.FromDateTime( DateTime.Parse( value.ToString()! ) );
    }


    // --------------------------
    //  TimeOnly
    // --------------------------
    // Reads only the time part.
    public static TimeOnly GetMyTime( this DbDataReader reader, string column )
    {
        var value = reader[column];
        if ( value == DBNull.Value )
            return default;

        if ( value is TimeSpan ts )
            return TimeOnly.FromTimeSpan( ts );

        if ( value is DateTime dt )
            return TimeOnly.FromDateTime( dt );

        if ( value is string s )
            return TimeOnly.Parse( s );

        return TimeOnly.Parse( value.ToString()! );
    }


    // --------------------------
    //  Decimal
    // --------------------------
    // Reads decimal safely from MySQL DECIMAL, DOUBLE, INT, string formats.
    public static decimal GetMyDecimal( this DbDataReader reader, string column )
    {
        var value = reader[column];
        if ( value == DBNull.Value )
            return 0m;

        return value switch
        {
            decimal d => d,
            double db => ( decimal ) db,
            float f => ( decimal ) f,
            int i => i,
            long l => l,
            string s => decimal.TryParse( s, out var result ) ? result : 0m,
            _ => Convert.ToDecimal( value )
        };
    }


    // --------------------------
    //  Double
    // --------------------------
    // Reads double safely.
    public static double GetMyDouble( this DbDataReader reader, string column )
    {
        var value = reader[column];
        if ( value == DBNull.Value )
            return 0d;

        return value switch
        {
            double d => d,
            float f => f,
            decimal dec => ( double ) dec,
            int i => i,
            long l => l,
            string s => double.TryParse( s, out var result ) ? result : 0d,
            _ => Convert.ToDouble( value )
        };
    }


    // --------------------------
    //  Enum<T>
    // --------------------------
    // Reads an enum using name OR numeric value.
    public static T GetMyEnum<T>( this DbDataReader reader, string column ) where T : struct, Enum
    {
        var value = reader[column];
        if ( value == DBNull.Value )
            return default;

        if ( value is string s )
        {
            // Try parse enum name
            if ( Enum.TryParse<T>( s, true, out var result ) )
                return result;
            // Try parse numeric string
            if ( int.TryParse( s, out var i ) )
                return ( T ) Enum.ToObject( typeof( T ), i );
        }

        // Numeric column?
        if ( value is int i2 )
            return ( T ) Enum.ToObject( typeof( T ), i2 );

        if ( value is long l )
            return ( T ) Enum.ToObject( typeof( T ), ( int ) l );

        // Fallback
        return ( T ) Enum.Parse( typeof( T ), value.ToString()!, true );
    }

    // -------------------------------------------------------
    // Converts database types to XML-safe string values
    // -------------------------------------------------------
    public static string? ConvertToString( object value )
    {
        // Dates → ISO format
        if ( value is DateTime dt )
            return dt.ToString( "yyyy-MM-dd" );

        if ( value is DateOnly d )
            return d.ToString( "yyyy-MM-dd" );

        if ( value is TimeSpan ts )
            return ts.ToString( "c" );

        return value?.ToString();
    }
}