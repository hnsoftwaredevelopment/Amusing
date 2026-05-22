using Amusing.Services;

using Xunit;

namespace Beheer.Tests;

public class GenericDataServiceTests
{
    [Fact]
    public void UseConfiguredDatabase_RewritesHardcodedAmusingSchema()
    {
        string sql = """
            SELECT *
            FROM amusing.ah_festivals f
            JOIN `amusing`.user_log l ON l.user_id = f.festival_id
            WHERE EXISTS (SELECT 1 FROM amusing.ah_inschrijvingen i)
            """;

        string rewritten = GenericDataService.UseConfiguredDatabase( sql, "amusingdev" );

        Assert.DoesNotContain( "amusing.", rewritten );
        Assert.DoesNotContain( "`amusing`.", rewritten );
        Assert.Contains( "`amusingdev`.ah_festivals", rewritten );
        Assert.Contains( "`amusingdev`.user_log", rewritten );
        Assert.Contains( "`amusingdev`.ah_inschrijvingen", rewritten );
    }

    [Fact]
    public void UseConfiguredDatabase_LeavesProductionSchemaUnchanged()
    {
        string sql = "INSERT INTO amusing.user_log (status) VALUES (@Status);";

        string rewritten = GenericDataService.UseConfiguredDatabase( sql, "amusing" );

        Assert.Equal( sql, rewritten );
    }
}
