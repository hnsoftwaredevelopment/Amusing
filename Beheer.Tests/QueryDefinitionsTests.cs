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

    [Fact]
    public void GetPersonRolesByPersonIdQuery_LoadsRolesWithGroupsForSelectedPerson()
    {
        string query = QueryDefinitions.GetPersonRolesByPersonId;

        Assert.Contains("ah_personen_rollen", query);
        Assert.Contains("ah_zanggroepen", query);
        Assert.Contains("@PersonId", query);
    }

    [Fact]
    public void RegisterPersonForCurrentFestivalQuery_PreventsDuplicateVolunteerRegistration()
    {
        string query = QueryDefinitions.RegisterPersonForCurrentFestival;

        Assert.Contains("ah_vrijwilligers", query);
        Assert.Contains("@PersonId", query);
        Assert.Contains("@FestivalId", query);
        Assert.Contains("NOT EXISTS", query);
    }

    [Fact]
    public void UpsertPersonPasswordQuery_StoresHashForPerson()
    {
        string query = QueryDefinitions.UpsertPersonPassword;

        Assert.Contains("ah_personen_wachtwoorden", query);
        Assert.Contains("@PersonId", query);
        Assert.Contains("@Hash", query);
        Assert.Contains("ON DUPLICATE KEY UPDATE", query);
    }

    [Fact]
    public void GetGroupRegistrationsByGroupIdQuery_LoadsRegistrationsForSelectedGroup()
    {
        string query = QueryDefinitions.GetGroupRegistrationsByGroupId;

        Assert.Contains("ah_inschrijvingen", query);
        Assert.Contains("ah_festivals", query);
        Assert.Contains("@GroupId", query);
    }

    [Fact]
    public void RegisterGroupForCurrentFestivalQuery_PreventsDuplicateRegistration()
    {
        string query = QueryDefinitions.RegisterGroupForCurrentFestival;

        Assert.Contains("ah_inschrijvingen", query);
        Assert.Contains("@GroupId", query);
        Assert.Contains("@FestivalId", query);
        Assert.Contains("NOT EXISTS", query);
    }

    [Fact]
    public void GetFestivalOverviewQuery_ClosesLastDynamicYearAlias()
    {
        string query = QueryDefinitions.GetFestivalOverviewQuery(2024, 2026, filterOutOldGroups: false);

        Assert.Contains("AS `Y2026`", query);
        Assert.DoesNotContain("AS `Y2026" + Environment.NewLine + "FROM", query);
    }

    [Fact]
    public void GetFestivalOverviewQuery_DoesNotTrimDynamicColumnsByPlatformNewlineWidth()
    {
        string source = File.ReadAllText(Path.Combine(
            "..",
            "..",
            "..",
            "..",
            "Amusing",
            "Helpers",
            "QueryDefinitions.cs"));

        Assert.DoesNotContain("Length -= 3", source);
    }
}
