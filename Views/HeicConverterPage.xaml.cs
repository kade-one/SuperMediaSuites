using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SuperMediaSuites.Helpers;
using SuperMediaSuites.ViewModels;

namespace SuperMediaSuites.Views;

/// <summary>Tab 4 view.</summary>
public sealed partial class HeicConverterPage : UserControl
{
    public HeicConverterViewModel ViewModel { get; } = new HeicConverterViewModel();

    public HeicConverterPage()
    {
        InitializeComponent();
    }

    private void OnDragOver(object sender, DragEventArgs e) => DropHandler.MarkCopy(e);

    private async void OnDrop(object sender, DragEventArgs e) =>
        await DropHandler.AcceptAsync(e, ViewModel.AddPathsAsync);

    private void OnRemoveItemClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Microsoft.UI.Xaml.FrameworkElement element && element.DataContext is SuperMediaSuites.Models.ConversionItem item)
        {
            ViewModel.RemoveItemCommand.Execute(item);
        }
    }
}
