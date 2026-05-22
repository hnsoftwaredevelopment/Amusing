using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

using Amusing.Helpers;
using Amusing.Models;
using Amusing.Services;

using Bit.BlazorUI;
using Bit.BlazorUI.Extras;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.Calendars;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.Popups;

namespace Amusing.Components.Pages;

public partial class MaintenanceUsers : ComponentBase
{
    [Inject] protected LoggingService LoggingService { get; set; } = default!;
    [Inject] protected UserService UserService { get; set; } = default!;
    [Inject] protected CustomAuthenticationService CustomAuthenticationService { get; set; } = default!;
    [Inject] protected ToastService ToastService { get; set; } = default!;

    protected bool _initialLoadDone = false;
    protected bool IsLoading = false;
    protected UserModel? SelectedUser;
    protected UserModel? SelectedUserOriginal;
    protected int VisibleRowCount = 0;
    protected EditContext? editContext;
    protected List<UserModel> Users = [];
    protected SfGrid<UserModel>? GridRef;
    protected string FileName = "Gebruikers";
    public string HashedPassword { get; set; } = string.Empty;

    // String wrappers voor textboxes en combobox
    public uint UserId
    {
        get => SelectedUser?.UserId ?? 0;
        set { if (SelectedUser != null) SelectedUser.UserId = value; }
    }

    public string UserUsername
    {
        get => SelectedUser?.Username ?? string.Empty;
        set { if (SelectedUser != null) SelectedUser.Username = value; }
    }

    public string UserRole
    {
        get => SelectedUser?.Role ?? string.Empty;
        set { if (SelectedUser != null) SelectedUser.Role = value; }
    }

    public string UserPassword
    {
        get => SelectedUser?.Password ?? string.Empty;
        set { if (SelectedUser != null) SelectedUser.Password = value; }
    }

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;

        Users = await UserService.GetAllUsersAsync();

        SelectedUser = Users.FirstOrDefault();
        editContext = new EditContext( SelectedUser );

