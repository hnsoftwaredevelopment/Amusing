using Amusing.Helpers;

using Xunit;

namespace Beheer.Tests;

public class QueryDefinitionsTests
{
    [Fact]
    public void AddRegistrationQuery_IncludesParticipantCountAndStageType()
    {
        string query = QueryDefinitions.AddRegistration;

        Assert.Contains("aantal_deelnemers", query);
        Assert.Contains("podiumsoort", query);
        Assert.Contains("@aantalDeelnemers", query);
        Assert.Contains("@podiumsoort", query);
    }
}
