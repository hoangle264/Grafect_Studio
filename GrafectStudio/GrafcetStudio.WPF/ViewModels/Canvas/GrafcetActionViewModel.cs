using GrafcetStudio.Core.Models;
using GrafcetStudio.Core.Models.Document;
using Prism.Mvvm;

namespace GrafcetStudio.WPF.ViewModels.Canvas;

/// <summary>
/// Editable wrapper around <see cref="GrafcetAction"/> for use in the
/// Properties panel DataGrid. Supports two-way binding via <see cref="SetProperty"/>.
/// </summary>
public class GrafcetActionViewModel : BindableBase
{
    private ActionQualifier _qualifier;
    private string          _variable  = "";
    private string?         _parameter;

    public GrafcetActionViewModel() { }

    public GrafcetActionViewModel(GrafcetAction action)
    {
        _qualifier = action.Qualifier;
        _variable  = action.Variable;
        _parameter = action.Parameter;
    }

    public ActionQualifier Qualifier
    {
        get => _qualifier;
        set => SetProperty(ref _qualifier, value);
    }

    public string Variable
    {
        get => _variable;
        set => SetProperty(ref _variable, value);
    }

    public string? Parameter
    {
        get => _parameter;
        set => SetProperty(ref _parameter, value);
    }

    /// <summary>Converts this view model back to a plain <see cref="GrafcetAction"/> model.</summary>
    public GrafcetAction ToModel() => new()
    {
        Qualifier = _qualifier,
        Variable  = _variable,
        Parameter = _parameter
    };
}
