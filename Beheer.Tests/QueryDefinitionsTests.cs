using Amusing.Helpers;
using Amusing.Models;
using Amusing.Services;

using DocumentFormat.OpenXml.Packaging;

using System.Runtime.CompilerServices;

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
    public void LogPersonActionsQuery_IncludesPersonIdParameter()
    {
        string query = QueryDefinitions.LogPersonActions;

        Assert.Contains("person_id", query);
        Assert.Contains("@PersonId", query);
    }

    [Fact]
    public void LogMailingEntityQueries_IncludeEntityParameters()
    {
        Assert.Contains("template_id", QueryDefinitions.LogTemplateActions);
        Assert.Contains("@TemplateId", QueryDefinitions.LogTemplateActions);
        Assert.Contains("recipientlist_id", QueryDefinitions.LogRecipientListActions);
        Assert.Contains("@RecipientListId", QueryDefinitions.LogRecipientListActions);
    }

    [Fact]
    public void GetUsersLogQuery_LoadsUserNameAndLogbookColumns()
    {
        string query = QueryDefinitions.GetUsersLog;

        Assert.Contains("ah_beheer", query);
        Assert.Contains("UserName", query);
        Assert.Contains("Status", query);
        Assert.Contains("@FromDate", query);
        Assert.Contains("ORDER BY l.date DESC", query);
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
    public void GetNewlyAddedEmailAddressesQuery_DoesNotHardcodeDutchCountry()
    {
        string query = QueryDefinitions.GetNewlyAddedEmailAddresses;

        Assert.DoesNotContain("ah_zanggroepen.land = 'NL'", query);
        Assert.DoesNotContain("ah_zanggroepen.land = 'nl'", query);
    }

    [Fact]
    public void EmailAddressQueries_DoNotUseUnsupportedAnyValueFunction()
    {
        string[] queries =
        [
            QueryDefinitions.GetNewlyAddedEmailAddresses,
            QueryDefinitions.GetOldEmailAddresses,
            QueryDefinitions.GetPreviousEmailAddresses,
            QueryDefinitions.GetUpcommingEmailAddresses,
            QueryDefinitions.GetQueueUpcommingEmailAddresses,
            QueryDefinitions.GetIncompleteEmailAddresses,
        ];

        foreach ( string query in queries )
            Assert.DoesNotContain("any_value", query, StringComparison.OrdinalIgnoreCase);
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
        string sourcePath = GetSourcePath("Amusing", "Helpers", "QueryDefinitions.cs");
        string source = File.ReadAllText(sourcePath);

        Assert.DoesNotContain("Length -= 3", source);
    }

    [Fact]
    public void PlanningVolunteerOverviewByVolunteerQuery_LoadsServicesAndPerformances()
    {
        string query = QueryDefinitions.GetPlanningVolunteerOverviewByVolunteer;

        Assert.Contains("planner_vrijwilligersdiensten", query);
        Assert.Contains("planner_optredens", query);
        Assert.Contains("Optreden met", query);
        Assert.Contains("@FestivalId", query);
    }

    [Fact]
    public void PlanningVolunteerOverviewByTaskQuery_LoadsOnlyTaskServices()
    {
        string query = QueryDefinitions.GetPlanningVolunteerOverviewByTask;

        Assert.Contains("planner_vrijwilligersdiensten", query);
        Assert.Contains("ah_taken", query);
        Assert.Contains("vd.taak IS NOT NULL", query);
        Assert.Contains("@FestivalId", query);
    }

    [Fact]
    public void PlanningVolunteerOverviewByStageQuery_LoadsOnlyStageServices()
    {
        string query = QueryDefinitions.GetPlanningVolunteerOverviewByStage;

        Assert.Contains("planner_vrijwilligersdiensten", query);
        Assert.Contains("ah_podia", query);
        Assert.Contains("vd.taak IS NULL", query);
        Assert.Contains("@FestivalId", query);
    }

    [Fact]
    public void PlanningCalamityListQuery_LoadsStageVolunteerContactDataSortedByStageName()
    {
        string query = QueryDefinitions.GetPlanningCalamityList;

        Assert.Contains("StageName", query);
        Assert.Contains("StageNumber", query);
        Assert.Contains("StartTime", query);
        Assert.Contains("EndTime", query);
        Assert.Contains("Volunteer", query);
        Assert.Contains("PhoneNumber", query);
        Assert.Contains("planner_vrijwilligersdiensten", query);
        Assert.Contains("ah_contactgegevens", query);
        Assert.Contains("vd.taak IS NULL", query);
        Assert.Contains("@FestivalId", query);
        Assert.Contains("ORDER BY pod.naam", query);
    }

    [Fact]
    public void GetPlanningFestivalsQuery_UsesAliasesReadByPlanningService()
    {
        string query = QueryDefinitions.GetPlanningFestivals;

        Assert.Contains("f.festival_id AS FestivalId", query);
        Assert.Contains("YEAR(f.festivaldatum) AS Festival", query);
        Assert.DoesNotContain("CONCAT('Amusing Hengelo '", query);
        Assert.Contains("f.festivaldatum AS FestivalDate", query);
        Assert.Contains("f.duuroptreden AS PerformanceLength", query);
        Assert.Contains("TIME_FORMAT(f.start_festivaldag, '%H:%i') AS StartFestivalday", query);
        Assert.Contains("TIME_FORMAT(f.einde_festivaldag, '%H:%i') AS EndFestivalday", query);
        Assert.Contains("TIME_FORMAT(f.begin_pauze, '%H:%i') AS StartPause", query);
        Assert.Contains("TIME_FORMAT(f.einde_pauze, '%H:%i') AS EndPause", query);
        Assert.Contains("TIME_FORMAT(f.einde_ervaren_reserve, '%H:%i') AS EndExperiencedSubstitude", query);
    }

    [Fact]
    public void CalamityListWordExport_CreatesDocxDocument()
    {
        List<PlanningCalamityListRow> rows =
        [
            new()
            {
                StageName = "A Podium",
                StageNumber = 1,
                StartTime = new TimeOnly(11, 0),
                EndTime = new TimeOnly(12, 0),
                Volunteer = "Ada Lovelace",
                PhoneNumber = "0612345678"
            }
        ];

        byte[] bytes = PlanningService.BuildCalamityListWordDocument("Calamiteitenlijst 2026", rows);

        using var stream = new MemoryStream(bytes);
        using var document = WordprocessingDocument.Open(stream, false);
        string text = document.MainDocumentPart!.Document.Body!.InnerText;

        Assert.Equal('P', (char)bytes[0]);
        Assert.Equal('K', (char)bytes[1]);
        Assert.Contains("Calamiteitenlijst 2026", text);
        Assert.Contains("A Podium", text);
        Assert.Contains("Ada Lovelace", text);
        Assert.Contains("0612345678", text);
    }

    [Fact]
    public void ModifyChangedGridValue_RejectsUnknownFieldName()
    {
        Assert.Throws<ArgumentException>(() => QueryDefinitions.ModifyChangedGridValue("betaald = NOW() --"));
    }

    [Theory]
    [InlineData("betaald")]
    [InlineData("afgehaakt")]
    [InlineData("bevestigd")]
    [InlineData("binnen")]
    [InlineData("buiten")]
    [InlineData("wens_1")]
    [InlineData("wens_2")]
    [InlineData("wens_3")]
    [InlineData("wens_4")]
    public void ModifyChangedGridValue_AllowsKnownEditableFields(string fieldName)
    {
        string query = QueryDefinitions.ModifyChangedGridValue(fieldName);

        Assert.Contains($"SET `{fieldName}` = @value", query);
    }

    private static string GetSourcePath(params string[] pathParts)
    {
        var testDirectory = Path.GetDirectoryName(GetThisFilePath())!;
        var repositoryRoot = Directory.GetParent(testDirectory)!.FullName;

        return Path.Combine([repositoryRoot, .. pathParts]);
    }

    private static string GetThisFilePath([CallerFilePath] string path = "") => path;
}
