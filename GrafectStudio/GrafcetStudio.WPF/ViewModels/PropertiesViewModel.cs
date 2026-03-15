using System.Collections.ObjectModel;
using System.Text.Json;
using GrafcetStudio.Core.Commands;
using GrafcetStudio.Core.Models;
using GrafcetStudio.Core.Models.Document;
using GrafcetStudio.Core.Services;
using GrafcetStudio.WPF.Events;
using GrafcetStudio.WPF.ViewModels.Canvas;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation.Regions;

namespace GrafcetStudio.WPF.ViewModels;

/// <summary>
/// ViewModel for the Properties panel.
/// Reacts to <see cref="ElementSelectedEvent"/> and exposes the selected
/// element's data for two-way editing with undo support.
/// </summary>
public class PropertiesViewModel : BindableBase, INavigationAware
{
    private readonly IEventAggregator  _eventAggregator;
    private readonly GrafcetDocument   _document;
    private readonly ToolCallExecutor  _toolCallExecutor;

    private bool    _isStepSelected;
    private bool    _isTransitionSelected;
    private object? _selectedVm;   // cached StepViewModel or TransitionViewModel

    // ── Step backing fields ───────────────────────────────────────────────────
    private int    _stepId;
    private string _stepName      = "";
    private bool   _stepIsInitial;

    // ── Transition backing fields ─────────────────────────────────────────────
    private int    _transitionId;
    private string _transitionCondition = "";

    // ── Actions ───────────────────────────────────────────────────────────────
    private GrafcetActionViewModel? _selectedAction;

    public PropertiesViewModel(
        IEventAggregator  eventAggregator,
        GrafcetDocument   document,
        ToolCallExecutor  toolCallExecutor)
    {
        _eventAggregator  = eventAggregator;
        _document         = document;
        _toolCallExecutor = toolCallExecutor;

        Actions = [];

        ApplyCommand = new DelegateCommand(ExecuteApply,
                () => IsStepSelected || IsTransitionSelected)
            .ObservesProperty(() => IsStepSelected)
            .ObservesProperty(() => IsTransitionSelected);

        CancelCommand       = new DelegateCommand(ExecuteCancel);
        AddActionCommand    = new DelegateCommand(ExecuteAddAction);
        RemoveActionCommand = new DelegateCommand<GrafcetActionViewModel>(ExecuteRemoveAction);

        eventAggregator
            .GetEvent<ElementSelectedEvent>()
            .Subscribe(OnElementSelected, ThreadOption.UIThread);
    }

    // ── Step properties ───────────────────────────────────────────────────────

    public int StepId
    {
        get => _stepId;
        private set => SetProperty(ref _stepId, value);
    }

    public string StepName
    {
        get => _stepName;
        set => SetProperty(ref _stepName, value);
    }

    public bool StepIsInitial
    {
        get => _stepIsInitial;
        private set => SetProperty(ref _stepIsInitial, value);
    }

    // ── Transition properties ─────────────────────────────────────────────────

    public int TransitionId
    {
        get => _transitionId;
        private set => SetProperty(ref _transitionId, value);
    }

    public string TransitionCondition
    {
        get => _transitionCondition;
        set => SetProperty(ref _transitionCondition, value);
    }

    // ── Actions ───────────────────────────────────────────────────────────────

    public ObservableCollection<GrafcetActionViewModel> Actions { get; }

    public GrafcetActionViewModel? SelectedAction
    {
        get => _selectedAction;
        set => SetProperty(ref _selectedAction, value);
    }

    public static IEnumerable<ActionQualifier> ActionQualifiers { get; } =
        Enum.GetValues<ActionQualifier>();

    // ── Common properties ─────────────────────────────────────────────────────

    public bool IsStepSelected
    {
        get => _isStepSelected;
        private set
        {
            if (SetProperty(ref _isStepSelected, value))
                RaisePropertyChanged(nameof(IsNothingSelected));
        }
    }

