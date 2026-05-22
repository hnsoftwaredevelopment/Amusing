using Amusing.Models;
using Amusing.Services;
using Amusing.Helpers;

using Bit.BlazorUI;

using Blazorise;

using DocumentFormat.OpenXml.Spreadsheet;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Grids.Internal;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.RichTextEditor;

using static Amusing.Components.Pages.OverviewEmailAddresses;

namespace Amusing.Components.Pages;

public partial class MaintenanceGroups : ComponentBase
{
    [Inject] protected CountryService CountryService { get; set; } = default!;
    [Inject] protected GenreService GenreService { get; set; } = default!;
    [Inject] protected GroupService GroupService { get; set; } = default!;
    [Inject] protected LoggingService LoggingService { get; set; } = default!;
    [Inject] protected PersonService PersonService { get; set; } = default!;
    [Inject] protected RegistrationService RegistrationService { get; set; } = default!;

    private bool _initialLoadDone = false;
    private bool _selectFirstRowActivePending;
    private bool _selectFirstRowUnrelatedPending;

    private bool IsDeleteEnabled => SelectedGroup?.GroupId != 0;

    private bool IsLoading = false;

    private bool IsPersonsTabEnabled => SelectedGroup?.GroupId != 0;

    private const long LogoMaxImageSize = 8 * 1024 * 1024; // 8 MB
    private const long PhotoMaxImageSize = 16 * 1024 * 1024; // 16 MB

    private GroupModel? SelectedGroup;
    private GroupModel? SelectedGroupOriginal;
    private int currentTabIndex = 0;
    private int VisibleRowCount = 0;
    private List<CountryModel> Countries = [];
    private List<GenreModel> Genres = [];
    private List<GroupModel> Groups = [];
    private List<PersonModel> ActivePersonsList = new();
    private List<PersonModel> UnrelatedPersonsList = new();
    private List<GroupRegistrationModel> GroupRegistrations = new();
    private PersonFestivalModel? LatestFestival;
    private SfGrid<GroupModel>? GridRef;
    private SfGrid<PersonModel>? ActivePersonGridRef;
    private SfGrid<PersonModel>? UnrelatedPersonGridRef;
    private SfTab? GroupTab;
    private string FileName = "Koren";
    private string? LogoUploadErrorMessage;
    private string? PhotoUploadErrorMessage;
    public List<ContextMenuItemModel>? ActivePersonsContextMenuItems;
    public List<ContextMenuItemModel>? UnrelatedPersonsContextMenuItems;

    public string SelectedGroupName
    {
        get => SelectedGroup?.Name ?? string.Empty;
        set { if (SelectedGroup != null) SelectedGroup.Name = value; }
    }

    public string SelectedGroupCity
    {
        get => SelectedGroup?.City ?? string.Empty;
        set { if (SelectedGroup != null) SelectedGroup.City = value; }
    }

    public string SelectedGroupCountry
    {
        get => SelectedGroup?.Country ?? string.Empty;
        set { if (SelectedGroup != null) SelectedGroup.Country = value; }
    }

    public string SelectedGroupBankAccount
    {
        get => SelectedGroup?.BankAccount ?? string.Empty;
        set { if (SelectedGroup != null) SelectedGroup.BankAccount = value; }
    }

    public string SelectedGroupWebsite
    {
        get => SelectedGroup?.Website ?? string.Empty;
        set { if (SelectedGroup != null) SelectedGroup.Website = value; }
    }

    public string SelectedGroupEmail
    {
        get => SelectedGroup?.Email ?? string.Empty;
        set { if (SelectedGroup != null) SelectedGroup.Email = value; }
    }

    public string SelectedGroupDescription
    {
        get => SelectedGroup?.Description ?? string.Empty;
        set { if (SelectedGroup != null) SelectedGroup.Description = value; }
    }

    public uint SelectedGroupGenreId
    {
        get => SelectedGroup?.GenreId ?? 0;
        set { if (SelectedGroup != null) SelectedGroup.GenreId = value; }
    }

    private bool IsRegisteredForLatestFestival =>
        LatestFestival is not null &&
        GroupRegistrations.Any(registration => registration.FestivalId == LatestFestival.FestivalId);

    private bool CanRegisterForLatestFestival =>
        SelectedGroup is not null &&
        SelectedGroup.GroupId != 0 &&
        LatestFestival is not null &&
        !IsRegisteredForLatestFestival;

