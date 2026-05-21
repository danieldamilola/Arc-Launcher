using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Flow.Views;

public partial class ResultItem : UserControl
{
    private static Brush? _hoverBrush;
    private static Brush? _selectedBrush;
    private static Brush? _selectedTextBrush;
    private static Brush? _normalTextBrush;
    private static Brush? _mutedBrush;

    public ResultItem()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _hoverBrush      ??= Application.Current.TryFindResource("HoverBg")      as Brush ?? new SolidColorBrush(Color.FromRgb(0x1a, 0x1a, 0x1a));
        _selectedBrush   ??= Application.Current.TryFindResource("SelectedBg")   as Brush ?? new SolidColorBrush(Color.FromRgb(0xf2, 0xf2, 0xf2));
        _selectedTextBrush ??= Application.Current.TryFindResource("SelectedText") as Brush ?? Brushes.Black;
        _normalTextBrush ??= Application.Current.TryFindResource("TextPrimary")  as Brush ?? new SolidColorBrush(Color.FromRgb(0xf2, 0xf2, 0xf2));
        _mutedBrush      ??= Application.Current.TryFindResource("TextSecondary") as Brush ?? new SolidColorBrush(Color.FromRgb(0x9a, 0x9a, 0x9a));
    }

    private void OnItemClicked(object sender, MouseButtonEventArgs e)
    {
        var vm = FindParentViewModel();
        if (vm is null || DataContext is not Models.SearchResult result) return;

        int idx = -1;
        for (int i = 0; i < vm.Results.Count; i++)
        {
            if (ReferenceEquals(vm.Results[i], result)) { idx = i; break; }
        }

        if (idx >= 0)
        {
            vm.SelectedIndex = idx;
            vm.OpenSelectedCommand.Execute(null);
        }
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        var vm = FindParentViewModel();
        bool isSelected = IsCurrentlySelected(vm);
        if (!isSelected)
            RowBg.Background = _hoverBrush;

        EnterHint.Visibility = Visibility.Visible;
        ShowSubtitleIfPresent();
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        var vm = FindParentViewModel();
        bool isSelected = IsCurrentlySelected(vm);
        if (!isSelected)
        {
            RowBg.Background = Brushes.Transparent;
            EnterHint.Visibility = Visibility.Collapsed;
            SubtitleText.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>Called by ResultsList when keyboard selection changes.</summary>
    public void SetSelected(bool selected)
    {
        RowBg.Background   = selected ? _selectedBrush   : Brushes.Transparent;
        NameText.Foreground = selected ? _selectedTextBrush : _normalTextBrush;
        SubtitleText.Foreground = selected
            ? new SolidColorBrush(Color.FromArgb(0x80, 0, 0, 0))
            : _mutedBrush;

        AccentRail.Visibility  = selected ? Visibility.Visible  : Visibility.Collapsed;
        EnterHint.Visibility   = selected ? Visibility.Visible  : Visibility.Collapsed;

        if (selected)
            ShowSubtitleIfPresent();
        else
            SubtitleText.Visibility = Visibility.Collapsed;

        if (selected && EnterHint.Background is SolidColorBrush)
        {
            var eb = Application.Current.TryFindResource("BgContainerHigh") as Brush
                     ?? new SolidColorBrush(Color.FromRgb(0x28, 0x28, 0x28));
            EnterHint.Background = eb;
        }
    }

    private bool IsCurrentlySelected(ViewModels.MainViewModel? vm)
        => vm is not null
           && DataContext is Models.SearchResult r
           && vm.SelectedIndex >= 0
           && vm.SelectedIndex < vm.Results.Count
           && ReferenceEquals(vm.Results[vm.SelectedIndex], r);

    private void ShowSubtitleIfPresent()
    {
        var subtitle = (DataContext as Models.SearchResult)?.Subtitle;
        SubtitleText.Visibility = string.IsNullOrEmpty(subtitle)
            ? Visibility.Collapsed : Visibility.Visible;
    }

    private ViewModels.MainViewModel? FindParentViewModel()
    {
        DependencyObject? current = this;
        while (current is not null)
        {
            if ((current as FrameworkElement)?.DataContext is ViewModels.MainViewModel vm)
                return vm;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }
}
