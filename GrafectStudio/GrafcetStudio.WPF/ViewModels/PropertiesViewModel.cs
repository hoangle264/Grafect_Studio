using GrafcetStudio.WPF.Events;
using GrafcetStudio.WPF.ViewModels.Canvas;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation.Regions;

namespace GrafcetStudio.WPF.ViewModels;

/// <summary>
/// ViewModel for the Properties panel.
/// Reacts to <see cref="ElementSelectedEvent"/> and exposes the selected
/// element's data for display and two-way editing.
/// </summary>
public class PropertiesViewModel : BindableBase, INavigationAware
{
    private StepViewModel?       _selectedStep;
    private TransitionViewModel? _selectedTransition;
    private bool _isStepSelected;
    private bool _isTransitionSelected;

    public PropertiesViewModel(IEventAggregator eventAggregator)
    {
        eventAggregator
            .GetEvent<ElementSelectedEvent>()
            .Subscribe(OnElementSelected, ThreadOption.UIThread);
    }

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>Header label: "Step" | "Transition" | "Nothing selected".</summary>
    public string ElementType =>
        _isStepSelected       ? "Step"       :
        _isTransitionSelected ? "Transition" :
                                "Nothing selected";

    /// <summary>Primary value: step name or transition condition.</summary>
    public string DisplayName =>
        _isStepSelected       ? _selectedStep?.Name      ?? "" :
        _isTransitionSelected ? _selectedTransition?.Condition ?? "" :
                                "";

    public bool IsStepSelected
    {
        get => _isStepSelected;
        private set
        {
            if (SetProperty(ref _isStepSelected, value))
            {
                RaisePropertyChanged(nameof(ElementType));
                RaisePropertyChanged(nameof(DisplayName));
                RaisePropertyChanged(nameof(IsNothingSelected));
            }
        }
    }

    public bool IsTransitionSelected
    {
        get => _isTransitionSelected;
        private set
        {
            if (SetProperty(ref _isTransitionSelected, value))
            {
                RaisePropertyChanged(nameof(ElementType));
                RaisePropertyChanged(nameof(DisplayName));
                RaisePropertyChanged(nameof(IsNothingSelected));
            }
        }
    }

    /// <summary>True when no element is selected; drives the fallback prompt visibility.</summary>
    public bool IsNothingSelected => !_isStepSelected && !_isTransitionSelected;

    public StepViewModel? SelectedStep
    {
        get => _selectedStep;
        private set => SetProperty(ref _selectedStep, value);
    }

    public TransitionViewModel? SelectedTransition
    {
        get => _selectedTransition;
        private set => SetProperty(ref _selectedTransition, value);
    }

    // ── ElementSelectedEvent handler ──────────────────────────────────────────

    private void OnElementSelected(object? payload)
    {
        if (payload is StepViewModel step)
        {
            SelectedStep         = step;
            SelectedTransition   = null;
            IsStepSelected       = true;
            IsTransitionSelected = false;
        }
        else if (payload is TransitionViewModel trans)
        {
            SelectedStep         = null;
            SelectedTransition   = trans;
            IsStepSelected       = false;
            IsTransitionSelected = true;
        }
        else
        {
            SelectedStep         = null;
            SelectedTransition   = null;
            IsStepSelected       = false;
            IsTransitionSelected = false;
        }
    }

    // ── INavigationAware ──────────────────────────────────────────────────────

    public void OnNavigatedTo(NavigationContext navigationContext) { }
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }
}
