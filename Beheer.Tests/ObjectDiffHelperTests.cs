using Amusing.Helpers;

using Xunit;

namespace Beheer.Tests;

public class ObjectDiffHelperTests
{
    private sealed class ImageHolder
    {
        public byte[] Logo { get; set; } = [];
    }

    [Fact]
    public void GetDifferences_DoesNotReportEqualByteArraysAsChanged()
    {
        var original = new ImageHolder { Logo = [ 1, 2, 3 ] };
        var modified = new ImageHolder { Logo = [ 1, 2, 3 ] };

        var differences = ObjectDiffHelper.GetDifferences( original, modified );

        Assert.Empty( differences );
    }

    [Fact]
    public void GetDifferences_TreatsNullAndEmptyByteArraysAsNoImage()
    {
        var original = new ImageHolder { Logo = [] };
        var modified = new ImageHolder { Logo = null! };

        var differences = ObjectDiffHelper.GetDifferences( original, modified );

        Assert.Empty( differences );
    }
}
