using System.Collections.ObjectModel;
using System.Text.Json;
using GrafcetStudio.Core.Commands;
using GrafcetStudio.Core.Commands.Links;
using GrafcetStudio.Core.Commands.Steps;
using GrafcetStudio.Core.Commands.Transitions;
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
/// ViewModel for the GRAFCET diagram canvas.
/// Owns the observable collections of steps, transitions and links
/// rendered by the canvas View, and coordinates selection and zoom.
/// </summary>
public class GrafcetCanvasViewModel : BindableBase, INavigationAware
{
    // Default dimensions used for link endpoint and transition position calculations
    private const double STEP_WIDTH        = 80.0;
    private const double STEP_HEIGHT       = 40.0;
    private const double TRANSITION_HEIGHT = 4.0;
    private const double ZOOM_STEP         = 0.1;
    private const double ZOOM_MIN          = 0.2;
    private const double ZOOM_MAX          = 5.0;

    private readonly IEventAggregator _eventAggregator;
    private readonly ToolCallExecutor _toolCallExecutor;
    private readonly UndoRedoStack _undoRedoStack;

    private GrafcetDocument? _document;
    private double _zoom = 1.0;
    private object? _selectedElement;
    private bool _isLinkMode;
    private object? _linkSource;

    public GrafcetCanvasViewModel(
        IEventAggregator eventAggregator,
        ToolCallExecutor toolCallExecutor,
        GrafcetDocument document,
        UndoRedoStack undoRedoStack)
    {
        _eventAggregator  = eventAggregator;
        _toolCallExecutor = toolCallExecutor;
        _undoRedoStack    = undoRedoStack;
        _document         = document;

        DeleteCommand = new DelegateCommand(ExecuteDelete, () => SelectedElement is not null)
            .ObservesProperty(() => SelectedElement);

        ZoomCommand          = new DelegateCommand<string>(ExecuteZoom);
        SelectElementCommand = new DelegateCommand<object?>(ExecuteSelectOrConnect);
        DropElementCommand   = new DelegateCommand<DropPayload>(ExecuteDrop);
        MoveElementCommand   = new DelegateCommand<MovePayload>(ExecuteMove);
        CancelLinkCommand    = new DelegateCommand(ExecuteCancelLink);
    }

    // ── Collections ───────────────────────────────────────────────────────────

    public ObservableCollection<StepViewModel>       Steps       { get; } = [];
    public ObservableCollection<TransitionViewModel> Transitions { get; } = [];
    public ObservableCollection<LinkViewModel>       Links       { get; } = [];

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>Current canvas zoom level (0.2 – 5.0, default 1.0).</summary>
    public double Zoom
    {
        get => _zoom;
        set => SetProperty(ref _zoom, Math.Clamp(value, ZOOM_MIN, ZOOM_MAX));
    }

    /// <summary>
    /// Currently selected canvas element (StepViewModel, TransitionViewModel, or null).
    /// Setting this updates <see cref="StepViewModel.IsSelected"/> /
    /// <see cref="TransitionViewModel.IsSelected"/> and publishes
    /// <see cref="ElementSelectedEvent"/>.
    /// </summary>
    public object? SelectedElement
    {
        get => _selectedElement;
        set
        {
            var previous = _selectedElement;
            if (!SetProperty(ref _selectedElement, value)) return;

            // Deselect previous element
            if (previous is StepViewModel prevStep)       prevStep.IsSelected = false;
            else if (previous is TransitionViewModel prevTrans) prevTrans.IsSelected = false;

            // Select new element
            if (value is StepViewModel step)            step.IsSelected = true;
            else if (value is TransitionViewModel trans) trans.IsSelected = true;

            _eventAggregator.GetEvent<ElementSelectedEvent>().Publish(value);
        }
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    /// <summary>Deletes the currently selected step or transition (with undo support).</summary>
    public DelegateCommand DeleteCommand { get; }

