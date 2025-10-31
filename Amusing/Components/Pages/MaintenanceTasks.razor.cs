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
using Syncfusion.Blazor.RichTextEditor;

namespace Amusing.Components.Pages;

public partial class MaintenanceTasks : ComponentBase
{
    [Inject] protected LoggingService LoggingService { get; set; } = default!;
    [Inject] protected TaskService TaskService { get; set; } = default!;

    protected bool _initialLoadDone = false;
    protected bool IsDeleteEnabled => SelectedTask?.TaskId != 0 || SelectedTask?.IsActive == true;
    protected bool IsLoading = false;
    protected TaskModel? SelectedTask;
    protected TaskModel? SelectedTaskOriginal;
    protected int VisibleRowCount = 0;
    protected List<TaskModel> Tasks = [];
    protected SfGrid<TaskModel>? GridRef;
    protected string FileName = "Taken";
    protected SfRichTextEditor? Rte;


    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;

        Tasks = await TaskService.GetAllTasksAsync();

        SelectedTask = Tasks.FirstOrDefault();

        IsLoading = false;
    }

    protected async Task OnGridDataBound()
    {
        if ( !_initialLoadDone && Tasks?.Any() == true )
        {
            _initialLoadDone = true;
            await UpdateVisibleRowCountAsync();
            await GridRef.SelectRowAsync( 0 );
        }
    }

    protected void OnRowSelected( RowSelectEventArgs<TaskModel> args )
    {
        SelectedTask = args.Data;

        SelectedTaskOriginal = new TaskModel
        {
            TaskId = SelectedTask.TaskId,
            ShortName = SelectedTask.ShortName,
            Name = SelectedTask.Name,
            MinTimeSpan = SelectedTask.MinTimeSpan,
            MaxTimeSpan = SelectedTask.MaxTimeSpan,
            Occupation = SelectedTask.Occupation,
            TimeBlock1From = SelectedTask.TimeBlock1From,
            TimeBlock1Until = SelectedTask.TimeBlock1Until,
            TimeBlock1Volunteers = SelectedTask.TimeBlock1Volunteers,
            TimeBlock2From = SelectedTask.TimeBlock2From,
            TimeBlock2Until = SelectedTask.TimeBlock2Until,
            TimeBlock2Volunteers = SelectedTask.TimeBlock2Volunteers,
            Description = SelectedTask.Description,
            Active = SelectedTask.Active,
        };

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

    public class RequiredIfActiveAttribute : ValidationAttribute
    {
        protected readonly string _dependentProperty;

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

    protected bool IsActive
    {
        get => SelectedTask.Active == "ja";
        set => SelectedTask.Active = value ? "ja" : "nee";
    }

    protected string GetActiveCss()
    {
        return IsActive ? "active-green" : "inactive-red";
    }

    protected async Task Save()
    {
        if ( SelectedTask is null )
            return;

        if ( SelectedTask.TaskId != 0 )
        {
            await TaskService.UpdateTaskAsync( SelectedTask );
            // Check the differences between the original and changed version
            var differences = ObjectDiffHelper.GetDifferences(SelectedTaskOriginal, SelectedTask);

            if ( differences.Count > 0 )
            {
                string taskName = SelectedTask.Name;

                foreach ( var diff in differences )
                {
                    string logMessage = $"<_userName> heeft {diff.PropertyName} van \"{taskName}\" gewijzigd van '{diff.OldValue}' in '{diff.NewValue}'.";
                    await LoggingService.WriteUserActionTaskAsync( SelectedTask.TaskId, "Beheer", "Taken", "updated", logMessage );
                }
            }
        }
        else
        {
            // Save the new Person and get the new Person Id
            var savedId = await TaskService.AddTaskAsync(SelectedTask);
            // Check the differences between the original and changed version
            var differences = ObjectDiffHelper.GetDifferences(SelectedTaskOriginal, SelectedTask);

            if ( differences.Count > 0 )
            {
                string taskName = SelectedTask.Name;

                foreach ( var diff in differences )
                {
                    string logMessage = $"<_userName> heeft {diff.PropertyName} van \"{taskName}\" gewijzigd van '{diff.OldValue}' in '{diff.NewValue}'.";
                    await LoggingService.WriteUserActionTaskAsync( SelectedTask.TaskId, "Beheer", "Taken", "updated", logMessage );
                }
            }

            // Refresh the list
            Tasks = await TaskService.GetAllTasksAsync();
            await Task.Delay( 50 );
            await GridRef.Refresh();

            // Search the modified record
            var index = Tasks.FindIndex(s => s.TaskId == savedId);
            if ( index >= 0 )
            {
                SelectedTask = Tasks [ index ];
                await GridRef.SelectRowAsync( index );
            }
        }

        
    }

    protected async Task AddNew()
    {
        var newTask = new TaskModel
        {
            TaskId = 0,
            ShortName = string.Empty,
            Name = string.Empty,
            MinTimeSpan = 0,
            MaxTimeSpan = 0,
            Occupation = "[]",
            TimeBlock1From = null,
            TimeBlock1Until = null,
            TimeBlock1Volunteers = 0,
            TimeBlock2From = null,
            TimeBlock2Until = null,
            TimeBlock2Volunteers = 0,
            Description =  string.Empty,
            Active = "ja",
        };

        await GridRef.AddRecordAsync( newTask, 0 );
        await GridRef.SelectRowAsync( 0 );

        SelectedTask = newTask;

        string logMessage = $"<_userName> heeft een nieuwe taak aangemaakt ({SelectedTask.TaakId}).";
        await LoggingService.WriteUserActionTaskAsync(SelectedTask.TaskId, "Beheer", "Taken", "added", logMessage );

        StateHasChanged();
    }

    protected async Task TaskActivation()
    {
        if ( SelectedTask is null )
            return;

        await TaskService.TaskActivationAsync( SelectedTask );

        var _tempState = SelectedTask.Active == "ja" ? "nee" : "ja";
        string logMessage = $"<_userName> heeft het De active status van {SelectedTask.Name} aangepast in {_tempState}.";
        await LoggingService.WriteUserActionTaskAsync( SelectedTask.TaskId, "Beheer", "Taken", "success", logMessage );

        
    }
}