    public async Task OnContextMenuClickActivePerson(ContextMenuClickEventArgs<PersonModel> args)
    {
        if (args.Item.Items?.Count > 0)
            return;

        var selected = args.RowInfo.RowData;
        if (args.Item.Id.StartsWith("role-"))
        {
            await PersonService.ModifyPersonRoleAsync(selected.GroupId, selected.PersonId, args.Item.Text);
            string logMessage = $"<_userName> heeft de rol van {AuditLogMessageBuilder.BuildPersonName( selected )} bij het koor {SelectedGroup?.Name} aangepast naar {args.Item.Text}.";
            await LoggingService.WriteUserActionGroupAsync( selected.GroupId, "Beheer", "Groepen", "success", logMessage );
        }
        else if (args.Item.Id.StartsWith("make"))
        {
            await PersonService.DeletePersonRoleAsync(selected.GroupId, selected.PersonId);
            string logMessage = $"<_userName> heeft {AuditLogMessageBuilder.BuildPersonName( selected )} verwijderd als relatie van het koor {SelectedGroup?.Name}.";
            await LoggingService.WriteUserActionGroupAsync( selected.GroupId, "Beheer", "Groepen", "success", logMessage );
        }

        await SetupPersonTabAsync();
    }

    public async Task OnContextMenuClickUnrelatedPerson(ContextMenuClickEventArgs<PersonModel> args)
    {
        if (args.Item.Items?.Count > 0)
            return;

        var selected = args.RowInfo.RowData;
        await PersonService.InsertNewPersonRoleAsync(SelectedGroup.GroupId, selected.PersonId, args.Item.Text);
        string logMessage = $"<_userName> heeft {AuditLogMessageBuilder.BuildPersonName( selected )} toegevoegd aan het koor {SelectedGroup.Name} met rol {args.Item.Text}.";
        await LoggingService.WriteUserActionGroupAsync( SelectedGroup.GroupId, "Beheer", "Groepen", "success", logMessage );

        await SetupPersonTabAsync();
    }

    private string? LogoPreview => SelectedGroup?.Logo != null
        ? $"data:{GetImageMimeType(SelectedGroup.Logo)};base64,{Convert.ToBase64String(SelectedGroup.Logo)}"
        : string.Empty;
    private string? PhotoPreview => SelectedGroup?.Photo != null
        ? $"data:{GetImageMimeType(SelectedGroup.Photo)};base64,{Convert.ToBase64String(SelectedGroup.Photo)}"
        : string.Empty;

    private async Task OnImageSelected(InputFileChangeEventArgs e, bool isLogo)
    {
        var file = e.File;

        if (isLogo)
        {
            if (file.Size > LogoMaxImageSize)
            {
                LogoUploadErrorMessage = $"Bestand is te groot ({file.Size / 1024 / 1024} MB). " +
                                         $"Maximaal toegestaan: {LogoMaxImageSize / 1024 / 1024} MB.";
                return;
            }
        }
        else
        {
            if (file.Size > PhotoMaxImageSize)
            {
                PhotoUploadErrorMessage = $"Bestand is te groot ({file.Size / 1024 / 1024} MB). " +
                                          $"Maximaal toegestaan: {PhotoMaxImageSize / 1024 / 1024} MB.";
                return;
            }

        }

        LogoUploadErrorMessage = null;
        PhotoUploadErrorMessage = null;

        await using var stream = file.OpenReadStream(PhotoMaxImageSize);
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);

        var buffer = ms.ToArray();

