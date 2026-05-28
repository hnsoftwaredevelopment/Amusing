using System.Data;
using System.Xml.Linq;

using Amusing.DataReaderExtensions;
using Amusing.Helpers;
using Amusing.Models;

using ClosedXML.Excel;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Amusing.Services;

// -------------------------------------------------------
// Beschrijft één exporttabel: SQL, parameters, namen en
// eventuele split-kolommen. Dit is de single source of
// truth voor zowel XML- als Excel-export.
// -------------------------------------------------------
internal sealed record ExportTableDefinition(
    string XmlName,
    string ExcelName,
    string Sql,
    Dictionary<string, object?> Parameters,
    IEnumerable<string>? SplitColumns = null);

public class PlanningService(GenericDataService dataService)
{
    private readonly GenericDataService _dataService = dataService;

    private List<ExportTableDefinition> BuildExportDefinitions(int festivalId)
    {
        var p = new Dictionary<string, object?> { { "@FestivalId", festivalId } };

        return
        [
            new( "ah_podium_type_relaties",  "Podiumtype - relaties",   QueryDefinitions.GetPlanningStageTypeRelations,    p, ["vervangt_podium_type_id"] ),
            new( "ah_podium_genre_relaties", "Podiumgenre - relaties",  QueryDefinitions.GetPlanningStageGenreRelations,   p ),
            new( "ah_podium_koor_relaties",  "Podiumkoor - relaties",   QueryDefinitions.GetPlanningStageGroupRelations,   p ),
            new( "ah_festivals",             "Festival",                QueryDefinitions.GetPlanningFestivals,             p ),
            new( "ah_voorwaarden",           "Voorwaarden",             QueryDefinitions.GetPlanningConditions,            p ),
            new( "ah_podium_types",          "Podiumtypes",             QueryDefinitions.GetPlanningStageTypes,            p ),
            new( "ah_genres",                "Genres",                  QueryDefinitions.GetPlanningGenres,                p ),
            new( "ah_inschrijvingen",        "Inschrijvingen",          QueryDefinitions.GetPlanningRegistrations,         p ),
            new( "ah_personen_rollen",       "Personen - rollen",       QueryDefinitions.GetPlanningPersonRoles,           p ),
            new( "ah_personen",              "Personen",                QueryDefinitions.GetPlanningPersons,               p ),
            new( "ah_podia",                 "Podia",                   QueryDefinitions.GetPlanningStages,                p ),
            new( "ah_zanggroepen",           "Koren",                   QueryDefinitions.GetPlanningGroups,                p ),
            new( "ah_vrijwilligers",         "Vrijwilligers",           QueryDefinitions.GetPlanningVolunteers,            p ),
            new( "ah_optredens",             "Optredens",               QueryDefinitions.GetPlanningPerformances,          p ),
            new( "ah_vrijwilligersdiensten", "Vrijwilligersdiensten",   QueryDefinitions.GetPlanningVolunteerShifts,       p ),
            new( "ah_taken",                 "Taken",                   QueryDefinitions.GetPlanningVolunteerTasks,        p ),
            new( "ah_taakbezetting",         "Bezetting per taak",      QueryDefinitions.GetPlanningVolunteerTaskOccupancy,p ),
        ];
    }

    private async Task<List<XElement>> BuildAllTablesAsync(
        List<ExportTableDefinition> definitions,
        Func<ExportTableDefinition, string> nameSelector,
        bool skipEmpty = true)
    {
        var result = new List<XElement>();

        foreach (var def in definitions)
        {
            // Voorwaarden-tabel heeft een afwijkende opbouw (key/value)
            var tableEl = def.XmlName == "ah_voorwaarden"
                ? await BuildKeyValueTableAsync(nameSelector(def), def.Sql, def.Parameters)
                : await BuildTableElementAsync(nameSelector(def), def.Sql, def.Parameters, def.SplitColumns);

            // Sla lege tabellen over als dat gevraagd is
            if (skipEmpty && !tableEl.HasElements)
                continue;

            result.Add(tableEl);
        }

        return result;
    }

