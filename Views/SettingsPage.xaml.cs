using Microsoft.UI.Xaml.Controls;
using SuperMediaSuites.ViewModels;

namespace SuperMediaSuites.Views;

/// <summary>Tab 7 view.</summary>
public sealed partial class SettingsPage : UserControl
{
    public SettingsViewModel ViewModel { get; } = new SettingsViewModel();

    public SettingsPage()
    {
        InitializeComponent();
    }
}