        if (isLogo)
        {
            SelectedGroup.Logo = buffer;
        }
        else
        {
            SelectedGroup.Photo = buffer;
        }
    }

    private string GetImageMimeType(byte[] imageBytes)
    {
        if (imageBytes == null || imageBytes.Length < 4)
            return "application/octet-stream"; // fallback

        // PNG: first 8 bytes: 89 50 4E 47 0D 0A 1A 0A
        if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 && imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
            return "image/png";

        // JPEG: first 2 bytes: FF D8
        if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8)
            return "image/jpeg";

        return "application/octet-stream"; // fallback
    }

    private List<ToolbarItemModel> Tools = new List<ToolbarItemModel>()
    {
        new ToolbarItemModel() { Command = ToolbarCommand.Undo },
        new ToolbarItemModel() { Command = ToolbarCommand.Redo },
        new ToolbarItemModel() { Command = ToolbarCommand.Separator },
        new ToolbarItemModel() { Command = ToolbarCommand.Bold },
        new ToolbarItemModel() { Command = ToolbarCommand.Italic },
        new ToolbarItemModel() { Command = ToolbarCommand.Underline },
        new ToolbarItemModel() { Command = ToolbarCommand.StrikeThrough },
        new ToolbarItemModel() { Command = ToolbarCommand.SuperScript },
        new ToolbarItemModel() { Command = ToolbarCommand.SubScript },
        new ToolbarItemModel() { Command = ToolbarCommand.Separator },
        new ToolbarItemModel() { Command = ToolbarCommand.FontSize },
        new ToolbarItemModel() { Command = ToolbarCommand.FontColor },
        new ToolbarItemModel() { Command = ToolbarCommand.BackgroundColor },
        new ToolbarItemModel() { Command = ToolbarCommand.Separator },
        new ToolbarItemModel() { Command = ToolbarCommand.Outdent },
        new ToolbarItemModel() { Command = ToolbarCommand.Indent }
    };

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;

        Groups = await GroupService.GetAllGroupsAsync();
        Genres = await GenreService.GetGenresAsync();
        Countries = await CountryService.GetActiveCountriesAsync();
        LatestFestival = await RegistrationService.GetLatestFestivalForMaintenanceAsync();

        SelectedGroup = Groups.FirstOrDefault();
        SelectedGroupOriginal = CloneGroup( SelectedGroup );

        IsLoading = false;
    }

    // Helper to create ContextSubmenu based on roles
    private List<MenuItem> CreateRoleMenuItems()
    {
        // Define all roles
        var roles = new[]
    {
            "contactpersoon1",
            "contactpersoon2",
            "dirigent",
            "muzikant",
            "penningmeester",
            "zanger"
        };

        // Define roles that can only exist once
        var exclusiveRoles = new HashSet<string>
        {
            "contactpersoon1",
            "contactpersoon2",
            "penningmeester",
            "dirigent"
        };

        // Collect already assigned roles
        var assignedRoles = ActivePersonsList.Select(p => p.Role).ToHashSet();

        // Build the menu items
        return roles
            .Where(r =>
                !exclusiveRoles.Contains(r) || // not exclusive, always allowed
                !assignedRoles.Contains(r))    // exclusive, but not yet assigned
            .Select(r => new MenuItem { Text = r, Id = $"role-{r}" })
            .ToList();
    }

    private async Task OnGridDataBound()
    {
        if (!_initialLoadDone && Groups?.Any() == true)
        {
            _initialLoadDone = true;
            await UpdateVisibleRowCountAsync();
            await GridRef.SelectRowAsync(0);
        }
    }

    private async Task OnActivePersonsDataBound()
    {
        if (_selectFirstRowActivePending)
        {
            _selectFirstRowActivePending = false;
            await ActivePersonGridRef.SelectRowAsync(0); // safe: data is bound
        }
    }

    private async Task OnUnrelatedPersonsDataBound()
    {
        if (_selectFirstRowUnrelatedPending)
        {
            _selectFirstRowUnrelatedPending = false;
            await UnrelatedPersonGridRef.SelectRowAsync(0); // safe: data is bound
        }
    }

    private async Task OnRowSelected(RowSelectEventArgs<GroupModel> args)
    {
        SelectedGroup = args.Data;
        SelectedGroupOriginal = CloneGroup( args.Data );


        if (currentTabIndex == 2)
        {
            await SetupPersonTabAsync();
        }
        else if (currentTabIndex == 3)
        {
            await LoadGroupRegistrationsAsync();
        }

        StateHasChanged();
    }

    private async Task UpdateVisibleRowCountAsync()
    {
        if (GridRef is not null)
        {
            var records = await GridRef.GetCurrentViewRecordsAsync();
            await Task.Delay(150);
            VisibleRowCount = records?.Count ?? 0;
            StateHasChanged();
        }
    }

    public async Task OnInput(InputEventArgs args)
    {
        await GridRef.SearchAsync(args.Value);

        await Task.Delay(50);
        await UpdateVisibleRowCountAsync();
    }

    public async Task OnPersonInput(InputEventArgs args)
    {
        await UnrelatedPersonGridRef.SearchAsync(args.Value);
    }

    private async Task SaveGroup()
    {
        if (SelectedGroup is null)
            return;

        if (SelectedGroup.GroupId != 0)
        {
            await GroupService.UpdateGroupAsync(SelectedGroup);
            await GroupService.UpdateGroupDetailsAsync(SelectedGroup);
            await LogGroupChangesAsync();
            SelectedGroupOriginal = CloneGroup( SelectedGroup );
        }
        else
        {
            // Save the new group and get the new group Id
            var savedId = await GroupService.AddGroupAsync(SelectedGroup);

            // Save the groepdetails using the GroupId
            var record = await GroupService.AddGroupDetailsAsync(SelectedGroup, savedId);

            // Refresh the list
            Groups = await GroupService.GetAllGroupsAsync();
            await GridRef.Refresh();

            // Search the modified record
            var index = Groups.FindIndex(s => s.GroupId == savedId);
            if (index >= 0)
            {
                SelectedGroup = Groups[index];
                await GridRef.SelectRowAsync(index);
            }

            string groupName = string.IsNullOrWhiteSpace( SelectedGroup?.Name ) ? "zonder naam" : SelectedGroup.Name;
            string logMessage = $"<_userName> heeft het koor {groupName} toegevoegd.";
            await LoggingService.WriteUserActionGroupAsync( savedId, "Beheer", "Groepen", "success", logMessage );
            SelectedGroupOriginal = CloneGroup( SelectedGroup );
        }
    }

    private async Task AddNewGroup()
    {
        var newGroup = new GroupModel
        {
            GroupId = 0,
            Name = string.Empty,
            Genre = string.Empty,
            City = string.Empty,
            CountryId = string.Empty,
            Country = string.Empty,
            Website = string.Empty,
            Email = string.Empty,
            Photo = Array.Empty<byte>(),
            Logo = Array.Empty<byte>(),
            Description = string.Empty,
            BankAccount = string.Empty,
            Active = 1,
            IsActive = true
        };

        await GridRef.AddRecordAsync(newGroup, 0);
        await GridRef.SelectRowAsync(0);

        SelectedGroup = newGroup;

        StateHasChanged();
    }

    private async Task DeleteGroup()
    {
        if (SelectedGroup is null)
            return;
        uint deletedGroupId = SelectedGroup.GroupId;
        string deletedGroupName = string.IsNullOrWhiteSpace( SelectedGroup.Name ) ? "zonder naam" : SelectedGroup.Name;
        await GroupService.DeleteGroupAsync(SelectedGroup.GroupId);

        var groupModels = await GroupService.GetAllGroupsAsync();

        Groups = await GroupService.GetAllGroupsAsync();
        SelectedGroup = Groups
            .OrderByDescending(f => f.Name)
            .FirstOrDefault();
        await GridRef.Refresh();

        // Select the first record in the grid
        if (Groups.Any())
        {
            SelectedGroup = Groups[0];
            await GridRef.SelectRowAsync(0);
        }
        else
        {
            SelectedGroup = null;
            SelectedGroupOriginal = null;
        }

        string logMessage = $"<_userName> heeft het koor {deletedGroupName} verwijderd.";
        await LoggingService.WriteUserActionGroupAsync( deletedGroupId, "Beheer", "Groepen", "success", logMessage );
    }

    private async Task OnTabSelected(SelectEventArgs args)
    {
        currentTabIndex = args.SelectedIndex;

        if (currentTabIndex == 2) // 2 = Personstab'
        {
            await SetupPersonTabAsync();
        }
        else if (currentTabIndex == 3) // 3 = Inschrijvingen
        {
            await LoadGroupRegistrationsAsync();
        }
    }

    private async Task LoadGroupRegistrationsAsync()
    {
        if (SelectedGroup is null || SelectedGroup.GroupId == 0)
        {
            GroupRegistrations = [];
            return;
        }

        GroupRegistrations = await RegistrationService.GetGroupRegistrationsByGroupIdAsync(SelectedGroup.GroupId);
        StateHasChanged();
    }

    private async Task RegisterSelectedGroupForLatestFestivalAsync()
    {
        if (!CanRegisterForLatestFestival || SelectedGroup is null || LatestFestival is null)
            return;

        await RegistrationService.RegisterGroupForCurrentFestivalAsync(SelectedGroup.GroupId, LatestFestival.FestivalId);
        string logMessage = $"<_userName> heeft het koor {SelectedGroup.Name} ingeschreven voor festival editie {LatestFestival.Festival}.";
        await LoggingService.WriteUserActionGroupAsync( SelectedGroup.GroupId, "Beheer", "Groepen", "success", logMessage );
        await LoadGroupRegistrationsAsync();
    }

    private async Task LoadPersonsAsync()
    {
        if (SelectedGroup is null)
            return;

        ActivePersonsList = await PersonService.GetAllActivePersonsByGroupId(SelectedGroup.GroupId);
        UnrelatedPersonsList = await PersonService.GetAllUnrelatedPersonsByGroupId(SelectedGroup.GroupId);

        StateHasChanged(); // Force UI update
    }

    private async Task SetupPersonTabAsync()
    {
        if (SelectedGroup is null)
            return;

        // 1) Load data
        ActivePersonsList = await PersonService.GetAllActivePersonsByGroupId(SelectedGroup.GroupId);
        UnrelatedPersonsList = await PersonService.GetAllUnrelatedPersonsByGroupId(SelectedGroup.GroupId);

        // 2) Build context menus (uses ActivePersonsList for exclusive roles)
        ActivePersonsContextMenuItems = new()
    {
        new ContextMenuItemModel { Text = "Verwijder als relatie van koor", Id = "make-inactive" },
        new ContextMenuItemModel { Text = "Verander de rol in >", Id = "role-parent", Items = CreateRoleMenuItems() }
    };

        UnrelatedPersonsContextMenuItems = new()
    {
        new ContextMenuItemModel { Text = $"Voeg toe voor {SelectedGroup.Name} in de rol van >", Id = "add-to-group", Items = CreateRoleMenuItems() }
    };

        // 3) Ask grids to select first row AFTER they finish databinding
        _selectFirstRowActivePending = ActivePersonsList.Count > 0;
        _selectFirstRowUnrelatedPending = UnrelatedPersonsList.Count > 0;

        StateHasChanged(); // let the two person grids re-render with new data & menus
    }

    private async Task LogGroupChangesAsync()
    {
        if ( SelectedGroupOriginal is null || SelectedGroup is null )
            return;

        var diffOptions = new DiffOptions
        {
            ExcludedProperties = [ "GroupId", "CountryId", "GenreId", "IsActive" ]
        };

        var differences = ObjectDiffHelper.GetDifferences( SelectedGroupOriginal, SelectedGroup, diffOptions );
        string subject = $"het koor {SelectedGroup.Name}";

        if ( SelectedGroupOriginal.GenreId != SelectedGroup.GenreId )
        {
            string oldGenre = GetGenreName( SelectedGroupOriginal.GenreId, SelectedGroupOriginal.Genre );
            string newGenre = GetGenreName( SelectedGroup.GenreId, SelectedGroup.Genre );
            string logMessage = $"<_userName> heeft het genre aangepast van {subject} van '{oldGenre}' naar '{newGenre}'.";
            await LoggingService.WriteUserActionGroupAsync( SelectedGroup.GroupId, "Beheer", "Groepen", "success", logMessage );
        }

        foreach ( var diff in differences )
        {
            string logMessage = AuditLogMessageBuilder.BuildChangeReport( subject, diff );
            await LoggingService.WriteUserActionGroupAsync( SelectedGroup.GroupId, "Beheer", "Groepen", "success", logMessage );
        }
    }

    private static GroupModel? CloneGroup( GroupModel? group )
    {
        if ( group is null )
            return null;

        return new GroupModel
        {
            GroupId = group.GroupId,
            Name = group.Name,
            GenreId = group.GenreId,
            Genre = group.Genre,
            City = group.City,
            CountryId = group.CountryId,
            Country = group.Country,
            Website = group.Website,
            Email = group.Email,
            Photo = group.Photo?.ToArray() ?? [],
            Logo = group.Logo?.ToArray() ?? [],
            Description = group.Description,
            BankAccount = group.BankAccount,
            Active = group.Active,
            IsActive = group.IsActive
        };
    }

    private string GetGenreName( uint genreId, string? fallback )
    {
        return Genres.FirstOrDefault( genre => genre.GenreId == genreId )?.Nl
            ?? fallback
            ?? "onbekend genre";
    }
}
