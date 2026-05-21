using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Flow.Models;
using Flow.Services;

namespace Flow.Extensions;

/// <summary>
/// Displays clipboard history (up to 20 items). Click any item to copy it back to the clipboard.
/// </summary>
public partial class ClipboardExtension : UserControl, IExtension
{
    public string Id => "clipboard";
    public string DisplayName => "Clipboard History";
    public object Trigger => "clip";

    public ClipboardExtension()
    {
        InitializeComponent();
    }

    public UserControl CreateControl(string query)
    {
        var control = new ClipboardExtension();
        control.LoadHistory();
        return control;
    }

    private void LoadHistory()
    {
        var history = ClipboardService.GetHistory();

        if (history.Count == 0)
        {
            EmptyLabel.Visibility = Visibility.Visible;
        }
        else
        {
            EmptyLabel.Visibility = Visibility.Collapsed;
            ClipboardItemsControl.ItemsSource = history;
        }
    }

    private void OnItemClicked(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.Tag is ClipboardItem item)
        {
            ClipboardService.CopyToClipboard(item.Content);

            // Brief visual feedback: flash the border
            border.Background = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)FindResource("Accent"));
        }
    }
}
