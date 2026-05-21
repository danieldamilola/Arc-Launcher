using System.Windows;
using System.Windows.Controls;

namespace Flow.Views;

public partial class SearchBar : UserControl
{
    public SearchBar()
    {
        InitializeComponent();
        Loaded += (_, _) => SearchInput.Focus();
    }

    private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        var hasText = !string.IsNullOrEmpty(SearchInput.Text);
        Placeholder.Visibility = hasText ? Visibility.Collapsed : Visibility.Visible;
        HintBadge.Visibility = hasText ? Visibility.Collapsed : Visibility.Visible;
    }
}
