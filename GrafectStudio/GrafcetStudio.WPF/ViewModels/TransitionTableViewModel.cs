using System.Collections.ObjectModel;
using GrafcetStudio.Core.Models.Document;
using GrafcetStudio.WPF.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation.Regions;

namespace GrafcetStudio.WPF.ViewModels;

/// <summary>ViewModel for the transition table panel — exposes the full transition list for grid-based editing.</summary>
public class TransitionTableViewModel : BindableBase, INavigationAware
{
    private readonly GrafcetDocument  _document;
    private readonly IEventAggregator  _eventAggregator;
    private TransitionRowViewModel? _selectedTransition;

    public ObservableCollection<TransitionRowViewModel> Transitions { get; } = [];

    public TransitionRowViewModel? SelectedTransition
    {
        get => _selectedTransition;
        set
        {
            SetProperty(ref _selectedTransition, value);
            RemoveTransitionCommand.RaiseCanExecuteChanged();
        }
    }

    public DelegateCommand AddTransitionCommand    { get; }
    public DelegateCommand RemoveTransitionCommand { get; }

    public TransitionTableViewModel(GrafcetDocument document, IEventAggregator eventAggregator)
    {
        _document        = document;
        _eventAggregator = eventAggregator;
        AddTransitionCommand = new DelegateCommand(ExecuteAddTransition);
        RemoveTransitionCommand = new DelegateCommand(ExecuteRemoveTransition, () => SelectedTransition is not null)
            .ObservesProperty(() => SelectedTransition);

        _eventAggregator
            .GetEvent<DocumentChangedEvent>()
            .Subscribe(OnDocumentChanged);
    }

    private void OnDocumentChanged()
    {
        LoadFrom(_document);
    }

    private void ExecuteAddTransition()
    {
        var newTrans = TransitionRowViewModel.CreateNew();
        newTrans.Id = Transitions.Count == 0 ? 1 : Transitions.Max(t => t.Id) + 1;
        Transitions.Add(newTrans);
        SelectedTransition = newTrans;
        ApplyTo(_document);
        _eventAggregator.GetEvent<DocumentChangedEvent>().Publish();
    }

    private void ExecuteRemoveTransition()
    {
        if (SelectedTransition is null) return;
        Transitions.Remove(SelectedTransition);
        SelectedTransition = null;
        ApplyTo(_document);
        _eventAggregator.GetEvent<DocumentChangedEvent>().Publish();
    }

    /// <summary>Clears and reloads <see cref="Transitions"/> from the given document.</summary>
    public void LoadFrom(GrafcetDocument doc)
    {
        Transitions.Clear();
        foreach (var transition in doc.Transitions)
            Transitions.Add(new TransitionRowViewModel(transition));
    }

    /// <summary>Writes all <see cref="TransitionRowViewModel"/> entries back into the document's transition list.</summary>
    public void ApplyTo(GrafcetDocument doc)
    {
        doc.Transitions.Clear();
        foreach (var row in Transitions)
        {
            doc.Transitions.Add(new GrafcetTransition
            {
                Id         = row.Id,
                FromStepId = row.FromStepId,
                ToStepId   = row.ToStepId,
                Condition  = row.Condition
            });
        }
    }

    // ── INavigationAware ──────────────────────────────────────────────────────

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        LoadFrom(_document);
    }
    public void OnNavigatedFrom(NavigationContext navigationContext) { }
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Editable row representing a single GRAFCET transition in the transition table.</summary>
public class TransitionRowViewModel : BindableBase
{
    private int    _id;
    private int    _fromStepId;
    private int    _toStepId;
    private string _condition = "TRUE";

    public TransitionRowViewModel(GrafcetTransition transition)
    {
        _id         = transition.Id;
        _fromStepId = transition.FromStepId;
        _toStepId   = transition.ToStepId;
        _condition  = transition.Condition;
    }

    private TransitionRowViewModel() { }

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public int FromStepId
    {
        get => _fromStepId;
        set => SetProperty(ref _fromStepId, value);
    }

    public int ToStepId
    {
        get => _toStepId;
        set => SetProperty(ref _toStepId, value);
    }

    public string Condition
    {
        get => _condition;
        set => SetProperty(ref _condition, value);
    }

    /// <summary>Creates a blank <see cref="TransitionRowViewModel"/> with Id = 0 and default condition TRUE.</summary>
    public static TransitionRowViewModel CreateNew() => new();
}
