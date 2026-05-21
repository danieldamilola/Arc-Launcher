using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Flow.Services;
using Flow.ViewModels;
using Flow.Views;

namespace Flow;

public partial class MainWindow : Window
{
    private MainViewModel? _viewModel;
    private HotkeyService? _hotkeyService;

    public MainWindow()
    {
        InitializeComponent();
    }

    public void Initialize(MainViewModel viewModel, HotkeyService hotkeyService, SettingsViewModel settingsVm)
    {
        _viewModel = viewModel;
        _hotkeyService = hotkeyService;
        DataContext = viewModel;

        SettingsPanelView.DataContext = settingsVm;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MainViewModel.ActiveExtension)) return;

        var isSettings = _viewModel?.ActiveExtension == "settings";
        SettingsPanelView.Visibility = isSettings ? Visibility.Visible : Visibility.Collapsed;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // Register Alt+Space global hotkey now that we have a window handle.
        _hotkeyService?.RegisterHotkey(ModifierKeys.Alt, Key.Space, ToggleWindow);
    }

    public void ShowWindow()
    {
        if (!IsVisible)
        {
            Opacity = 0;
            Show();
            Activate();

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            BeginAnimation(OpacityProperty, fadeIn);
        }
        else
        {
            Activate();
        }
    }

    public void HideWindow()
    {
        if (!IsVisible) return;

        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(100))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        fadeOut.Completed += (_, _) =>
        {
            Hide();
            _viewModel?.ResetSearchCommand.Execute(null);
        };
        BeginAnimation(OpacityProperty, fadeOut);
    }

    private void ToggleWindow()
    {
        if (IsVisible)
            HideWindow();
        else
            ShowWindow();
    }

    private void Window_Deactivated(object? sender, EventArgs e) => HideWindow();

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                HideWindow();
                e.Handled = true;
                break;
            case Key.Down:
                _viewModel?.MoveSelectionDownCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.Up:
                _viewModel?.MoveSelectionUpCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.Enter:
                _viewModel?.OpenSelectedCommand.Execute(null);
                HideWindow();
                e.Handled = true;
                break;
        }
    }

    private void SearchBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    protected override void OnClosed(EventArgs e)
    {
        _hotkeyService?.Dispose();
        base.OnClosed(e);
    }
}
