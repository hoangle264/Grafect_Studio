using System.IO;
using System.Windows;
using GrafcetStudio.Core.CodeGeneration;
using GrafcetStudio.Core.CodeGeneration.Interfaces;
using GrafcetStudio.Core.Models.Document;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;

namespace GrafcetStudio.WPF.ViewModels;

/// <summary>
/// ViewModel for the Code Output panel.
/// Drives target selection, code generation, clipboard copy and file save.
/// </summary>
public class CodeOutputViewModel : BindableBase, INavigationAware
{
    private readonly CodeGenerationService _codeGenerationService;
    private readonly GrafcetDocument _document;

    private string _selectedTarget = "";
    private string _generatedCode  = "";

    public CodeOutputViewModel(
        CodeGenerationService codeGenerationService,
        GrafcetDocument       document)
    {
        _codeGenerationService = codeGenerationService;
        _document              = document;

        _selectedTarget = _codeGenerationService.AvailableTargets.FirstOrDefault() ?? "";

        GenerateCommand = new DelegateCommand(ExecuteGenerate,
                () => !string.IsNullOrEmpty(SelectedTarget))
            .ObservesProperty(() => SelectedTarget);

        CopyCommand = new DelegateCommand(ExecuteCopy,
                () => !string.IsNullOrEmpty(GeneratedCode))
            .ObservesProperty(() => GeneratedCode);

        SaveCommand = new DelegateCommand(ExecuteSave,
                () => !string.IsNullOrEmpty(GeneratedCode))
            .ObservesProperty(() => GeneratedCode);
    }

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>All code generation targets registered with the service.</summary>
    public IEnumerable<string> AvailableTargets => _codeGenerationService.AvailableTargets;

    /// <summary>Currently selected generation target name.</summary>
    public string SelectedTarget
    {
        get => _selectedTarget;
        set => SetProperty(ref _selectedTarget, value);
    }

    /// <summary>The last generated code string (read-only in the View).</summary>
    public string GeneratedCode
    {
        get => _generatedCode;
        private set => SetProperty(ref _generatedCode, value);
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    public DelegateCommand GenerateCommand { get; }
    public DelegateCommand CopyCommand     { get; }
    public DelegateCommand SaveCommand     { get; }

    // ── Command handlers ──────────────────────────────────────────────────────

    private void ExecuteGenerate()
    {
        if (string.IsNullOrEmpty(_selectedTarget)) return;

        var result = _codeGenerationService
            .Get(_selectedTarget)
            .Generate(_document, new CodeGenOptions());

        GeneratedCode = result.Success
            ? result.Code
            : string.Join(Environment.NewLine, result.Errors.Select(e => $"// ERROR: {e}"));
    }

    private void ExecuteCopy()
    {
        if (!string.IsNullOrEmpty(_generatedCode))
            Clipboard.SetText(_generatedCode);
    }

    private void ExecuteSave()
    {
        if (string.IsNullOrEmpty(_generatedCode)) return;

        var ext = _codeGenerationService.IsRegistered(_selectedTarget)
            ? _codeGenerationService.Get(_selectedTarget).FileExtension
            : ".txt";

        var dialog = new SaveFileDialog
        {
            Title      = "Save Generated Code",
            Filter     = $"Generated file (*{ext})|*{ext}|All files (*.*)|*.*",
            DefaultExt = ext,
        };

        if (dialog.ShowDialog() == true)
            File.WriteAllText(dialog.FileName, _generatedCode);
    }

    // ── INavigationAware ──────────────────────────────────────────────────────

    public void OnNavigatedTo(NavigationContext navigationContext) { }
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }
}
