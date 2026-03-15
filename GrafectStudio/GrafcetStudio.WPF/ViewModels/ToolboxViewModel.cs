using System.Collections.ObjectModel;
using System.Windows;
using GrafcetStudio.WPF.ViewModels.Canvas;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;

namespace GrafcetStudio.WPF.ViewModels;

/// <summary>
/// ViewModel for the Toolbox panel.
/// Exposes the fixed set of GRAFCET element templates and handles drag initiation.
/// </summary>
public class ToolboxViewModel : BindableBase, INavigationAware
{
    public ToolboxViewModel()
    {
        Items = new ObservableCollection<ToolboxItemViewModel>
        {
            new() { Label = "Step",             Icon = "□", ElementType = "Step"            },
            new() { Label = "Initial Step",     Icon = "◎", ElementType = "InitialStep"     },
            new() { Label = "Transition",       Icon = "━", ElementType = "Transition"      },
            new() { Label = "Link",             Icon = "↕", ElementType = "Link"            },
            new() { Label = "Parallel Branch",  Icon = "═", ElementType = "ParallelBranch"  },
            new() { Label = "Selective Branch", Icon = "┤", ElementType = "SelectiveBranch" },
        };

        BeginDragCommand = new DelegateCommand<ToolboxItemViewModel>(ExecuteBeginDrag);
    }

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>Fixed list of draggable element templates shown in the toolbox.</summary>
    public ObservableCollection<ToolboxItemViewModel> Items { get; }

    // ── Commands ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Starts a WPF drag-and-drop operation for the given toolbox item.
    /// The drag payload is the item's <see cref="ToolboxItemViewModel.ElementType"/> string.
    /// </summary>
    public DelegateCommand<ToolboxItemViewModel> BeginDragCommand { get; }

    // ── Command handlers ──────────────────────────────────────────────────────

    private static void ExecuteBeginDrag(ToolboxItemViewModel? item)
    {
        if (item is null) return;

        var data = new DataObject(typeof(string), item.ElementType);
        DragDrop.DoDragDrop(Application.Current.MainWindow, data, DragDropEffects.Copy);
    }

    // ── INavigationAware ──────────────────────────────────────────────────────

    public void OnNavigatedTo(NavigationContext navigationContext) { }
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }
}