        IsLoading = false;
    }

    protected async Task OnGridDataBound()
    {
        if ( !_initialLoadDone && Users?.Any() == true )
        {
            _initialLoadDone = true;
            await UpdateVisibleRowCountAsync();
            await GridRef.SelectRowAsync( 0 );
        }
    }

    protected void OnRowSelected( RowSelectEventArgs<UserModel> args )
    {
        SelectedUser = args.Data;

        SelectedUserOriginal = new UserModel
        {
            UserId = SelectedUser.UserId,
            Username = SelectedUser.Username,
            Password = SelectedUser.Password,
            PasswordHash = SelectedUser.PasswordHash,
            Role = SelectedUser.Role
        };

        editContext = new EditContext( SelectedUser );
        StateHasChanged();
    }

    protected async Task UpdateVisibleRowCountAsync()
    {
        if ( GridRef is not null )
        {
            var records = await GridRef.GetCurrentViewRecordsAsync();
            await Task.Delay( 150 );
            VisibleRowCount = records?.Count ?? 0;
            StateHasChanged();
        }
    }

    public async Task OnInput( InputEventArgs args )
    {
        await GridRef.SearchAsync( args.Value );

        await Task.Delay( 50 );
        await UpdateVisibleRowCountAsync();
    }

    protected async Task Save()
    {
        if ( SelectedUser is null )
            return;

        if ( SelectedUser.UserId != 0 )
        {
            await UserService.UpdateUserAsync( SelectedUser );

            var diffOptions = new DiffOptions
            {
                ExcludedProperties = ["PasswordHash"],
                MaskedProperties = ["Password"]
            };

            // Check the differences between the original and changed version
            var differences = ObjectDiffHelper.GetDifferences(SelectedUserOriginal, SelectedUser, diffOptions);

            if ( differences.Count > 0 )
            {
                string userName = SelectedUser.Username;

                foreach ( var diff in differences )
                {
                    string logMessage = $"<_userName> heeft {diff.PropertyName} van \"{userName}\" gewijzigd van '{diff.OldValue}' in '{diff.NewValue}'.";
                    await LoggingService.WriteUserActionAsync( "Beheer", "Gebruikers", "updated", logMessage );
                }
            }
        }
        else
        {
            // Save the new Person and get the new Person Id
            SelectedUser.Password = CustomAuthenticationService.ComputeMd5Hash( SelectedUser.Password );
            SelectedUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword( SelectedUser.PasswordHash );

            var savedId = await UserService.AddUserAsync(SelectedUser);

            var diffOptions = new DiffOptions
            {
                ExcludedProperties = ["PasswordHash"],
                MaskedProperties = ["Password"]
            };

            // Check the differences between the original and changed version
            var differences = ObjectDiffHelper.GetDifferences(SelectedUserOriginal, SelectedUser, diffOptions);

            if ( differences.Count > 0 )
            {
                string userName = SelectedUser.Username;

                foreach ( var diff in differences )
                {
                    string logMessage = $"<_userName> heeft {diff.PropertyName} van \"{userName}\" gewijzigd van '{diff.OldValue}' in '{diff.NewValue}'.";
                    await LoggingService.WriteUserActionAsync( "Beheer", "Gebruikers", "updated", logMessage );
                }
            }

            // Refresh the list
            Users = await UserService.GetAllUsersAsync();
            await Task.Delay( 50 );
            await GridRef.Refresh();
            await UpdateVisibleRowCountAsync();


            // Search the modified record
            var index = Users.FindIndex(s => s.UserId == savedId);
            if ( index >= 0 )
            {
                SelectedUser = Users [ index ];
                await GridRef.SelectRowAsync( index );
            }
        }

        await ToastService.ShowSuccessAsync( $"De wijzigingen voor gebruiker {SelectedUser.Username} zijn opgeslagen." );
        SelectedUser.Password = string.Empty;
        if ( SelectedUserOriginal is not null )
        {
            SelectedUserOriginal.Password = string.Empty;
        }
    }

    protected async Task AddNew()
    {
        var newUser = new UserModel
        {
            UserId = 0,
            Username = "",
            Password = "",
            PasswordHash = "",
            Role =  ""
        };

        await GridRef.AddRecordAsync( newUser, 0 );
        await GridRef.SelectRowAsync( 0 );

        SelectedUser = newUser;
        editContext = new EditContext( SelectedUser );
        StateHasChanged();
    }

    protected async Task SavePassword()
    {
        if ( !string.IsNullOrWhiteSpace( SelectedUser.Password ) )
        {
            SelectedUser.Password = CustomAuthenticationService.ComputeMd5Hash( SelectedUser.Password );

            await UserService.UpdatePasswordAsync( SelectedUser );

            SelectedUser.Password = string.Empty;

            string logMessage = $"<_userName> heeft wachtwoord van {SelectedUser.Username} gewijzigd van '********' in '********'.";
            await LoggingService.WriteUserActionAsync( "Beheer", "Gebruikers", "password", logMessage );

            await ToastService.ShowSuccessAsync( $"Het wachtwoord voor gebruiker {SelectedUser.Username} is opgeslagen." );
        }
    }

    protected async Task Delete()
    {
        if ( SelectedUser.UserId == 0 )
            return;

        string userName = SelectedUser.Username;
        await UserService.DeleteUserAsync( SelectedUser );

        string logMessage = $"<_userName> heeft het gebruiker: {userName} verwijderd.";
        await LoggingService.WriteUserActionAsync( "Beheer", "Gebruikers", "deleted", logMessage );
        await ToastService.ShowSuccessAsync( $"Gebruiker {userName} is verwijderd." );

        // Refresh the list
        Users = await UserService.GetAllUsersAsync();
        await Task.Delay( 50 );
        await GridRef.Refresh();
        await UpdateVisibleRowCountAsync();
    }

    protected List<UserRoles> Roles =
    [
        new UserRoles() { Role= "admin" },
        new UserRoles() { Role= "algemeen" },
        new UserRoles() { Role= "penningmeester" },
        new UserRoles() { Role= "contactpersoon" },
        new UserRoles() { Role= "vrijwilligers" },
        new UserRoles() { Role= "pr" }
    ];


}
