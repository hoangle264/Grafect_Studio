using Prism.Mvvm;

namespace GrafcetStudio.WPF.ViewModels.Canvas;

/// <summary>
/// Represents a single draggable item in the Toolbox panel.
/// </summary>
public class ToolboxItemViewModel : BindableBase
{
    private string _label = string.Empty;
    private string _icon  = string.Empty;
    private string _elementType = string.Empty;

    /// <summary>Display name shown below the icon in the toolbox.</summary>
    public string Label
    {
        get => _label;
        set => SetProperty(ref _label, value);
    }

    /// <summary>Unicode character or short text used as the visual icon.</summary>
    public string Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }

    /// <summary>
    /// Identifies the element kind to create on drop.
    /// Valid values: Step | InitialStep | Transition | Link | ParallelBranch | SelectiveBranch
    /// </summary>
    public string ElementType
    {
        get => _elementType;
        set => SetProperty(ref _elementType, value);
    }
}