    public bool IsTransitionSelected
    {
        get => _isTransitionSelected;
        private set
        {
            if (SetProperty(ref _isTransitionSelected, value))
                RaisePropertyChanged(nameof(IsNothingSelected));
        }
    }

    public bool IsNothingSelected => !_isStepSelected && !_isTransitionSelected;

    // ── Commands ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Commits edited properties to the document via <see cref="ToolCallExecutor"/>
    /// and triggers a canvas refresh. Enabled only when an element is selected.
    /// </summary>
    public DelegateCommand ApplyCommand { get; }

    /// <summary>Reloads the current element''s data, discarding any unsaved edits.</summary>
    public DelegateCommand CancelCommand { get; }

    /// <summary>Adds a new default action row to <see cref="Actions"/>.</summary>
    public DelegateCommand AddActionCommand { get; }

    /// <summary>Removes the supplied action row from <see cref="Actions"/>.</summary>
    public DelegateCommand<GrafcetActionViewModel> RemoveActionCommand { get; }

    // ── Event handler ─────────────────────────────────────────────────────────

    private void OnElementSelected(object? payload)
    {
        if (payload is StepViewModel stepVm)
        {
            _selectedVm = stepVm;
            LoadStep(stepVm.Id);
            IsStepSelected       = true;
            IsTransitionSelected = false;
        }
        else if (payload is TransitionViewModel transVm)
        {
            _selectedVm = transVm;
            LoadTransition(transVm.Id);
            IsStepSelected       = false;
            IsTransitionSelected = true;
        }
        else
        {
            _selectedVm          = null;
            IsStepSelected       = false;
            IsTransitionSelected = false;
            Actions.Clear();
        }
    }

    private void LoadStep(int stepId)
    {
        var step = _document.GetStep(stepId);
        if (step is null) return;

        StepId        = step.Id;
        StepName      = step.Name;
        StepIsInitial = step.IsInitial;

        Actions.Clear();
        foreach (var action in step.Actions)
            Actions.Add(new GrafcetActionViewModel(action));
    }

    private void LoadTransition(int transitionId)
    {
        var trans = _document.GetTransition(transitionId);
        if (trans is null) return;

        TransitionId        = trans.Id;
        TransitionCondition = trans.Condition;
    }

    // ── Command handlers ──────────────────────────────────────────────────────

    private void ExecuteApply()
    {
        ToolCallBatch batch;

        if (IsStepSelected)
        {
            var actionModels = Actions.Select(a => a.ToModel()).ToList();
            var prms = JsonSerializer.SerializeToElement(new
            {
                stepId  = StepId,
                name    = StepName,
                actions = actionModels
            });
            batch = new ToolCallBatch
            {
                Calls       = [new ToolCall { Tool = "ModifyStep", Params = prms }],
                Explanation = $"Edit step {StepId}"
            };
        }
        else if (IsTransitionSelected)
        {
            var prms = JsonSerializer.SerializeToElement(new
            {
                transitionId = TransitionId,
                condition    = TransitionCondition
            });
            batch = new ToolCallBatch
            {
                Calls       = [new ToolCall { Tool = "ModifyTransition", Params = prms }],
                Explanation = $"Edit transition {TransitionId}"
            };
        }
        else return;

        var result = _toolCallExecutor.Execute(batch);
        if (!result.Success) return;

        // Trigger canvas refresh then reselect so the canvas updates labels
        _eventAggregator.GetEvent<ElementSelectedEvent>().Publish(null);
        _eventAggregator.GetEvent<ElementSelectedEvent>().Publish(_selectedVm);
    }

    private void ExecuteCancel() => OnElementSelected(_selectedVm);

    private void ExecuteAddAction()
        => Actions.Add(new GrafcetActionViewModel { Qualifier = ActionQualifier.N, Variable = "" });

    private void ExecuteRemoveAction(GrafcetActionViewModel? action)
    {
        if (action is not null)
            Actions.Remove(action);
    }

    // ── INavigationAware ──────────────────────────────────────────────────────

    public void OnNavigatedTo(NavigationContext navigationContext) { }
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }
}
