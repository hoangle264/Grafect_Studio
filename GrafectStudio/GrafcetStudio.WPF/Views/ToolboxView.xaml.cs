using System.Windows.Controls;
using System.Windows.Input;
using GrafcetStudio.WPF.ViewModels;
using GrafcetStudio.WPF.ViewModels.Canvas;

namespace GrafcetStudio.WPF.Views;

public partial class ToolboxView : UserControl
{
    public ToolboxView()
    {
        InitializeComponent();
    }

    // Triggered by EventSetter on each ListBoxItem.
    // Resolves the ToolboxItemViewModel from DataContext and delegates to BeginDragCommand.
    private void OnItemMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListBoxItem { DataContext: ToolboxItemViewModel item } &&
            DataContext is ToolboxViewModel vm)
        {
            vm.BeginDragCommand.Execute(item);
        }
    }
}
