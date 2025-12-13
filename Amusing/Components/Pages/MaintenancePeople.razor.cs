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
using Syncfusion.Blazor.Grids.Internal;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.Popups;

namespace Amusing.Components.Pages;

public partial class MaintenancePeople : ComponentBase
{
    [Inject] protected LoggingService LoggingService { get; set; } = default!;
    [Inject] protected PersonService PersonService { get; set; } = default!;

    private bool _initialLoadDone = false;
    private bool IsDeleteEnabled => SelectedPerson?.PersonId != 0 || SelectedPerson?.IsActive == true;
    private bool _isLoading = false;
    private PersonModel? SelectedPerson;
    private int _visibleRowCount = 0;
    private List<PersonModel> Persons = [];
    private List<PersonModel> FilteredPersons = new();
    private SfGrid<PersonModel>? GridRef;
    private string _activeFilter = "All";
    private string ActiveFilter
    {
        get => _activeFilter;
        set
        {
            if ( _activeFilter != value )
            {
                _activeFilter = value;
                ApplyFilter();
            }
        }
    }

    // Strings
    public string SelectedPersonFirstName
    {
        get => SelectedPerson?.FirstName ?? string.Empty;
        set { if (SelectedPerson != null) SelectedPerson.FirstName = value; }
    }

    public string SelectedPersonNameInfix
    {
        get => SelectedPerson?.NameInfix ?? string.Empty;
        set { if (SelectedPerson != null) SelectedPerson.NameInfix = value; }
    }

    public string SelectedPersonLastName
    {
        get => SelectedPerson?.LastName ?? string.Empty;
        set { if (SelectedPerson != null) SelectedPerson.LastName = value; }
    }

    public string SelectedPersonStreet
    {
        get => SelectedPerson?.Street ?? string.Empty;
        set { if (SelectedPerson != null) SelectedPerson.Street = value; }
    }

    public string SelectedPersonHomeNr
    {
        get => SelectedPerson?.HomeNr ?? string.Empty;
        set { if (SelectedPerson != null) SelectedPerson.HomeNr = value; }
    }

    public string SelectedPersonHomeNrAddition
    {
        get => SelectedPerson?.HomeNrAddition ?? string.Empty;
        set { if (SelectedPerson != null) SelectedPerson.HomeNrAddition = value; }
    }

    public string SelectedPersonZip
    {
        get => SelectedPerson?.Zip ?? string.Empty;
        set { if (SelectedPerson != null) SelectedPerson.Zip = value; }
    }

    public string SelectedPersonCity
    {
        get => SelectedPerson?.City ?? string.Empty;
        set { if (SelectedPerson != null) SelectedPerson.City = value; }
    }

    public string SelectedPersonEmail
    {
        get => SelectedPerson?.PersonsEmail ?? string.Empty;
        set { if (SelectedPerson != null) SelectedPerson.PersonsEmail = value; }
    }

    public string SelectedPersonMobile
    {
        get => SelectedPerson?.Mobile ?? string.Empty;
        set { if (SelectedPerson != null) SelectedPerson.Mobile = value; }
    }

    public string SelectedPersonPhone
    {
        get => SelectedPerson?.Phone ?? string.Empty;
        set { if (SelectedPerson != null) SelectedPerson.Phone = value; }
    }

    // Booleans
    public bool SelectedPersonInfoMailing
    {
        get => SelectedPerson?.InfoMailingBool ?? false;
        set { if (SelectedPerson != null) SelectedPerson.InfoMailingBool = value; }
    }




    private string FileName = "Personen";


    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;

        Persons = await PersonService.GetAllPersonsAsync();

        SelectedPerson = Persons.FirstOrDefault();
        ApplyFilter();

