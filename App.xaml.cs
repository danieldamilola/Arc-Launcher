using System.Windows;
using Flow.Services;
using Flow.ViewModels;

namespace Flow;

public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {

        var appDiscovery = new AppDiscoveryService();
        var fileSearch   = new FileSearchService();
        var config       = new ConfigService();
        var hotkey       = new HotkeyService();

        var vm = new MainViewModel(appDiscovery, fileSearch, config);

        // Pre-warm icon cache once apps are loaded.
        vm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.Results)) return;
            if (args.PropertyName != "IsLoading") return;
            _ = IconService.PreloadAsync(
                appDiscovery.InstalledApps.Select(a => a.Icon));
        };

        var settingsVm = new SettingsViewModel(config, vm);

        var window = new MainWindow();
        window.Initialize(vm, hotkey, settingsVm);

        // Keep app alive but window hidden — show via Alt+Space.
        // Show briefly to initialise the HWND for hotkey registration,
        // then immediately hide.
        window.Show();
        window.Hide();

        ShutdownMode = ShutdownMode.OnExplicitShutdown;
    }
}