    /// <summary>Zooms in ("+" argument) or out ("-" argument).</summary>
    public DelegateCommand<string> ZoomCommand { get; }

    /// <summary>Selects a step or transition by clicking on it in the canvas.</summary>
    public DelegateCommand<object?> SelectElementCommand { get; }

    /// <summary>Creates a GRAFCET element at the drop coordinates supplied by <see cref="DropPayload"/>.</summary>
    public DelegateCommand<DropPayload> DropElementCommand { get; }

    /// <summary>Moves a step or transition by the delta supplied by <see cref="MovePayload"/>.</summary>
    public DelegateCommand<MovePayload> MoveElementCommand { get; }

    /// <summary>Cancels the current link-creation operation.</summary>
    public DelegateCommand CancelLinkCommand { get; }

    /// <summary>True when the canvas is in link-creation mode (click source then target).</summary>
    public bool IsLinkMode
    {
        get => _isLinkMode;
        private set => SetProperty(ref _isLinkMode, value);
    }

    // ── Command handlers ──────────────────────────────────────────────────────

    private void ExecuteDelete()
    {
        if (_document is null || _selectedElement is null) return;

        ToolCallBatch batch;

        if (_selectedElement is StepViewModel step)
        {
            var prms = JsonSerializer.SerializeToElement(new { stepId = step.Id });
            batch = new ToolCallBatch
            {
                Calls       = [new ToolCall { Tool = "RemoveStep", Params = prms }],
                Explanation = $"Delete step {step.Id}"
            };
        }
        else if (_selectedElement is TransitionViewModel trans)
        {
            var prms = JsonSerializer.SerializeToElement(new { transitionId = trans.Id });
            batch = new ToolCallBatch
            {
                Calls       = [new ToolCall { Tool = "RemoveTransition", Params = prms }],
                Explanation = $"Delete transition {trans.Id}"
            };
        }
        else return;

        var result = _toolCallExecutor.Execute(batch);
        if (result.Success)
            LoadFrom(_document); // Reload canvas to reflect the updated document
    }

    private void ExecuteZoom(string direction)
    {
        Zoom = direction switch
        {
            "+" => Zoom + ZOOM_STEP,
            "-" => Zoom - ZOOM_STEP,
            _   => Zoom
        };
    }

    private void ExecuteDrop(DropPayload? payload)
    {
        if (payload is null || _document is null) return;

        switch (payload.ElementType)
        {
            case "Step":
            case "InitialStep":
            {
                bool isInitial = payload.ElementType == "InitialStep";

                // GRAFCET rule: at most one initial step
                if (isInitial && _document.Steps.Any(s => s.IsInitial))
                {
                    System.Diagnostics.Debug.WriteLine(
                        "[DropElement] Warning: document already has an initial step — drop ignored.");
                    return;
                }

                int id   = _document.NextStepId();
                var step = new GrafcetStep
                {
                    Id        = id,
                    Name      = $"Step {id}",
                    IsInitial = isInitial,
                    X         = payload.X,
                    Y         = payload.Y
                };

                _undoRedoStack.Push(new AddStepCommand(step), _document);
                LoadFrom(_document);
                SelectedElement = Steps.FirstOrDefault(s => s.Id == step.Id);
                break;
            }

            case "Transition":
            {
                int id         = _document.NextTransitionId();
                var transition = new GrafcetTransition
                {
                    Id         = id,
                    Condition  = "TRUE",
                    FromStepId = 0,
                    ToStepId   = 0,
                    X          = payload.X,
                    Y          = payload.Y
                };

                _undoRedoStack.Push(new AddTransitionCommand(transition), _document);
                LoadFrom(_document);
                SelectedElement = Transitions.FirstOrDefault(t => t.Id == transition.Id);
                break;
            }

            case "Link":
                IsLinkMode  = true;
                _linkSource = null;
                SelectedElement = null;
                System.Diagnostics.Debug.WriteLine("[DropElement] Link mode activated.");
                break;

            case "ParallelBranch":
            case "SelectiveBranch":
                System.Diagnostics.Debug.WriteLine(
                    $"[DropElement] Warning: '{payload.ElementType}' not implemented yet.");
                break;
        }
    }

