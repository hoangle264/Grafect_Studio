using System.Collections.ObjectModel;
using GrafcetStudio.Core.Models;
using GrafcetStudio.Core.Models.Document;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;

namespace GrafcetStudio.WPF.ViewModels;

/// <summary>ViewModel for the branch table panel — exposes the full branch list for grid-based editing.</summary>
public class BranchTableViewModel : BindableBase, INavigationAware
{
    private readonly GrafcetDocument _document;
    private BranchRowViewModel? _selectedBranch;

    public ObservableCollection<BranchRowViewModel> Branches { get; } = [];

    public BranchRowViewModel? SelectedBranch
    {
        get => _selectedBranch;
        set
        {
            SetProperty(ref _selectedBranch, value);
            RemoveBranchCommand.RaiseCanExecuteChanged();
        }
    }

    public DelegateCommand AddBranchCommand    { get; }
    public DelegateCommand RemoveBranchCommand { get; }

    public BranchTableViewModel(GrafcetDocument document)
    {
        _document = document;
        AddBranchCommand = new DelegateCommand(ExecuteAddBranch);
        RemoveBranchCommand = new DelegateCommand(ExecuteRemoveBranch, () => SelectedBranch is not null)
            .ObservesProperty(() => SelectedBranch);
    }

    private void ExecuteAddBranch()
    {
        var newBranch = BranchRowViewModel.CreateNew();
        newBranch.Id = Branches.Count == 0 ? 1 : Branches.Max(b => b.Id) + 1;
        Branches.Add(newBranch);
        SelectedBranch = newBranch;
    }

    private void ExecuteRemoveBranch()
    {
        if (SelectedBranch is null) return;
        Branches.Remove(SelectedBranch);
        SelectedBranch = null;
    }

    /// <summary>Clears and reloads <see cref="Branches"/> from the given document.</summary>
    public void LoadFrom(GrafcetDocument doc)
    {
        Branches.Clear();
        foreach (var branch in doc.Branches)
            Branches.Add(new BranchRowViewModel(branch));
    }

    /// <summary>Writes all <see cref="BranchRowViewModel"/> entries back into the document's branch list.</summary>
    public void ApplyTo(GrafcetDocument doc)
    {
        doc.Branches.Clear();
        foreach (var row in Branches)
            doc.Branches.Add(row.ToModel());
    }

    // ── INavigationAware ──────────────────────────────────────────────────────

    public void OnNavigatedTo(NavigationContext navigationContext)   { }
    public void OnNavigatedFrom(NavigationContext navigationContext) { }
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Editable row representing a single GRAFCET branch in the branch table.</summary>
public class BranchRowViewModel : BindableBase
{
    private int        _id;
    private BranchType _type;
    private int        _splitStepId;
    private string     _branchStepIds = "";
    private int        _mergeStepId;
    private string?    _description;

    public BranchRowViewModel(GrafcetBranch branch)
    {
        _id            = branch.Id;
        _type          = branch.Type;
        _splitStepId   = 0;
        _branchStepIds = string.Join(",", branch.StepIds);
        _mergeStepId   = branch.MergeStepId;
        _description   = null;
    }

    private BranchRowViewModel() { }

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public BranchType Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    public int SplitStepId
    {
        get => _splitStepId;
        set => SetProperty(ref _splitStepId, value);
    }

    public string BranchStepIds
    {
        get => _branchStepIds;
        set => SetProperty(ref _branchStepIds, value);
    }

    public int MergeStepId
    {
        get => _mergeStepId;
        set => SetProperty(ref _mergeStepId, value);
    }

    public string? Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    /// <summary>Converts this row back to a plain <see cref="GrafcetBranch"/> model.</summary>
    public GrafcetBranch ToModel() => new()
    {
        Id          = _id,
        Type        = _type,
        MergeStepId = _mergeStepId,
        StepIds     = _branchStepIds
                        .Split(',')
                        .Where(s => int.TryParse(s.Trim(), out _))
                        .Select(s => int.Parse(s.Trim()))
                        .ToList()
    };

    /// <summary>Creates a blank <see cref="BranchRowViewModel"/> with default values.</summary>
    public static BranchRowViewModel CreateNew() => new();
}
