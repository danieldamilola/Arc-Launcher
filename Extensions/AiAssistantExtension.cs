using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Flow.Services;

namespace Flow.Extensions;

/// <summary>
/// Sends a question to the Groq AI API and streams the response word-by-word.
/// Shows loading states, error states, and the streaming response.
/// </summary>
public partial class AiAssistantExtension : UserControl, IExtension
{
    public string Id => "ai";
    public string DisplayName => "AI Assistant";

    /// <summary>
    /// Matches "ai " followed by any question text.
    /// </summary>
    public object Trigger => new Regex(
        @"^ai\s+.+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private CancellationTokenSource? _cts;
    private int _tokenCount;

    public AiAssistantExtension()
    {
        InitializeComponent();
    }

    public UserControl CreateControl(string query)
    {
        var control = new AiAssistantExtension();
        _ = control.ProcessQueryAsync(query);
        return control;
    }

    private async Task ProcessQueryAsync(string query)
    {
        // Strip the "ai " prefix to get the actual question
        var question = Regex.Replace(query.Trim(), @"^ai\s+", "", RegexOptions.IgnoreCase);

        if (string.IsNullOrWhiteSpace(question))
        {
            ShowNoKey("Enter a question after 'ai'.");
            return;
        }

        QueryLabel.Text = question;

        // Check for API key
        var apiKey = Environment.GetEnvironmentVariable("FLOW_GROQ_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            ShowNoKey("Add your Groq API key in settings.");
            return;
        }

        // Show loading state
        ShowLoading();
        _cts = new CancellationTokenSource();
        _tokenCount = 0;

        var startedAt = DateTime.Now;

        try
        {
            var hasContent = false;

            await AiService.QueryAsync(question, token =>
            {
                // Check for error tokens from AiService
                if (token.StartsWith("[Error:", StringComparison.Ordinal))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowError(token.Trim('[', ']'));
                    });
                    return;
                }

                hasContent = true;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Switch from loading to response on first token
                    if (_tokenCount == 0)
                    {
                        ShowResponse();
                    }

                    ResponseText.Text += token;
                    _tokenCount++;

                    // Auto-scroll to bottom
                    ResponseScroller.ScrollToEnd();
                });
            }, _cts.Token);

            var elapsed = (DateTime.Now - startedAt).TotalSeconds;

            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusBar.Text = hasContent
                    ? $"Response: {_tokenCount} tokens · {elapsed:F1}s"
                    : "No response received.";
            });
        }
        catch (TaskCanceledException)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusBar.Text = "Cancelled";
            });
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ShowError(ex.Message);
            });
        }
    }

    private void ShowLoading()
    {
        LoadingBorder.Visibility = Visibility.Visible;
        NoKeyBorder.Visibility = Visibility.Collapsed;
        ResponseScroller.Visibility = Visibility.Collapsed;
        ResponseText.Text = "";
        StatusBar.Text = "";
        _ = AnimateDotsAsync();
    }

    private void ShowResponse()
    {
        LoadingBorder.Visibility = Visibility.Collapsed;
        NoKeyBorder.Visibility = Visibility.Collapsed;
        ResponseScroller.Visibility = Visibility.Visible;
    }

    private void ShowNoKey(string hint)
    {
        LoadingBorder.Visibility = Visibility.Collapsed;
        NoKeyBorder.Visibility = Visibility.Visible;
        ResponseScroller.Visibility = Visibility.Collapsed;
        StatusBar.Text = "";
    }

    private void ShowError(string message)
    {
        LoadingBorder.Visibility = Visibility.Collapsed;
        NoKeyBorder.Visibility = Visibility.Collapsed;
        ResponseScroller.Visibility = Visibility.Visible;
        ResponseText.Text = message;
        ResponseText.Foreground = new System.Windows.Media.SolidColorBrush(
            (System.Windows.Media.Color)FindResource("Destructive"));
        StatusBar.Text = "Error";
    }

    private async Task AnimateDotsAsync()
    {
        var dots = new[] { ".", "..", "...", "...." };
        var index = 0;

        while (LoadingBorder.Visibility == Visibility.Visible)
        {
            AnimatedDots.Text = dots[index];
            index = (index + 1) % dots.Length;
            await Task.Delay(350);

            if (LoadingBorder.Visibility != Visibility.Visible)
                break;
        }
    }
}