    private void ExecuteMove(MovePayload? payload)
    {
        if (payload is null || _document is null) return;

        if (payload.Element is StepViewModel stepVm)
        {
            var step = _document.GetStep(stepVm.Id);
            if (step is null) return;

            double newX = step.X + payload.DeltaX;
            double newY = step.Y + payload.DeltaY;

            System.Diagnostics.Debug.WriteLine($"[ExecuteMove] Step {stepVm.Id}: ({step.X}, {step.Y}) + ({payload.DeltaX}, {payload.DeltaY}) = ({newX}, {newY})");

            _undoRedoStack.Push(new ModifyStepCommand(stepVm.Id, x: newX, y: newY), _document);

            stepVm.X = newX;
            stepVm.Y = newY;
            RecalculateLinks();
        }
        else if (payload.Element is TransitionViewModel transVm)
        {
            var trans = _document.GetTransition(transVm.Id);
            if (trans is null) return;

            double newX = trans.X + payload.DeltaX;
            double newY = trans.Y + payload.DeltaY;

            System.Diagnostics.Debug.WriteLine($"[ExecuteMove] Transition {transVm.Id}: ({trans.X}, {trans.Y}) + ({payload.DeltaX}, {payload.DeltaY}) = ({newX}, {newY})");

            _undoRedoStack.Push(new ModifyTransitionCommand(transVm.Id, x: newX, y: newY), _document);

            transVm.X = newX;
            transVm.Y = newY;
            RecalculateLinks();
        }
    }

    private void ExecuteSelectOrConnect(object? element)
    {
        if (!IsLinkMode)
        {
            SelectedElement = element;
            return;
        }

        if (element is null) return;

        if (_linkSource is null)
        {
            // First click: store source, highlight it
            _linkSource = element;
            SelectedElement = element;
            return;
        }

        // Second click: validate Step↔Transition pairing
        bool sourceIsStep = _linkSource is StepViewModel;
        bool targetIsStep = element    is StepViewModel;

        if (sourceIsStep == targetIsStep)
        {
            // Invalid pairing (Step→Step or Transition→Transition) — reset source
            _linkSource = element;
            SelectedElement = element;
            return;
        }

        if (_document is null) return;

        int sourceId = _linkSource is StepViewModel sv ? sv.Id : ((TransitionViewModel)_linkSource).Id;
        int targetId = element    is StepViewModel tv ? tv.Id : ((TransitionViewModel)element).Id;

        var link = new GrafcetLink
        {
            SourceId           = sourceId,
            TargetId           = targetId,
            IsStepToTransition = sourceIsStep
        };

        _undoRedoStack.Push(new AddLinkCommand(link), _document);
        LoadFrom(_document);

        IsLinkMode  = false;
        _linkSource = null;
    }

    private void ExecuteCancelLink()
    {
        IsLinkMode      = false;
        _linkSource     = null;
        SelectedElement = null;
    }

    // ── Document loading ──────────────────────────────────────────────────────

