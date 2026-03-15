using System.Collections.ObjectModel;
using GrafcetStudio.Core.Commands;
using GrafcetStudio.Core.Commands.Steps;
using GrafcetStudio.Core.Commands.Transitions;
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
    private readonly IEventAggregator _eventAggregator;
    private readonly GrafcetDocument  _document;
    private readonly UndoRedoStack    _undoRedoStack;

    private bool _isStepSelected;
    private bool _isTransitionSelected;
    private object? _selectedVm;   // cached StepViewModel or TransitionViewModel

    // ── Step backing fields ───────────────────────────────────────────────────
    private int           _stepId;
    private string        _stepName       = "";
    private bool          _stepIsInitial;
    private double        _stepX;
    private double        _stepY;
    private GrafcetAction? _selectedAction;

    // ── Transition backing fields ─────────────────────────────────────────────
    private int    _transitionId;
    private string _transitionCondition = "";
    private int    _transitionFromStepId;
    private int    _transitionToStepId;

    public PropertiesViewModel(
        IEventAggregator eventAggregator,
        GrafcetDocument  document,
        UndoRedoStack    undoRedoStack)
    {
        _eventAggregator = eventAggregator;
        _document        = document;
        _undoRedoStack   = undoRedoStack;

        StepActions = [];

        eventAggregator
            .GetEvent<ElementSelectedEvent>()
            .Subscribe(OnElementSelected, ThreadOption.UIThread);

        ApplyStepCommand = new DelegateCommand(ExecuteApplyStep, () => IsStepSelected)
            .ObservesProperty(() => IsStepSelected);

        ApplyTransitionCommand = new DelegateCommand(ExecuteApplyTransition, () => IsTransitionSelected)
            .ObservesProperty(() => IsTransitionSelected);

        AddActionCommand    = new DelegateCommand(ExecuteAddAction);
        RemoveActionCommand = new DelegateCommand<GrafcetAction>(ExecuteRemoveAction);
    }

    // ── Step properties ───────────────────────────────────────────────────────

    /// <summary>Id of the selected step (read-only).</summary>
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
        set => SetProperty(ref _stepIsInitial, value);
    }

    public double StepX
    {
        get => _stepX;
        set => SetProperty(ref _stepX, value);
    }

    public double StepY
    {
        get => _stepY;
        set => SetProperty(ref _stepY, value);
    }

    /// <summary>Editable list of actions for the selected step.</summary>
    public ObservableCollection<GrafcetAction> StepActions { get; }

    public GrafcetAction? SelectedAction
    {
        get => _selectedAction;
        set => SetProperty(ref _selectedAction, value);
    }

    public static IEnumerable<ActionQualifier> ActionQualifiers { get; } =
        Enum.GetValues<ActionQualifier>();

    // ── Transition properties ─────────────────────────────────────────────────

    /// <summary>Id of the selected transition (read-only).</summary>
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

    public int TransitionFromStepId
    {
        get => _transitionFromStepId;
        set => SetProperty(ref _transitionFromStepId, value);
    }

    public int TransitionToStepId
    {
        get => _transitionToStepId;
        set => SetProperty(ref _transitionToStepId, value);
    }

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

    /// <summary>Commits edited step properties to the document via undo/redo.</summary>
    public DelegateCommand ApplyStepCommand { get; }

    /// <summary>Commits edited transition properties to the document via undo/redo.</summary>
    public DelegateCommand ApplyTransitionCommand { get; }

    /// <summary>Adds a new default action to <see cref="StepActions"/>.</summary>
    public DelegateCommand AddActionCommand { get; }

    /// <summary>Removes the given action from <see cref="StepActions"/>.</summary>
    public DelegateCommand<GrafcetAction> RemoveActionCommand { get; }

    // ── Event handler ─────────────────────────────────────────────────────────

    private void OnElementSelected(object? payload)
    {
        if (payload is StepViewModel stepVm)
        {
            _selectedVm = stepVm;
            var step = _document.GetStep(stepVm.Id);
            if (step is not null)
            {
                StepId        = step.Id;
                StepName      = step.Name;
                StepIsInitial = step.IsInitial;
                StepX         = step.X;
                StepY         = step.Y;

                StepActions.Clear();
                foreach (var action in step.Actions)
                    StepActions.Add(action);
            }

            IsStepSelected       = true;
            IsTransitionSelected = false;
        }
        else if (payload is TransitionViewModel transVm)
        {
            _selectedVm = transVm;
            var trans = _document.GetTransition(transVm.Id);
            if (trans is not null)
            {
                TransitionId         = trans.Id;
                TransitionCondition  = trans.Condition;
                TransitionFromStepId = trans.FromStepId;
                TransitionToStepId   = trans.ToStepId;
            }

            IsStepSelected       = false;
            IsTransitionSelected = true;
        }
        else
        {
            _selectedVm          = null;
            IsStepSelected       = false;
            IsTransitionSelected = false;
        }
    }

    // ── Command handlers ──────────────────────────────────────────────────────

    private void ExecuteApplyStep()
    {
        var cmd = new ModifyStepCommand(
            StepId,
            StepName,
            StepIsInitial,
            (int)StepX,
            (int)StepY,
            StepActions.ToList());

        _undoRedoStack.Push(cmd, _document);

        _eventAggregator.GetEvent<ElementSelectedEvent>().Publish(null);        // canvas refresh
        _eventAggregator.GetEvent<ElementSelectedEvent>().Publish(_selectedVm); // reselect
    }

    private void ExecuteApplyTransition()
    {
        var cmd = new ModifyTransitionCommand(
            TransitionId,
            TransitionCondition,
            TransitionFromStepId,
            TransitionToStepId);

        _undoRedoStack.Push(cmd, _document);

        _eventAggregator.GetEvent<ElementSelectedEvent>().Publish(null);        // canvas refresh
        _eventAggregator.GetEvent<ElementSelectedEvent>().Publish(_selectedVm); // reselect
    }

    private void ExecuteAddAction()
        => StepActions.Add(new GrafcetAction { Qualifier = ActionQualifier.N, Variable = "" });

    private void ExecuteRemoveAction(GrafcetAction? action)
    {
        if (action is not null)
            StepActions.Remove(action);
    }

    // ── INavigationAware ──────────────────────────────────────────────────────

    public void OnNavigatedTo(NavigationContext navigationContext) { }
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }
}
