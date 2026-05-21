using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Flow.Extensions;

/// <summary>
/// Countdown timer triggered by "timer Xm", "timer Xs", or "timer Xh" queries.
/// Uses <see cref="DispatcherTimer"/> for the countdown.
/// </summary>
public partial class TimerExtension : UserControl, IExtension
{
    public string Id => "timer";
    public string DisplayName => "Timer";

    /// <summary>
    /// Matches "timer" followed by a number and a unit (s, m, h).
    /// </summary>
    public object Trigger => new Regex(
        @"^timer\s+\d+\s*[smh]$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private DispatcherTimer? _timer;
    private TimeSpan _remaining;
    private TimeSpan _total;

    public TimerExtension()
    {
        InitializeComponent();
    }

    public UserControl CreateControl(string query)
    {
        var control = new TimerExtension();

        if (TryParseDuration(query, out var duration))
        {
            control.StartCountdown(duration);
        }
        else
        {
            control.ShowError("Invalid duration. Use: timer 10m, timer 30s, timer 1h");
        }

        return control;
    }

    /// <summary>
    /// Parses the duration from a "timer X[unit]" query.
    /// </summary>
    private static bool TryParseDuration(string query, out TimeSpan duration)
    {
        duration = TimeSpan.Zero;

        var match = Regex.Match(query.Trim(), @"^timer\s+(\d+)\s*([smh])$", RegexOptions.IgnoreCase);
        if (!match.Success)
            return false;

        if (!int.TryParse(match.Groups[1].Value, out var value) || value <= 0)
            return false;

        var unit = match.Groups[2].Value.ToLowerInvariant();

        duration = unit switch
        {
            "s" => TimeSpan.FromSeconds(value),
            "m" => TimeSpan.FromMinutes(value),
            "h" => TimeSpan.FromHours(value),
            _ => TimeSpan.Zero
        };

        return duration > TimeSpan.Zero;
    }

    private void StartCountdown(TimeSpan duration)
    {
        _remaining = duration;
        _total = duration;

        UpdateDisplay();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _timer.Tick += OnTick;
        _timer.Start();

        StatusLabel.Text = "Running...";
    }

    private void OnTick(object? sender, EventArgs e)
    {
        _remaining = _remaining.Subtract(TimeSpan.FromMilliseconds(100));

        if (_remaining <= TimeSpan.Zero)
        {
            _remaining = TimeSpan.Zero;
            _timer?.Stop();
            OnComplete();
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (_remaining.TotalHours >= 1)
        {
            CountdownText.Text = _remaining.ToString(@"hh\:mm\:ss");
        }
        else
        {
            CountdownText.Text = _remaining.ToString(@"mm\:ss");
        }

        var progress = _total > TimeSpan.Zero
            ? (_remaining.TotalMilliseconds / _total.TotalMilliseconds) * 100.0
            : 0.0;

        TimerProgress.Value = Math.Clamp(progress, 0, 100);
    }

    private void OnComplete()
    {
        CountdownText.Text = "00:00";
        TimerProgress.Value = 0;
        StatusLabel.Text = "Done!";
        StatusLabel.Foreground = new System.Windows.Media.SolidColorBrush(
            (System.Windows.Media.Color)FindResource("Success"));
    }

    private void ShowError(string message)
    {
        CountdownText.Text = "--:--";
        StatusLabel.Text = "";
        ErrorLabel.Text = message;
        ErrorLabel.Visibility = System.Windows.Visibility.Visible;
    }
}
