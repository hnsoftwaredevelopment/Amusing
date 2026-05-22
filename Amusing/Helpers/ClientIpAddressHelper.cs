using System.Net;

using Microsoft.AspNetCore.Http;

namespace Amusing.Helpers;

public static class ClientIpAddressHelper
{
    public static string GetClientIp( HttpContext? httpContext )
    {
        if ( httpContext is null )
            return "Onbekend";

        string? forwardedIp = GetFirstHeaderValue( httpContext, "X-Forwarded-For" )
            ?? GetFirstHeaderValue( httpContext, "X-Real-IP" )
            ?? GetForwardedHeaderForValue( httpContext );

        if ( !string.IsNullOrWhiteSpace( forwardedIp ) )
            return forwardedIp;

        string? remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();

        if ( string.IsNullOrWhiteSpace( remoteIp ) || IPAddress.IsLoopback( httpContext.Connection.RemoteIpAddress! ) )
            return "127.0.0.1 (localhost)";

        return remoteIp;
    }

    private static string? GetFirstHeaderValue( HttpContext httpContext, string headerName )
    {
        if ( httpContext.Request.Headers.TryGetValue( headerName, out var headerValues ) != true )
            return null;

        string? rawValue = headerValues.FirstOrDefault();
        return rawValue?
            .Split( ',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries )
            .FirstOrDefault();
    }

    private static string? GetForwardedHeaderForValue( HttpContext httpContext )
    {
        string? forwarded = GetFirstHeaderValue( httpContext, "Forwarded" );
        if ( string.IsNullOrWhiteSpace( forwarded ) )
            return null;

        foreach ( string part in forwarded.Split( ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries ) )
        {
            if ( !part.StartsWith( "for=", StringComparison.OrdinalIgnoreCase ) )
                continue;

            return part [ 4.. ].Trim( '"' );
        }

        return null;
    }
}
