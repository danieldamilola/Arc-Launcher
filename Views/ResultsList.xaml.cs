using System.Windows;
using System.Windows.Controls;
using Flow.ViewModels;

namespace Flow.Views;

public partial class ResultsList : UserControl
{
    private MainViewModel? _vm;
    private int _lastSelectedIndex = -1;

    public ResultsList()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_vm is not null)
            _vm.PropertyChanged -= OnVmPropertyChanged;

        _vm = e.NewValue as MainViewModel;

        if (_vm is not null)
            _vm.PropertyChanged += OnVmPropertyChanged;
    }

    private void OnVmPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.SelectedIndex))
            Dispatcher.InvokeAsync(UpdateSelection);

        if (e.PropertyName is nameof(MainViewModel.Results) or nameof(MainViewModel.Query))
            Dispatcher.InvokeAsync(UpdateEmptyState);
    }

    private void UpdateEmptyState()
    {
        if (_vm is null) return;

        bool hasResults  = _vm.Results.Count > 0;
        bool hasQuery    = !string.IsNullOrWhiteSpace(_vm.Query);

        ResultsScroll.Visibility  = hasResults ? Visibility.Visible   : Visibility.Collapsed;
        EmptyState.Visibility     = (!hasResults && !hasQuery) ? Visibility.Visible : Visibility.Collapsed;
        NoResultsState.Visibility = (!hasResults && hasQuery)  ? Visibility.Visible : Visibility.Collapsed;

        if (!hasResults && hasQuery)
            NoResultsText.Text = $"No results for \"{_vm.Query}\"";
    }

    private void UpdateSelection()
    {
        if (_vm is null) return;

        var panel = GetItemsPanel();
        if (panel is null) return;

        int newIndex = _vm.SelectedIndex;

        // Clear previous
        if (_lastSelectedIndex >= 0 && _lastSelectedIndex < panel.Children.Count)
        {
            if (panel.Children[_lastSelectedIndex] is ResultItem old)
                old.SetSelected(false);
        }

        // Apply new
        if (newIndex >= 0 && newIndex < panel.Children.Count)
        {
            if (panel.Children[newIndex] is ResultItem item)
            {
                item.SetSelected(true);
                item.BringIntoView();
            }
        }

        _lastSelectedIndex = newIndex;
    }

    private System.Windows.Controls.Panel? GetItemsPanel()
    {
        if (ResultsItems.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            return null;

        // Try named template part first
        if (ResultsItems.Template?.FindName("ItemsHost", ResultsItems) is System.Windows.Controls.Panel namedPanel)
            return namedPanel;

        // Fall back: walk up from first container
        if (ResultsItems.ItemContainerGenerator.ContainerFromIndex(0) is FrameworkElement fe)
            return System.Windows.Media.VisualTreeHelper.GetParent(fe) as System.Windows.Controls.Panel;

        return null;
    }
}
