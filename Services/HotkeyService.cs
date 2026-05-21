using System.Runtime.InteropServices;
using System.Windows.Input;

namespace Flow.Services;

/// <summary>
/// Registers and handles a global hotkey via the Windows API.
/// </summary>
public class HotkeyService : IDisposable
{
    private const int WM_HOTKEY = 0x0312;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private IntPtr _hwnd;
    private int _hotkeyId;
    private Action? _callback;
    private bool _disposed;

    /// <summary>
    /// Registers a global hotkey that invokes the callback when pressed.
    /// </summary>
    public void RegisterHotkey(ModifierKeys modifier, Key key, Action onPressed)
    {
        _callback = onPressed;

        // Get the handle from the main window once the application is running
        var mainWindow = Application.Current?.MainWindow;
        if (mainWindow == null)
            return;

        _hwnd = new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle;
        _hotkeyId = GetHashCode();

        uint mod = (uint)ModifierKeysToWin32(modifier);
        uint vk = (uint)KeyInterop.VirtualKeyFromKey(key);

        RegisterHotKey(_hwnd, _hotkeyId, mod, vk);

        // Hook into the main window's message loop
        var source = System.Windows.Interop.HwndSource.FromHwnd(_hwnd);
        if (source != null)
        {
            source.AddHook(HwndHook);
        }
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == _hotkeyId)
        {
            _callback?.Invoke();
            handled = true;
        }
        return IntPtr.Zero;
    }

    private static int ModifierKeysToWin32(ModifierKeys modifier)
    {
        int mod = 0;
        if ((modifier & ModifierKeys.Alt) == ModifierKeys.Alt)
            mod |= 0x0001; // MOD_ALT
        if ((modifier & ModifierKeys.Control) == ModifierKeys.Control)
            mod |= 0x0002; // MOD_CONTROL
        if ((modifier & ModifierKeys.Shift) == ModifierKeys.Shift)
            mod |= 0x0004; // MOD_SHIFT
        if ((modifier & ModifierKeys.Windows) == ModifierKeys.Windows)
            mod |= 0x0008; // MOD_WIN
        return mod;
    }

    public void Dispose()
    {
        if (!_disposed && _hwnd != IntPtr.Zero)
        {
            UnregisterHotKey(_hwnd, _hotkeyId);
            _disposed = true;
        }
    }
}