    /// <summary>
    /// Rebuilds all canvas collections from the supplied document.
    /// Transition and link positions are computed from step coordinates.
    /// </summary>
    public void LoadFrom(GrafcetDocument doc)
    {
        _document = doc;

        Steps.Clear();
        Transitions.Clear();
        Links.Clear();
        SelectedElement = null;

        // Build step ViewModels and an id→vm lookup
        var stepMap = new Dictionary<int, StepViewModel>(doc.Steps.Count);
        foreach (var step in doc.Steps)
        {
            var vm = new StepViewModel(step);
            Steps.Add(vm);
            stepMap[step.Id] = vm;
        }

        // Build transition ViewModels — position computed as mid-point between steps
        var transMap = new Dictionary<int, TransitionViewModel>(doc.Transitions.Count);
        foreach (var trans in doc.Transitions)
        {
            double tx, ty;

            // Use persisted position when available; fall back to step mid-point
            if (trans.X != 0 || trans.Y != 0)
            {
                tx = trans.X;
                ty = trans.Y;
            }
            else if (stepMap.TryGetValue(trans.FromStepId, out var fromStep) &&
                     stepMap.TryGetValue(trans.ToStepId,   out var toStep))
            {
                tx = fromStep.X;
                ty = (fromStep.Y + STEP_HEIGHT + toStep.Y) / 2.0;
            }
            else
            {
                tx = 0;
                ty = 0;
            }

            var vm = new TransitionViewModel(trans, tx, ty);
            Transitions.Add(vm);
            transMap[trans.Id] = vm;
        }

        // Build link ViewModels — endpoints derived from step/transition positions
        foreach (var link in doc.Links)
            Links.Add(BuildLinkViewModel(link, stepMap, transMap));
    }

    // ── Link helpers ──────────────────────────────────────────────────────────

    /// <summary>
    /// Computes endpoint coordinates for <paramref name="link"/> using the smart
    /// attachment-point logic (top/bottom based on relative Y positions).
    /// </summary>
    private LinkViewModel BuildLinkViewModel(
        GrafcetLink link,
        Dictionary<int, StepViewModel> stepMap,
        Dictionary<int, TransitionViewModel> transMap)
    {
        double startX = 0, startY = 0, endX = 0, endY = 0;

        if (link.IsStepToTransition)
        {
            if (stepMap.TryGetValue(link.SourceId,  out var from) &&
                transMap.TryGetValue(link.TargetId, out var to))
            {
                if (from.Y < to.Y)
                {
                    startX = from.X + STEP_WIDTH / 2; startY = from.Y + STEP_HEIGHT;
                    endX   = to.X   + STEP_WIDTH / 2; endY   = to.Y;
                }
                else
                {
                    startX = from.X + STEP_WIDTH / 2; startY = from.Y;
                    endX   = to.X   + STEP_WIDTH / 2; endY   = to.Y + TRANSITION_HEIGHT;
                }
            }
        }
        else
        {
            if (transMap.TryGetValue(link.SourceId, out var from) &&
                stepMap.TryGetValue(link.TargetId,  out var to))
            {
                if (from.Y < to.Y)
                {
                    startX = from.X + STEP_WIDTH / 2; startY = from.Y + TRANSITION_HEIGHT;
                    endX   = to.X   + STEP_WIDTH / 2; endY   = to.Y;
                }
                else
                {
                    startX = from.X + STEP_WIDTH / 2; startY = from.Y;
                    endX   = to.X   + STEP_WIDTH / 2; endY   = to.Y + STEP_HEIGHT;
                }
            }
        }

        return new LinkViewModel(startX, startY, endX, endY);
    }

    /// <summary>
    /// Rebuilds the <see cref="Links"/> collection from <see cref="_document"/> using the
    /// current ViewModel positions in <see cref="Steps"/> and <see cref="Transitions"/>.
    /// Called after a move to avoid a full <see cref="LoadFrom"/> rebuild (prevents flicker).
    /// </summary>
    private void RecalculateLinks()
    {
        if (_document is null) return;

        var stepMap  = Steps.ToDictionary(s => s.Id);
        var transMap = Transitions.ToDictionary(t => t.Id);

        Links.Clear();
        foreach (var link in _document.Links)
            Links.Add(BuildLinkViewModel(link, stepMap, transMap));
    }

    // ── INavigationAware ──────────────────────────────────────────────────────

    public void OnNavigatedTo(NavigationContext navigationContext) { }
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }
}
