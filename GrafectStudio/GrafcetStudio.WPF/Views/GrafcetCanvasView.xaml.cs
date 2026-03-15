using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GrafcetStudio.WPF.ViewModels;
using GrafcetStudio.WPF.ViewModels.Canvas;

namespace GrafcetStudio.WPF.Views;

public partial class GrafcetCanvasView : UserControl
{
    private bool _isDragging = false;
    private Point _dragStartPoint;
    private object? _draggingElement;

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

    // ── Drag-to-move: mouse capture pattern ──────────────────────────────────

    private void OnElementMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;

        e.Handled = true;
        var element = (sender as FrameworkElement)?.DataContext;
        if (element is null) return;

        if (DataContext is GrafcetCanvasViewModel vm)
            vm.SelectElementCommand.Execute(element);

        var canvasGrid = GetCanvasGrid();
        if (canvasGrid is null) return;

        _draggingElement = element;
        _dragStartPoint = e.GetPosition(canvasGrid);
        (sender as FrameworkElement)?.CaptureMouse();
        _isDragging = true;

        System.Diagnostics.Debug.WriteLine($"[Drag] Start: element={element?.GetType().Name}, pos=({_dragStartPoint.X}, {_dragStartPoint.Y})");
    }

    private void OnElementMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || _draggingElement is null) return;

        var canvasGrid = GetCanvasGrid();
        if (canvasGrid is null) return;

        var current = e.GetPosition(canvasGrid);
        double dx = current.X - _dragStartPoint.X;
        double dy = current.Y - _dragStartPoint.Y;

        // Only trigger move if delta is significant (avoid jitter)
        if (Math.Abs(dx) < 1 && Math.Abs(dy) < 1) return;

        if (DataContext is GrafcetCanvasViewModel vm)
        {
            vm.MoveElementCommand.Execute(new MovePayload(_draggingElement, dx, dy));
            System.Diagnostics.Debug.WriteLine($"[Drag] Move: delta=({dx:F1}, {dy:F1})");
        }

        _dragStartPoint = current;
    }

    private void OnElementMouseUp(object sender, MouseButtonEventArgs e)
    {
        _isDragging      = false;
        _draggingElement = null;
        (sender as FrameworkElement)?.ReleaseMouseCapture();
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    /// <summary>Returns the 3000×3000 Canvas Grid inside the ScrollViewer.</summary>
    private Grid? GetCanvasGrid()
    {
        var scrollViewer = FindVisualChild<ScrollViewer>(this);
        if (scrollViewer is null) return null;
        return FindVisualChild<Grid>(scrollViewer);
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T result) return result;
            var found = FindVisualChild<T>(child);
            if (found is not null) return found;
        }
        return null;
    }
}

