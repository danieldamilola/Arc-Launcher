using System.Windows;
using System.Windows.Controls;

namespace Flow.Views;

/// <summary>
/// Code-behind for the SettingsPanel user control.
/// All logic is delegated to the SettingsViewModel via bindings.
/// </summary>
public partial class SettingsPanel : UserControl
{
    public SettingsPanel()
    {
        InitializeComponent();
    }
}
