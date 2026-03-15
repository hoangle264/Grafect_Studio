using System.Windows;
using Prism.Navigation.Regions;

namespace GrafcetStudio.WPF.Views;

public partial class MainWindow : Window
{
    public MainWindow(IRegionManager regionManager)
    {
        InitializeComponent();
    }
}
