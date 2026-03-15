using System.Windows;
using GrafcetStudio.Core.CodeGeneration;
using GrafcetStudio.Core.CodeGeneration.Keyence;
using GrafcetStudio.Core.CodeGeneration.StructuredText;
using GrafcetStudio.Core.Commands;
using GrafcetStudio.Core.Models.Document;
using GrafcetStudio.Core.Services;
using GrafcetStudio.WPF.ViewModels;
using GrafcetStudio.WPF.Views;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Navigation.Regions;

namespace GrafcetStudio.WPF;

public partial class App : PrismApplication
{
    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // ── Core singletons ───────────────────────────────────────────────────
        containerRegistry.RegisterSingleton<GrafcetDocument>();
        containerRegistry.RegisterSingleton<UndoRedoStack>();
        containerRegistry.RegisterSingleton<DocumentSerializer>();
        containerRegistry.RegisterSingleton<ToolCallFactory>();
        containerRegistry.RegisterSingleton<ToolCallExecutor>();
        containerRegistry.RegisterSingleton<CodeGenerationService>();

        // ── Navigation registrations ──────────────────────────────────────────
        containerRegistry.RegisterForNavigation<ToolboxView,        ToolboxViewModel>();
        containerRegistry.RegisterForNavigation<GrafcetCanvasView,  GrafcetCanvasViewModel>();
        containerRegistry.RegisterForNavigation<VariableTableView,  VariableTableViewModel>();
        containerRegistry.RegisterForNavigation<CodeOutputView,     CodeOutputViewModel>();
        containerRegistry.RegisterForNavigation<PropertiesView,     PropertiesViewModel>();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        // Register code generators with default options
        var codeService = Container.Resolve<CodeGenerationService>();
        codeService.Register(new KeyenceMnemonicGenerator());
        codeService.Register(new StructuredTextGenerator());

        // Navigate each region to its initial view
        var regionManager = Container.Resolve<IRegionManager>();
        regionManager.RequestNavigate("ToolboxRegion",     nameof(ToolboxView));
        regionManager.RequestNavigate("CanvasRegion",      nameof(GrafcetCanvasView));
        regionManager.RequestNavigate("VariablesRegion",   nameof(VariableTableView));
        regionManager.RequestNavigate("CodeRegion",        nameof(CodeOutputView));
        regionManager.RequestNavigate("PropertiesRegion",  nameof(PropertiesView));
    }
}
