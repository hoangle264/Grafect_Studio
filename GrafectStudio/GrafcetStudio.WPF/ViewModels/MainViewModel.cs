using System.IO;
using System.Windows;
using GrafcetStudio.Core.CodeGeneration;
using GrafcetStudio.Core.CodeGeneration.Interfaces;
using GrafcetStudio.Core.Commands;
using GrafcetStudio.Core.Models.Document;
using GrafcetStudio.Core.Models.Variables;
using GrafcetStudio.Core.Services;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation.Regions;

namespace GrafcetStudio.WPF.ViewModels;

/// <summary>
/// Root ViewModel for the application shell.
/// Owns the active <see cref="GrafcetDocument"/> and coordinates
/// undo/redo, file I/O, and code generation.
/// </summary>
public class MainViewModel : BindableBase
{
    private readonly IRegionManager _regionManager;
    private readonly IEventAggregator _eventAggregator;
    private readonly UndoRedoStack _undoRedoStack;
    private readonly CodeGenerationService _codeGenerationService;
    private readonly DocumentSerializer _documentSerializer;

    private readonly GrafcetDocument _document;
    private bool _canUndo;
    private bool _canRedo;
    private string _generatedCode = "";
    private string _selectedTarget = "KeyenceMnemonic";

    public MainViewModel(
        IRegionManager regionManager,
        IEventAggregator eventAggregator,
        UndoRedoStack undoRedoStack,
        CodeGenerationService codeGenerationService,
        DocumentSerializer documentSerializer,
        GrafcetDocument document)
    {
        _regionManager            = regionManager;
        _eventAggregator          = eventAggregator;
        _undoRedoStack            = undoRedoStack;
        _codeGenerationService    = codeGenerationService;
        _documentSerializer       = documentSerializer;
        _document                 = document;

        UndoCommand = new DelegateCommand(ExecuteUndo, () => CanUndo)
            .ObservesProperty(() => CanUndo);

        RedoCommand = new DelegateCommand(ExecuteRedo, () => CanRedo)
            .ObservesProperty(() => CanRedo);

        NewDocumentCommand  = new DelegateCommand(ExecuteNewDocument);
        SaveCommand         = new DelegateCommand(async () => await ExecuteSaveAsync());
        OpenCommand         = new DelegateCommand(async () => await ExecuteOpenAsync());
        GenerateCodeCommand = new DelegateCommand(ExecuteGenerateCode);

        _undoRedoStack.StateChanged += OnUndoRedoStateChanged;
    }

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>The currently active GRAFCET document.</summary>
    /// <summary>The shared singleton document. Content is replaced in-place on New/Open.</summary>
    public GrafcetDocument Document => _document;

    /// <summary>Reflects <see cref="UndoRedoStack.CanUndo"/>; drives UndoCommand.CanExecute.</summary>
    public bool CanUndo
    {
        get => _canUndo;
        private set => SetProperty(ref _canUndo, value);
    }

    /// <summary>Reflects <see cref="UndoRedoStack.CanRedo"/>; drives RedoCommand.CanExecute.</summary>
    public bool CanRedo
    {
        get => _canRedo;
        private set => SetProperty(ref _canRedo, value);
    }

    /// <summary>Last generated code string, ready for display or saving.</summary>
    public string GeneratedCode
    {
        get => _generatedCode;
        private set => SetProperty(ref _generatedCode, value);
    }

    /// <summary>Name of the code generation target (e.g. "KeyenceMnemonic", "StructuredText").</summary>
    public string SelectedTarget
    {
        get => _selectedTarget;
        set => SetProperty(ref _selectedTarget, value);
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    public DelegateCommand UndoCommand          { get; }
    public DelegateCommand RedoCommand          { get; }
    public DelegateCommand NewDocumentCommand   { get; }
    public DelegateCommand SaveCommand          { get; }
    public DelegateCommand OpenCommand          { get; }
    public DelegateCommand GenerateCodeCommand  { get; }

    // ── Command handlers ──────────────────────────────────────────────────────

    private void ExecuteUndo() => _undoRedoStack.Undo(_document);

    private void ExecuteRedo() => _undoRedoStack.Redo(_document);

    private void ExecuteNewDocument()
    {
        _document.Name         = "Untitled";
        _document.VariableTable = new VariableTable();
        _document.Steps        = [];
        _document.Transitions  = [];
        _document.Branches     = [];
        _document.Links        = [];
        _undoRedoStack.Clear();
        GeneratedCode = "";
    }

    private async Task ExecuteSaveAsync()
    {
        var dialog = new SaveFileDialog
        {
            Title      = "Save GRAFCET Document",
            Filter     = $"GRAFCET Studio Files (*{DocumentSerializer.FILE_EXTENSION})|*{DocumentSerializer.FILE_EXTENSION}",
            DefaultExt = DocumentSerializer.FILE_EXTENSION,
            FileName   = _document.Name,
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            await _documentSerializer.SaveAsync(_document, dialog.FileName);
            _document.Name = Path.GetFileNameWithoutExtension(dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task ExecuteOpenAsync()
    {
        var dialog = new OpenFileDialog
        {
            Title      = "Open GRAFCET Document",
            Filter     = $"GRAFCET Studio Files (*{DocumentSerializer.FILE_EXTENSION})|*{DocumentSerializer.FILE_EXTENSION}",
            DefaultExt = DocumentSerializer.FILE_EXTENSION,
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            var loaded = await _documentSerializer.LoadAsync(dialog.FileName);
            _document.Name          = loaded.Name;
            _document.VariableTable = loaded.VariableTable;
            _document.Steps         = loaded.Steps;
            _document.Transitions   = loaded.Transitions;
            _document.Branches      = loaded.Branches;
            _document.Links         = loaded.Links;
            _undoRedoStack.Clear();
            GeneratedCode = "";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Open Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExecuteGenerateCode()
    {
        if (!_codeGenerationService.IsRegistered(_selectedTarget))
        {
            GeneratedCode = $"// No generator registered for target '{_selectedTarget}'.";
            return;
        }

        CodeGenerationResult result = _codeGenerationService
            .Get(_selectedTarget)
            .Generate(_document, new CodeGenOptions());

        GeneratedCode = result.Success
            ? result.Code
            : string.Join(Environment.NewLine, result.Errors.Select(e => $"// ERROR: {e}"));
    }

    // ── UndoRedoStack state sync ──────────────────────────────────────────────

    private void OnUndoRedoStateChanged(object? sender, EventArgs e)
    {
        CanUndo = _undoRedoStack.CanUndo;
        CanRedo = _undoRedoStack.CanRedo;
    }
}
