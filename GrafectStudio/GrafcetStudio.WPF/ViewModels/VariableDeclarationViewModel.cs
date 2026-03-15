using GrafcetStudio.Core.Models;
using GrafcetStudio.Core.Models.Variables;
using Prism.Mvvm;

namespace GrafcetStudio.WPF.ViewModels;

/// <summary>ViewModel wrapper around a single <see cref="VariableDeclaration"/> model object.</summary>
public class VariableDeclarationViewModel : BindableBase
{
    private readonly VariableDeclaration _model;

    public VariableDeclarationViewModel(VariableDeclaration model)
    {
        _model = model;
    }

    /// <summary>Creates a new ViewModel backed by an empty declaration.</summary>
    public static VariableDeclarationViewModel CreateNew() =>
        new(new VariableDeclaration());

    /// <summary>Exposes the underlying model for serialization and ApplyTo operations.</summary>
    internal VariableDeclaration Model => _model;

    public string Name
    {
        get => _model.Name;
        set
        {
            if (_model.Name == value) return;
            _model.Name = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(IsValid));
        }
    }

    public PlcDataType DataType
    {
        get => _model.DataType;
        set
        {
            if (_model.DataType == value) return;
            _model.DataType = value;
            RaisePropertyChanged();
        }
    }

    public VariableKind Kind
    {
        get => _model.Kind;
        set
        {
            if (_model.Kind == value) return;
            _model.Kind = value;
            RaisePropertyChanged();
        }
    }

    public string Address
    {
        get => _model.Address;
        set
        {
            if (_model.Address == value) return;
            _model.Address = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(IsValid));
        }
    }

    public string? InitValue
    {
        get => _model.InitValue;
        set
        {
            if (_model.InitValue == value) return;
            _model.InitValue = value;
            RaisePropertyChanged();
        }
    }

    public string? Comment
    {
        get => _model.Comment;
        set
        {
            if (_model.Comment == value) return;
            _model.Comment = value;
            RaisePropertyChanged();
        }
    }

    public string? Group
    {
        get => _model.Group;
        set
        {
            if (_model.Group == value) return;
            _model.Group = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>True when both Name and Address are non-empty.</summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(_model.Name) &&
        !string.IsNullOrWhiteSpace(_model.Address);
}
