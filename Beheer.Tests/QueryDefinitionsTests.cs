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

    [Fact]
    public void InsertNewStageQuery_StoresZeroVolunteersAsZero()
    {
        string query = QueryDefinitions.InsertNewStage;

        Assert.Contains("aantal_vrijwilligers", query);
        Assert.Contains("0", query);
        Assert.DoesNotContain("'geen'", query);
    }

    [Fact]
    public void ModifyStageQuery_UsesVolunteerCountParameter()
    {
        string query = QueryDefinitions.ModifyStage;

        Assert.Contains("aantal_vrijwilligers = @AantalVrijwilligers", query);
    }

    [Fact]
    public void LogErrorQuery_IncludesIpAddressParameter()
    {
        string query = QueryDefinitions.LogError;

        Assert.Contains("ip_address", query);
        Assert.Contains("@UserIp", query);
    }
}
