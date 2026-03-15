using System.ComponentModel;
using System.IO;
using System.Windows;
using AvalonDock.Layout.Serialization;
using GrafcetStudio.WPF.Events;
using GrafcetStudio.WPF.ViewModels;
using Prism.Events;
using Prism.Navigation.Regions;

namespace GrafcetStudio.WPF.Views;

public partial class MainWindow : Window
{
    private readonly IEventAggregator _eventAggregator;

    public MainWindow(IRegionManager regionManager, IEventAggregator eventAggregator)
    {
        InitializeComponent();
        _eventAggregator = eventAggregator;
        Loaded  += OnLoaded;
        Closing += OnClosing;
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void OnLoaded(object sender, RoutedEventArgs e) => LoadLayout();

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        using var sw = new StringWriter();
        new XmlLayoutSerializer(DockManager).Serialize(sw);
        _eventAggregator.GetEvent<WindowClosingEvent>().Publish(sw.ToString());
    }

    // ── Layout helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Applies the layout XML cached by the ViewModel to the DockingManager.
    /// No-ops if no saved layout exists.
    /// </summary>
    private void LoadLayout()
    {
        if (DataContext is not MainViewModel vm) return;
        var xml = vm.CachedLayoutXml;
        if (xml is null) return;

        try
        {
            using var sr = new StringReader(xml);
            new XmlLayoutSerializer(DockManager).Deserialize(sr);
        }
        catch
        {
            // Corrupted layout file — keep the default layout defined in XAML
        }
    }
}

