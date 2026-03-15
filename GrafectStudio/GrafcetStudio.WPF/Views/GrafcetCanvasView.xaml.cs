using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GrafcetStudio.Core.Models;
using GrafcetStudio.WPF.ViewModels;
using GrafcetStudio.WPF.ViewModels.Canvas;

namespace GrafcetStudio.WPF.Views;

public partial class GrafcetCanvasView : UserControl
{
    private bool _isDragging = false;
    private Point _dragStartPoint;
    private object? _draggingElement;

    // Link-drag state
    private object? _linkDragSource;
    private Point   _linkDragStartPoint;
    private const double LINK_DRAG_THRESHOLD = 5.0;

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

        // Record link-drag source (only for step/transition elements)
        if (element is StepViewModel or TransitionViewModel)
        {
            _linkDragSource     = element;
            _linkDragStartPoint = _dragStartPoint;
        }

        System.Diagnostics.Debug.WriteLine($"[Drag] Start: element={element?.GetType().Name}, pos=({_dragStartPoint.X}, {_dragStartPoint.Y})");
    }

    private void OnElementMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || _draggingElement is null) return;

        var canvasGrid = GetCanvasGrid();
        if (canvasGrid is null) return;

        var current = e.GetPosition(canvasGrid);

        // Check if link-drag threshold exceeded → switch from move-drag to link-drag
        // Only in Connect mode; in Select mode the threshold block must be skipped entirely
        // so move-drag can proceed without being cancelled after 5 px.
        if (_linkDragSource is not null &&
            DataContext is GrafcetCanvasViewModel linkVm &&
            linkVm.CurrentMode == CanvasMode.Connect)
        {
            double totalDx = current.X - _linkDragStartPoint.X;
            double totalDy = current.Y - _linkDragStartPoint.Y;
            if (Math.Sqrt(totalDx * totalDx + totalDy * totalDy) >= LINK_DRAG_THRESHOLD)
            {
                // Release move-drag capture so PreviewMouseMove on the canvas Grid fires
                (sender as FrameworkElement)?.ReleaseMouseCapture();
                _isDragging      = false;
                _draggingElement = null;

                if (linkVm.ActiveDrag is null)
                    linkVm.BeginDrag(_linkDragSource, _linkDragStartPoint.X, _linkDragStartPoint.Y);
                linkVm.UpdateDrag(current.X, current.Y, null);
                return;
            }
        }

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

        // Keep _linkDragSource alive when an active link-drag is in progress so that
        // OnCanvasMouseUp (which bubbles next) can still call EndDrag.
        // Only clear it when there is no active drag (i.e. a plain click).
        if (DataContext is not GrafcetCanvasViewModel vm || vm.ActiveDrag is null)
            _linkDragSource = null;
    }

    // ── Link-drag canvas handlers ─────────────────────────────────────────────

    private void OnCanvasMouseMove(object sender, MouseEventArgs e)
    {
        if (_linkDragSource is null) return;
        if (DataContext is not GrafcetCanvasViewModel vm) return;
        if (vm.ActiveDrag is null) return;

        var canvasGrid = GetCanvasGrid();
        if (canvasGrid is null) return;

        var pos = e.GetPosition(canvasGrid);
        vm.UpdateDrag(pos.X, pos.Y, HitTestElement(pos));
    }

    private void OnCanvasMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not GrafcetCanvasViewModel vm) return;

        // vm.ActiveDrag is the authoritative signal that a link-drag is in progress.
        // We cannot rely on _linkDragSource alone because OnElementMouseUp (which fires
        // first when the mouse is released over a child element) may have already
        // cleared it.
        if (vm.ActiveDrag is not null)
        {
            var canvasGrid = GetCanvasGrid();
            var pos        = canvasGrid is not null ? e.GetPosition(canvasGrid) : new Point();
            vm.EndDrag(HitTestElement(pos));
        }
        else if (_linkDragSource is not null)
        {
            vm.ResetDrag();
        }

        _linkDragSource = null;
    }

    /// <summary>
    /// Uses <see cref="VisualTreeHelper.HitTest"/> to find the first
    /// <see cref="StepViewModel"/> or <see cref="TransitionViewModel"/> DataContext
    /// at <paramref name="canvasPos"/> within the canvas Grid.
    /// </summary>
    private object? HitTestElement(Point canvasPos)
    {
        var canvasGrid = GetCanvasGrid();
        if (canvasGrid is null) return null;

        object? found = null;
        VisualTreeHelper.HitTest(
            canvasGrid,
            null,
            result =>
            {
                var obj = result.VisualHit;
                while (obj is not null && obj != canvasGrid)
                {
                    if (obj is FrameworkElement fe &&
                        (fe.DataContext is StepViewModel || fe.DataContext is TransitionViewModel))
                    {
                        found = fe.DataContext;
                        return HitTestResultBehavior.Stop;
                    }
                    obj = VisualTreeHelper.GetParent(obj);
                }
                return HitTestResultBehavior.Continue;
            },
            new PointHitTestParameters(canvasPos));

        return found;
    }

    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseDown(e);
        // Ensure the UserControl has keyboard focus after any click on the canvas,
        // including clicks on links (which use MouseBinding, not code-behind events).
        this.Focus();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (DataContext is not GrafcetCanvasViewModel vm) return;

        if (e.Key == Key.Escape)
        {
            if (vm.IsLinkMode)
                vm.CancelLinkCommand.Execute();
            else if (vm.ActiveDrag is not null)
            {
                vm.ResetDrag();
                _linkDragSource = null;
            }
            e.Handled = true;
        }
        else if (e.Key == Key.Delete && vm.DeleteCommand.CanExecute())
        {
            vm.DeleteCommand.Execute();
            e.Handled = true;
        }
    }

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