    #region Export FileName
    public async Task<string> GetExportFileName(int festivalId)
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", festivalId } };

        var result = await _dataService.ExecuteScalarAsync<string>(
            QueryDefinitions.PlanningExportFilename,
            parameters).ConfigureAwait(false);

        return result;
    }
    #endregion

    #region Festivals
    public Task<List<PlanningFestivalsModel>> GetPlanningFestivalsAsync(int festivalId)
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", festivalId } };

        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetPlanningFestivals,
            reader => new PlanningFestivalsModel
            {
                FestivalId = reader.GetMyUInt("FestivalId"),
                Festival = $"Amusing Hengelo {reader.GetMyString("Festival")}",
                FestivalDate = reader.GetMyDate("FestivalDate"),
                PerformanceLength = 30,
                StartFestivalday = reader.GetMyTime("StartFestivalday"),
                EndFestivalday = reader.GetMyTime("EndFestivalday"),
                StartPause = reader.GetMyTime("StartPause"),
                EndPause = reader.GetMyTime("EndPause"),
                EndExperiencedSubstitude = reader.GetMyTime("EndExperiencedSubstitude")
            }, parameters);
    }
    #endregion

    #region Genres
    public Task<List<PlanningGenresModel>> GetPlanningGenresAsync()
    {
        return _dataService.ExecuteQueryAsync(QueryDefinitions.GetPlanningGenres,
            reader => new PlanningGenresModel
            {
                GenreId = reader.GetMyInt("GenreId"),
                Name = reader.GetMyString("Name")
            });
    }
    #endregion

    #region Groups
    public Task<List<PlanningGroupsModel>> GetPlanningGroupsAsync()
    {
        return _dataService.ExecuteQueryAsync(QueryDefinitions.GetPlanningGroups,
            reader => new PlanningGroupsModel
            {
                GroupId = reader.GetMyUInt("GroupId"),
                Name = reader.GetMyString("Name"),
                GenreId = reader.GetMyUInt("GenreId"),
                City = reader.GetMyString("City"),
                Country = reader.GetMyString("Country")
            });
    }
    #endregion

    #region Performances
    public async Task<bool> HasPerformances(int festivalId)
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", festivalId } };

        var result = await _dataService.ExecuteQueryAsync(
            QueryDefinitions.HasPlanningPerformances,
            reader => reader.GetBoolean(reader.GetOrdinal("HasRows")),
            parameters);

        return result.FirstOrDefault(false);
    }

    public Task<List<PlanningPerformancesModel>> GetPlanningPerformancesAsync()
    {
        return _dataService.ExecuteQueryAsync(QueryDefinitions.GetPlanningPerformances,
            reader =>
            {
                var stageName = reader.GetMyString("StageName");
                var fromTime = reader.GetMyTime("From");
                var groupName = reader.GetMyString("GroupName");

                return new PlanningPerformancesModel
                {
                    FestivalId = reader.GetMyUInt("FestivalId"),
                    GroupId = reader.GetMyUInt("GroupId"),
                    GroupName = groupName,
                    TimeSlotId = reader.GetMyUInt("TimeSlotId"),
                    StageId = reader.GetMyUInt("StageId"),
                    StageName = stageName,
                    From = fromTime,
                    To = reader.GetMyTime("To"),
                    Pinned = false,
                    Description = $"{stageName}, starttijd: {fromTime:hh\\:mm}, zanggroep: {groupName}"
                };
            });
    }
    #endregion

    #region Person Roles
    public Task<List<PlanningPersonRolesModel>> GetPlanningPersonRolesAsync()
    {
        return _dataService.ExecuteQueryAsync(QueryDefinitions.GetPlanningPersonRoles,
            reader => new PlanningPersonRolesModel
            {
                PersonId = reader.GetMyUInt("PersonId"),
                PersonName = reader.GetMyString("PersonName"),
                GroupId = reader.GetMyInt("GroupId"),
                GroupName = reader.GetMyString("GroupName"),
                Role = reader.GetMyString("Role")
            });
    }
    #endregion

    #region Persons
    public Task<List<PlanningPersonsModel>> GetPlanningPersonsAsync()
    {
        return _dataService.ExecuteQueryAsync(QueryDefinitions.GetPlanningPersons,
            reader => new PlanningPersonsModel
            {
                PersonId = reader.GetMyUInt("PersonId"),
                FirstName = reader.GetMyString("FirstName"),
                Affix = reader.GetMyString("Affix"),
                Surname = reader.GetMyString("Surname"),
                Name = reader.GetMyString("Name")
            });
    }
    #endregion

    #region Registrations
    public Task<List<PlanningRegistrationsModel>> GetPlanningRegistrationsAsync(int festivalId)
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", festivalId } };

        return _dataService.ExecuteQueryAsync(QueryDefinitions.GetPlanningRegistrations,
            reader => new PlanningRegistrationsModel
            {
                FestivalId = reader.GetMyUInt("FestivalId"),
                GroupId = reader.GetMyUInt("GroupId"),
                GroupName = reader.GetMyString("GroupName"),
                Wish1 = reader.GetMyString("Wish1"),
                Wish2 = reader.GetMyString("Wish2"),
                Wish3 = reader.GetMyString("Wish3"),
                Wish4 = reader.GetMyString("Wish4"),
                Singers = reader.GetMyUInt("Singers"),
                Stagetype = reader.GetMyString("Stagetype"),
                ForcedStageChoice = reader.GetMyInt("ForcedStageChoice"),
                Registered = reader.GetMyDateTime("Registered"),
                AvailableFrom = reader.GetMyTime("AvailableFrom"),
                AvailableTill = reader.GetMyTime("AvailableTill"),
                Queue = reader.GetMyUInt("Queue"),
                InsidePerformances = reader.GetMyUInt("InsidePerformance"),
                OutsidePerformances = reader.GetMyUInt("OutsidePerformance"),
                Confirmed = reader.GetMyDateTime("Confirmed")
            }, parameters);
    }
    #endregion

    #region Stages
    public Task<List<PlanningStagesModel>> GetPlanningStagesAsync(int festivalId)
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", festivalId } };

        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetPlanningStages,
            reader => new PlanningStagesModel
            {
                PodiumId = reader.GetMyUInt("PodiumId"),
                Name = reader.GetMyString("Name"),
                PerformanceLocation = reader.GetMyString("PerformanceLocation"),
                Type = reader.GetMyString("Type"),
                Quality = reader.GetMyUInt("Quality"),
                MaxSingers = reader.GetMyUInt("MaxSingers"),
                Volunteers = reader.GetMyString("Volunteers"),
                Opening = reader.GetMyTime("Opening"),
                Closing = reader.GetMyTime("Closing"),
                VolunteersFrom = reader.GetMyTime("VolunteersFrom"),
                VolunteersTill = reader.GetMyTime("VolunteersTill"),
                MapNumber = reader.GetMyUInt("MapNumber")
            }, parameters);
    }
    #endregion

    #region StageTypes
    public Task<List<PlanningStageTypesModel>> GetPlanniningStageTypesAsync()
    {
        return _dataService.ExecuteQueryAsync(QueryDefinitions.GetPlanningStageTypes,
            reader => new PlanningStageTypesModel
            {
                TypeId = reader.GetMyInt("TypeId"),
                Type = reader.GetMyString("Type"),
                CompatibleWith = reader.GetMyString("CompatibleWith")
            });
    }
    #endregion

    #region Volunteers
    public Task<List<PlanningVolunteersModel>> GetPlanningVolunteersAsync(int festivalId)
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", festivalId } };

        return _dataService.ExecuteQueryAsync(QueryDefinitions.GetPlanningVolunteers,
            reader => new PlanningVolunteersModel
            {
                VolunteerId = reader.GetMyUInt("VolunteerId"),
                Date = reader.GetMyDateTime("Date"),
                FestivalId = reader.GetMyUInt("FestivalId"),
                PersonId = reader.GetMyUInt("PersonId"),
                PersonName = reader.GetMyString("PersonName"),
                AvailableFrom = reader.GetMyTime("AvailableFrom"),
                AvailableTill = reader.GetMyTime("AvailableTill"),
                ChainedHours = reader.GetMyUInt("ChainedHours"),
                Lunch = reader.GetMyString("Lunch"),
                Vegetarian = reader.GetMyString("Vegetarian"),
                Meeting = reader.GetMyString("Meeting"),
                Experience = reader.GetMyString("Experience"),
                StageDuty = reader.GetMyString("StageDuty"),
                Tasks = reader.GetMyString("Tasks"),
                TogetherWithId = reader.GetMyUInt("TogetherWithId"),
                TogetherWithName = reader.GetMyString("TogetherWithName"),
                PreferedStage = reader.GetMyUInt("PreferedStage"),
                DisapprovedStage = reader.GetMyUInt("DisapprovedStage"),
                PreferedGroup = reader.GetMyUInt("PreferedGroup"),
                DisapprovedGroup = reader.GetMyUInt("DisapprovedGroup"),
                PreferedTask = reader.GetMyString("PreferedTask"),
                DisapprovedTask = reader.GetMyString("DisapprovedTask"),
                Notes = reader.GetMyString("Notes")
            }, parameters);
    }
    #endregion

    #region Volunteer Tasks
    public Task<List<PlanningVolunteerTasksModel>> GetPlanningVolunteerTasksAsync()
    {
        return _dataService.ExecuteQueryAsync(QueryDefinitions.GetPlanningVolunteerTasks,
            reader => new PlanningVolunteerTasksModel
            {
                TaakId = reader.GetMyUInt("TaakId"),
                ShortName = reader.GetMyString("ShortName"),
                Name = reader.GetMyString("Name"),
                MinimumTime = reader.GetMyUInt("MinimumTime"),
                MaximumTime = reader.GetMyUInt("MaximumTime"),
                Timeslot1From = reader.GetMyTime("Timeslot1From"),
                Timeslot1Till = reader.GetMyTime("Timeslot1Till"),
                Timeslot1Volunteers = reader.GetMyInt("Timeslot1Volunteers"),
                Timeslot2From = reader.GetMyTime("Timeslot2From"),
                Timeslot2Till = reader.GetMyTime("Timeslot2Till"),
                Timeslot2Volunteers = reader.GetMyInt("Timeslot2Volunteers")
            });
    }
    #endregion

    #region Volunteers Task Occupancy
    public async Task<bool> HasPlanningVolunteerTaskOccupancy(int festivalId)
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", festivalId } };

        var result = await _dataService.ExecuteQueryAsync(
            QueryDefinitions.HasPlanningVolunteerTaskOccupancy,
            reader => reader.GetBoolean(reader.GetOrdinal("HasRows")),
            parameters);

        return result.First();
    }

    public Task<List<PlanningVolunteerTaskOccupancyModel>> GetPlanningVolunteerTaskOccupancyAsync(int festivalId)
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", festivalId } };

        return _dataService.ExecuteQueryAsync(QueryDefinitions.GetPlanningVolunteerTaskOccupancy,
            reader => new PlanningVolunteerTaskOccupancyModel
            {
                TaskId = reader.GetMyUInt("TaskId"),
                TaskName = reader.GetMyString("TaskName"),
                PersonId = reader.GetMyUInt("PersonId"),
                PersonName = reader.GetMyString("PersonName"),
                StageId = reader.GetMyInt("StageId"),
                StageName = reader.GetMyString("StageName"),
                From = reader.GetMyTime("From"),
                Till = reader.GetMyTime("Till"),
                Pinned = reader.GetMyString("Pinned")
            }, parameters);
    }
    #endregion

    #region Planning
    #region Conditions
    public Task<List<PlanningConditionsModel>> GetPlanningConditionsAsync(int festivalId)
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", festivalId } };

        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetPlanningConditionsOverview,
            reader => new PlanningConditionsModel
            {
                WishTimeBetweenPerformances = reader.GetMyInt("WishTimeBetweenPerformances"),
                MaxTimeBetweenPerformances = reader.GetMyInt("MaxTimeBetweenPerformances"),
                MaxLentgVolunteersShift = reader.GetMyInt("MaxLentgVolunteersShift"),
                PenaltyInteruptionPerformances = reader.GetMyInt("PenaltyInteruptionPerformances"),
                TasknamesWithoutSwitchTime = reader.GetMyString("TasknamesWithoutSwitchTime"),
                SubstitudeTaskName = reader.GetMyString("SubstitudeTaskName"),
                PerformanceTime = reader.GetMyInt("PerformanceTime")
            }, parameters);
    }
    #endregion

    #region Stageduty
    public Task<List<PlanningStageVolunteersModel>> GetPlanningVolunteersPerStageOverview(int festivalId)
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", festivalId } };

        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetPlanningVolunteersPerStageOverview,
            reader => new PlanningStageVolunteersModel
            {
                StageNumber = reader.GetMyInt("StageNumber"),
                StageId = reader.GetMyInt("StageId"),
                StageName = reader.GetMyString("StageName"),
                Volunteer = reader.GetMyString("Volunteer"),
                StartTime = reader.IsDBNull("StartTime") ? null : reader.GetMyTime("StartTime"),
                EndTime = reader.IsDBNull("EndTime") ? null : reader.GetMyTime("EndTime")
            }, parameters);
    }
    #endregion

    #region Other duty
    public Task<List<PlanningOtherVolunteerTasksModel>> GetPlanningOtherVolunteerTasksOverview(int festivalId)
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", festivalId } };

        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetPlanningOtherVolunteerTasksOverview,
            reader => new PlanningOtherVolunteerTasksModel
            {
                TaskName = reader.GetMyString("Task"),
                Volunteer = reader.GetMyString("Volunteer"),
                StartTime = reader.IsDBNull("StartTime") ? null : reader.GetMyTime("StartTime"),
                EndTime = reader.IsDBNull("EndTime") ? null : reader.GetMyTime("EndTime")
            }, parameters);
    }
    #endregion

    #region Volunteer overview
    public Task<List<PlanningVolunteerOverviewRow>> GetPlanningVolunteerOverviewByVolunteerAsync(int festivalId)
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", festivalId } };

        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetPlanningVolunteerOverviewByVolunteer,
            reader => new PlanningVolunteerOverviewRow
            {
                PersonId = reader.GetMyInt("PersonId"),
                Volunteer = reader.GetMyString("Volunteer"),
                Contact = reader.GetMyString("Contact"),
                StartTime = reader.GetMyTime("StartTime"),
                EndTime = reader.GetMyTime("EndTime"),
                Description = reader.GetMyString("Description"),
                Fixed = reader.GetMyString("Fixed")
            }, parameters);
    }

    public Task<List<PlanningVolunteerOverviewRow>> GetPlanningVolunteerOverviewByTaskAsync(int festivalId)
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", festivalId } };

        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetPlanningVolunteerOverviewByTask,
            reader => new PlanningVolunteerOverviewRow
            {
                GroupName = reader.GetMyString("GroupName"),
                Volunteer = reader.GetMyString("Volunteer"),
                StartTime = reader.GetMyTime("StartTime"),
                EndTime = reader.GetMyTime("EndTime"),
                Fixed = reader.GetMyString("Fixed")
            }, parameters);
    }

    public Task<List<PlanningVolunteerOverviewRow>> GetPlanningVolunteerOverviewByStageAsync(int festivalId)
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", festivalId } };

        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetPlanningVolunteerOverviewByStage,
            reader => new PlanningVolunteerOverviewRow
            {
                GroupName = reader.GetMyString("GroupName"),
                Volunteer = reader.GetMyString("Volunteer"),
                StartTime = reader.GetMyTime("StartTime"),
                EndTime = reader.GetMyTime("EndTime"),
                Fixed = reader.GetMyString("Fixed")
            }, parameters);
    }

    public Task<List<PlanningCalamityListRow>> GetPlanningCalamityListAsync(int festivalId)
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", festivalId } };

        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetPlanningCalamityList,
            reader => new PlanningCalamityListRow
            {
                StageName = reader.GetMyString("StageName"),
                StageNumber = reader.GetMyInt("StageNumber"),
                StartTime = reader.GetMyTime("StartTime"),
                EndTime = reader.GetMyTime("EndTime"),
                Volunteer = reader.GetMyString("Volunteer"),
                PhoneNumber = reader.GetMyString("PhoneNumber")
            }, parameters);
    }

    public async Task<byte[]> ExportCalamityListToWordAsync(int festivalId, string title)
    {
        var rows = await GetPlanningCalamityListAsync(festivalId);
        return BuildCalamityListWordDocument(title, rows);
    }

    public static byte[] BuildCalamityListWordDocument(string title, IEnumerable<PlanningCalamityListRow> rows)
    {
        using var stream = new MemoryStream();
        using (var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
        {
            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new W.Document(new W.Body());

            var body = mainPart.Document.Body!;
            body.Append(
                new W.Paragraph(
                    new W.ParagraphProperties(new W.ParagraphStyleId { Val = "Title" }),
                    new W.Run(new W.Text(title))));

            var table = new W.Table();
            table.AppendChild(new W.TableProperties(
                new W.TableBorders(
                    new W.TopBorder { Val = W.BorderValues.Single, Size = 4 },
                    new W.BottomBorder { Val = W.BorderValues.Single, Size = 4 },
                    new W.LeftBorder { Val = W.BorderValues.Single, Size = 4 },
                    new W.RightBorder { Val = W.BorderValues.Single, Size = 4 },
                    new W.InsideHorizontalBorder { Val = W.BorderValues.Single, Size = 4 },
                    new W.InsideVerticalBorder { Val = W.BorderValues.Single, Size = 4 }),
                new W.TableWidth { Width = "100%", Type = W.TableWidthUnitValues.Pct }));

            table.Append(CreateWordRow(["Podiumnaam", "Podiumnummer", "Van", "Tot", "Vrijwilliger", "Telefoonnummer"], isHeader: true));

            foreach (var row in rows)
            {
                table.Append(CreateWordRow(
                [
                    row.StageName,
                    row.StageNumber.ToString(),
                    row.StartTime.ToString("HH:mm"),
                    row.EndTime.ToString("HH:mm"),
                    row.Volunteer,
                    row.PhoneNumber
                ]));
            }

            body.Append(table);
            body.Append(new W.SectionProperties(
                new W.PageSize { Width = 16838, Height = 11906, Orient = W.PageOrientationValues.Landscape },
                new W.PageMargin { Top = 720, Right = 720, Bottom = 720, Left = 720 }));

            mainPart.Document.Save();
        }

        return stream.ToArray();
    }

    private static W.TableRow CreateWordRow(IEnumerable<string> values, bool isHeader = false)
    {
        var row = new W.TableRow();

        foreach (var value in values)
        {
            var run = new W.Run(new W.Text(value ?? string.Empty));
            if (isHeader)
                run.PrependChild(new W.RunProperties(new W.Bold()));

            row.Append(new W.TableCell(
                new W.TableCellProperties(new W.TableCellWidth { Type = W.TableWidthUnitValues.Auto }),
                new W.Paragraph(run)));
        }

        return row;
    }
    #endregion

    #region Planned Performances
    public Task<List<StagePerformanceModel>> GetStagePerformancesAsync(int festivalId)
    {
        var parameters = new Dictionary<string, object> { { "@FestivalId", festivalId } };

        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetPlanningPerformancesOverview,
            reader => new StagePerformanceModel
            {
                SortOrder = reader.GetInt32("SortOrder"),
                StageId = reader.GetInt32("StageId"),
                StageName = reader.GetMyString("StageName"),
                Timeslot = reader.GetInt32("Timeslot"),
                GroupName = reader.GetMyString("GroupName")
            },
            parameters);
    }
    #endregion

    #region XML Export
    public async Task<byte[]> ExportFullPlanningToXmlAsync(int festivalId)
    {
        var definitions = BuildExportDefinitions(festivalId);
        var tables = await BuildAllTablesAsync(definitions, d => d.XmlName);

        var doc = new XDocument(new XElement("channel", tables));

        using var stream = new MemoryStream();
        doc.Save(stream);

        return stream.ToArray();
    }
    #endregion

    #region Excel Export
    public async Task<byte[]> ExportFullPlanningToExcelAsync(int festivalId)
    {
        var definitions = BuildExportDefinitions(festivalId);
        var tables = await BuildAllTablesAsync(definitions, d => d.ExcelName);

        using var workbook = new XLWorkbook();

        foreach (var table in tables)
        {
            string tableName = table.Attribute("name")?.Value ?? "Unknown";
            var ws = workbook.Worksheets.Add(tableName);

            var rows = table.Elements("row").ToList();
            if (!rows.Any())
                continue;

            var columns = rows.First().Elements().Select(e => e.Name.LocalName).ToList();

            for (int c = 0; c < columns.Count; c++)
            {
                ws.Cell(1, c + 1).Value = columns[c];
                ws.Cell(1, c + 1).Style.Font.Bold = true;
            }

            int r = 2;
            foreach (var row in rows)
            {
                for (int c = 0; c < columns.Count; c++)
                {
                    var el = row.Element(columns[c]);
                    ws.Cell(r, c + 1).Value = el?.Value ?? string.Empty;
                }
                r++;
            }

            ws.Range(1, 1, r - 1, columns.Count).CreateTable();
            ws.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return stream.ToArray();
    }
    #endregion

    #region Build Table Element
    private async Task<XElement> BuildTableElementAsync(
        string tableName,
        string sql,
        Dictionary<string, object?>? parameters = null,
        IEnumerable<string>? splitColumns = null)
    {
        var tableEl = new XElement("table", new XAttribute("name", tableName));

        if (string.IsNullOrWhiteSpace(sql))
            return tableEl;

        await _dataService.ExecuteReaderAsync(sql, async reader =>
        {
            while (await reader.ReadAsync())
            {
                var rowEl = new XElement("row");

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string field = reader.GetName(i);
                    object? value = reader.IsDBNull(i) ? null : reader.GetValue(i);

                    if (value is not null && splitColumns is not null && splitColumns.Contains(field))
                    {
                        foreach (var token in value.ToString()!
                            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                        {
                            rowEl.Add(new XElement(field, token));
                        }
                    }
                    else
                    {
                        var col = new XElement(field);
                        if (value is not null)
                            col.Value = ConvertToString(value) ?? string.Empty;
                        rowEl.Add(col);
                    }
                }

                tableEl.Add(rowEl);
            }
        }, parameters);

        return tableEl;
    }

    private async Task<XElement> BuildKeyValueTableAsync(
        string tableName,
        string sql,
        Dictionary<string, object?>? parameters)
    {
        var tableEl = new XElement("table", new XAttribute("name", tableName));

        await _dataService.ExecuteReaderAsync(sql, async reader =>
        {
            if (await reader.ReadAsync())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var colName = reader.GetName(i);
                    var colValue = reader.IsDBNull(i) ? string.Empty : reader.GetValue(i).ToString() ?? string.Empty;

                    tableEl.Add(new XElement("row",
                        new XElement("naam", colName),
                        new XElement("waarde", colValue)));
                }
            }
        }, parameters);

        return tableEl;
    }
    #endregion

    #region ConvertToString
    public static string? ConvertToString(object value) => value switch
    {
        DateTime dt => dt.ToString("yyyy-MM-dd"),
        DateOnly d => d.ToString("yyyy-MM-dd"),
        TimeSpan ts => ts.ToString("c"),
        _ => value.ToString()
    };
    #endregion
    #endregion
}
