using Microsoft.UI.Xaml.Controls;
using SuperMediaSuites.ViewModels;

namespace SuperMediaSuites.Views;

/// <summary>Tab 5 view.</summary>
public sealed partial class CdRipperPage : UserControl
{
    public CdRipperViewModel ViewModel { get; } = new CdRipperViewModel();

    public CdRipperPage()
    {
        InitializeComponent();
        Loaded += (_, _) => ViewModel.RefreshDrivesCommand.Execute(null);
    }
}
