using Amusing.Helpers;

using Microsoft.AspNetCore.Http;

using Xunit;

namespace Beheer.Tests;

public class ClientIpAddressHelperTests
{
    [Fact]
    public void GetClientIp_UsesFirstForwardedForAddress()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse( "127.0.0.1" );
        context.Request.Headers [ "X-Forwarded-For" ] = "203.0.113.12, 10.0.0.5";

        string ip = ClientIpAddressHelper.GetClientIp( context );

        Assert.Equal( "203.0.113.12", ip );
    }

    [Fact]
    public void GetClientIp_UsesRealIpHeaderBeforeLocalhostFallback()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.IPv6Loopback;
        context.Request.Headers [ "X-Real-IP" ] = "198.51.100.7";

        string ip = ClientIpAddressHelper.GetClientIp( context );

        Assert.Equal( "198.51.100.7", ip );
    }

    [Fact]
    public void GetClientIp_ReturnsLocalhostWhenNoForwardedHeaderExists()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.IPv6Loopback;

        string ip = ClientIpAddressHelper.GetClientIp( context );

        Assert.Equal( "127.0.0.1 (localhost)", ip );
    }
}