        _isLoading = false;
    }

    protected override void OnInitialized()
    {
        ApplyFilter();
    }

    private async Task OnGridDataBound()
    {
        if ( !_initialLoadDone && Persons?.Any() == true )
        {
            _initialLoadDone = true;
            await UpdateVisibleRowCountAsync();
            await GridRef.SelectRowAsync( 0 );
        }
    }

    private void OnRowSelected( RowSelectEventArgs<PersonModel> args )
    {
        SelectedPerson = args.Data;

        StateHasChanged();
    }

    private async Task UpdateVisibleRowCountAsync()
    {
        if ( GridRef is not null )
        {
            var records = await GridRef.GetCurrentViewRecordsAsync();
            await Task.Delay( 150 );
            _visibleRowCount = records?.Count ?? 0;
            StateHasChanged();
        }
    }

    public async Task OnInput( InputEventArgs args )
    {
        await GridRef.SearchAsync( args.Value );

        await Task.Delay( 50 );
        await UpdateVisibleRowCountAsync();
    }

    private async void ApplyFilter()
    {
        FilteredPersons = ActiveFilter switch
        {
            "Active" => Persons.Where( p => p.Active == 1 ).ToList(),
            "Inactive" => Persons.Where( p => p.Active == 0 ).ToList(),
            _ => Persons.ToList()
        };

        await Task.Delay( 50 );
        await UpdateVisibleRowCountAsync();
    }

    public class RequiredIfActiveAttribute : ValidationAttribute
    {
        private readonly string _dependentProperty;

        public RequiredIfActiveAttribute( string dependentProperty )
        {
            _dependentProperty = dependentProperty;
        }

        protected override ValidationResult IsValid( object? value, ValidationContext validationContext )
        {
            var activeProp = validationContext.ObjectType.GetProperty(_dependentProperty);
            if ( activeProp == null )
                return new ValidationResult( $"Property {_dependentProperty} not found" );

            var activeValue = (int)(activeProp.GetValue(validationContext.ObjectInstance) ?? 0);

            // Alleen valideren als Active = 1
            if ( activeValue == 1 && string.IsNullOrWhiteSpace( value?.ToString() ) )
            {
                return new ValidationResult( ErrorMessage ?? "Dit veld is verplicht" );
            }

            return ValidationResult.Success!;
        }
    }

    private bool IsActive
    {
        get => SelectedPerson.Active == 1;
        set => SelectedPerson.Active = value ? 1 : 0;
    }

    private async Task Save()
    {
        // Save data to ah_personen, af_contactgegevens
        if ( SelectedPerson is null )
            return;

        if ( SelectedPerson.PersonId != 0 )
        {
            var savedId = SelectedPerson.PersonId;

            await PersonService.UpdatePersonAsync( SelectedPerson );
            await PersonService.UpdateContactDataAsync( SelectedPerson );
        }
        else
        {
            // Save the new Person and get the new Person Id
            var savedId = await PersonService.AddPersonAsync(SelectedPerson);

            // Save the groepdetails using the PersonId
            var record = await PersonService.AddContactDataAsync(SelectedPerson, savedId);

            // Refresh the list
            Persons = await PersonService.GetAllPersonsAsync();
            await Task.Delay( 50 );
            await GridRef.Refresh();

            // Search the modified record
            var index = Persons.FindIndex(s => s.PersonId == savedId);
            if ( index >= 0 )
            {
                SelectedPerson = Persons [ index ];
                await GridRef.SelectRowAsync( index );
            }
        }
    }

    private async Task AddNew()
    {
        var newPerson = new PersonModel
        {
            PersonId = 0,
            FirstName = string.Empty,
            NameInfix = string.Empty,
            LastName = string.Empty,
            PersonsEmail = string.Empty,
            InfoMailing = 0,
            Zip = string.Empty,
            Street = string.Empty,
            HomeNr = string.Empty,
            HomeNrAddition = string.Empty,
            City = string.Empty,
            Phone = string.Empty,
            Mobile = string.Empty,
            Active = 1
        };

        await GridRef.AddRecordAsync( newPerson, 0 );
        await GridRef.SelectRowAsync( 0 );

        SelectedPerson = newPerson;

        StateHasChanged();
    }

    private async Task PersonActivation()
    {
        if ( SelectedPerson is null )
            return;

        await PersonService.PersonActivationAsync( SelectedPerson );
    }
}