using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using GrafcetStudio.Core.Models.Variables;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation.Regions;

namespace GrafcetStudio.WPF.ViewModels;

/// <summary>ViewModel for the variable table panel, including filtering and CRUD commands.</summary>
public class VariableTableViewModel : BindableBase, INavigationAware
{
    private readonly IEventAggregator _eventAggregator;
    private readonly ObservableCollection<VariableDeclarationViewModel> _variables = [];

    private VariableDeclarationViewModel? _selectedVariable;
    private string _filterGroup = "";

    public VariableTableViewModel(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;

        FilteredVariables = CollectionViewSource.GetDefaultView(_variables);
        FilteredVariables.Filter = FilterPredicate;

        AddVariableCommand = new DelegateCommand(ExecuteAddVariable);

        RemoveVariableCommand = new DelegateCommand(ExecuteRemoveVariable, () => SelectedVariable is not null)
            .ObservesProperty(() => SelectedVariable);

        DuplicateVariableCommand = new DelegateCommand(ExecuteDuplicateVariable, () => SelectedVariable is not null)
            .ObservesProperty(() => SelectedVariable);
    }

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>Full unfiltered collection backing the variable table.</summary>
    public ObservableCollection<VariableDeclarationViewModel> Variables => _variables;

    public VariableDeclarationViewModel? SelectedVariable
    {
        get => _selectedVariable;
        set => SetProperty(ref _selectedVariable, value);
    }

    /// <summary>When non-empty, only variables whose Group matches are shown.</summary>
    public string FilterGroup
    {
        get => _filterGroup;
        set
        {
            if (SetProperty(ref _filterGroup, value))
                FilteredVariables.Refresh();
        }
    }

    /// <summary>Live-filtered view of <see cref="Variables"/> for binding in the DataGrid.</summary>
    public ICollectionView FilteredVariables { get; }

    // ── Commands ──────────────────────────────────────────────────────────────

    public DelegateCommand AddVariableCommand { get; }
    public DelegateCommand RemoveVariableCommand { get; }
    public DelegateCommand DuplicateVariableCommand { get; }

    // ── Command handlers ──────────────────────────────────────────────────────

    private void ExecuteAddVariable()
    {
        var vm = VariableDeclarationViewModel.CreateNew();
        _variables.Add(vm);
        SelectedVariable = vm;
    }

    private void ExecuteRemoveVariable()
    {
        if (_selectedVariable is null) return;
        int index = _variables.IndexOf(_selectedVariable);
        _variables.Remove(_selectedVariable);
        SelectedVariable = _variables.Count > 0
            ? _variables[Math.Clamp(index, 0, _variables.Count - 1)]
            : null;
    }

    private void ExecuteDuplicateVariable()
    {
        if (_selectedVariable is null) return;
        var clone = new VariableDeclarationViewModel(new VariableDeclaration
        {
            Name      = _selectedVariable.Name + "_Copy",
            DataType  = _selectedVariable.DataType,
            Kind      = _selectedVariable.Kind,
            Address   = _selectedVariable.Address,
            InitValue = _selectedVariable.InitValue,
            Comment   = _selectedVariable.Comment,
            Group     = _selectedVariable.Group,
        });
        int index = _variables.IndexOf(_selectedVariable);
        _variables.Insert(index + 1, clone);
        SelectedVariable = clone;
    }

    // ── Load / Apply ──────────────────────────────────────────────────────────

    /// <summary>Populates the collection from a <see cref="VariableTable"/> model.</summary>
    public void LoadFrom(VariableTable table)
    {
        _variables.Clear();
        foreach (var v in table.Variables)
            _variables.Add(new VariableDeclarationViewModel(v));
    }

    /// <summary>Writes all current ViewModels back into a <see cref="VariableTable"/> model.</summary>
    public void ApplyTo(VariableTable table)
    {
        table.Variables.Clear();
        foreach (var vm in _variables)
            table.Variables.Add(vm.Model);
    }

    // ── Filter ────────────────────────────────────────────────────────────────

    private bool FilterPredicate(object item) =>
        string.IsNullOrWhiteSpace(_filterGroup) ||
        (item is VariableDeclarationViewModel vm &&
         string.Equals(vm.Group, _filterGroup, StringComparison.OrdinalIgnoreCase));

    // ── INavigationAware ──────────────────────────────────────────────────────

    public void OnNavigatedTo(NavigationContext navigationContext) { }
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }
}
