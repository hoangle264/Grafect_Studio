using System.Collections.ObjectModel;
using GrafcetStudio.Core.Models;
using GrafcetStudio.Core.Models.Document;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;

namespace GrafcetStudio.WPF.ViewModels;

/// <summary>ViewModel for the step table panel — exposes the full step list for grid-based editing.</summary>
public class StepTableViewModel : BindableBase, INavigationAware
{
    private readonly GrafcetDocument _document;
    private StepRowViewModel? _selectedStep;

    public ObservableCollection<StepRowViewModel> Steps { get; } = [];

    public StepRowViewModel? SelectedStep
    {
        get => _selectedStep;
        set
        {
            SetProperty(ref _selectedStep, value);
            RemoveStepCommand.RaiseCanExecuteChanged();
        }
    }

    public DelegateCommand AddStepCommand    { get; }
    public DelegateCommand RemoveStepCommand { get; }

    public StepTableViewModel(GrafcetDocument document)
    {
        _document = document;
        AddStepCommand = new DelegateCommand(ExecuteAddStep);
        RemoveStepCommand = new DelegateCommand(ExecuteRemoveStep, () => SelectedStep is not null)
            .ObservesProperty(() => SelectedStep);
    }

    private void ExecuteAddStep()
    {
        var newStep = StepRowViewModel.CreateNew();
        newStep.Id = Steps.Count == 0 ? 1 : Steps.Max(s => s.Id) + 1;
        Steps.Add(newStep);
        SelectedStep = newStep;
    }

    private void ExecuteRemoveStep()
    {
        if (SelectedStep is null) return;
        Steps.Remove(SelectedStep);
        SelectedStep = null;
    }

    /// <summary>Clears and reloads <see cref="Steps"/> from the given document.</summary>
    public void LoadFrom(GrafcetDocument doc)
    {
        Steps.Clear();
        foreach (var step in doc.Steps)
            Steps.Add(new StepRowViewModel(step));
    }

    /// <summary>Writes all <see cref="StepRowViewModel"/> entries back into the document's step list.</summary>
    public void ApplyTo(GrafcetDocument doc)
    {
        doc.Steps.Clear();
        foreach (var row in Steps)
        {
            doc.Steps.Add(new GrafcetStep
            {
                Id        = row.Id,
                Name      = row.Name,
                IsInitial = row.IsInitial,
                BranchId  = row.BranchId,
                BranchRole = row.BranchRole,
                Actions   = row.Actions.Select(a => a.ToModel()).ToList()
            });
        }
    }

    // ── INavigationAware ──────────────────────────────────────────────────────

    public void OnNavigatedTo(NavigationContext navigationContext)   { }
    public void OnNavigatedFrom(NavigationContext navigationContext) { }
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Editable row representing a single GRAFCET step in the step table.</summary>
public class StepRowViewModel : BindableBase
{
    private int         _id;
    private string      _name      = "";
    private bool        _isInitial;
    private int?        _branchId;
    private BranchRole  _branchRole;

    public StepRowViewModel(GrafcetStep step)
    {
        _id         = step.Id;
        _name       = step.Name;
        _isInitial  = step.IsInitial;
        _branchId   = step.BranchId;
        _branchRole = step.BranchRole;
        Actions = new ObservableCollection<ActionRowViewModel>(
            step.Actions.Select(a => new ActionRowViewModel(a)));
    }

    private StepRowViewModel() { }

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public bool IsInitial
    {
        get => _isInitial;
        set => SetProperty(ref _isInitial, value);
    }

    public int? BranchId
    {
        get => _branchId;
        set => SetProperty(ref _branchId, value);
    }

    public BranchRole BranchRole
    {
        get => _branchRole;
        set => SetProperty(ref _branchRole, value);
    }

    public ObservableCollection<ActionRowViewModel> Actions { get; }
        = [];

    /// <summary>Creates a blank <see cref="StepRowViewModel"/> with Id = 0.</summary>
    public static StepRowViewModel CreateNew() => new();
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Editable row representing a single GRAFCET action within a step.</summary>
public class ActionRowViewModel : BindableBase
{
    private ActionQualifier _qualifier;
    private string          _variable  = "";
    private string?         _parameter;

    public ActionRowViewModel(GrafcetAction action)
    {
        _qualifier = action.Qualifier;
        _variable  = action.Variable;
        _parameter = action.Parameter;
    }

    private ActionRowViewModel() { }

    public ActionQualifier Qualifier
    {
        get => _qualifier;
        set => SetProperty(ref _qualifier, value);
    }

    public string Variable
    {
        get => _variable;
        set => SetProperty(ref _variable, value);
    }

    public string? Parameter
    {
        get => _parameter;
        set => SetProperty(ref _parameter, value);
    }

    /// <summary>Converts this row back to a plain <see cref="GrafcetAction"/> model.</summary>
    public GrafcetAction ToModel() => new()
    {
        Qualifier = _qualifier,
        Variable  = _variable,
        Parameter = _parameter
    };

    /// <summary>Creates a blank <see cref="ActionRowViewModel"/> with default qualifier N.</summary>
    public static ActionRowViewModel CreateNew() => new();
}
