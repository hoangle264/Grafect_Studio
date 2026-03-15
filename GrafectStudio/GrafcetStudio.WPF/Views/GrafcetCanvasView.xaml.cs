using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GrafcetStudio.WPF.ViewModels;
using GrafcetStudio.WPF.ViewModels.Canvas;

namespace GrafcetStudio.WPF.Views;

public partial class GrafcetCanvasView : UserControl
{
    public GrafcetCanvasView()
    {
        InitializeComponent();
    }

    // ── Drag-over: show Copy cursor only when payload is a valid element type ─

    private void OnCanvasDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(typeof(string))
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    // ── Drop: resolve element type, compute canvas position, forward to VM ────

    private void OnCanvasDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(typeof(string)) is not string elementType) return;

        var pos = e.GetPosition(sender as IInputElement);

        if (DataContext is GrafcetCanvasViewModel vm)
            vm.DropElementCommand.Execute(new DropPayload(elementType, pos.X, pos.Y));
    }
}

