using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Models;
using Flow.Services;

namespace Flow.ViewModels;

/// <summary>
/// Central ViewModel for the Flow launcher. Manages search state, results,
/// window visibility, extension dispatching, and AI responses.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    // ─── Injected services ───────────────────────────────────────────
    private readonly AppDiscoveryService _appDiscovery;
    private readonly FileSearchService _fileSearch;
    private readonly ConfigService _configService;

    // ─── Search debounce ─────────────────────────────────────────────
    private CancellationTokenSource? _debounceCts;
    private static readonly TimeSpan DebounceDelay = TimeSpan.FromMilliseconds(150);

    // ─── Combined search corpus ──────────────────────────────────────
    private List<SearchResult> _allApps = new();

    // ─── Observable properties (source-generated) ────────────────────

    /// <summary>Current text in the search bar.</summary>
    [ObservableProperty]
    private string _query = "";

    /// <summary>Current list of search results displayed in the UI.</summary>
    [ObservableProperty]
    private ObservableCollection<SearchResult> _results = new();

    /// <summary>Zero-based index of the highlighted result.</summary>
    [ObservableProperty]
    private int _selectedIndex = -1;

    /// <summary>True while a search operation is in progress.</summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>True when the Flow window is visible on screen.</summary>
    [ObservableProperty]
    private bool _isWindowOpen;

    /// <summary>
    /// Identifier of the active extension, or null when showing search results.
    /// Possible values: "calculator", "clipboard", "color", "timer", "ip", "ai", "settings".
    /// </summary>
    [ObservableProperty]
    private string? _activeExtension;

    /// <summary>Streamed AI response text, built up token by token.</summary>
    [ObservableProperty]
    private string _aiResponse = "";

    /// <summary>True while an AI request is in flight.</summary>
    [ObservableProperty]
    private bool _aiIsLoading;

    /// <summary>Error message from a failed AI call, or null on success.</summary>
    [ObservableProperty]
    private string? _aiError;

    /// <summary>Current user configuration loaded from disk.</summary>
    [ObservableProperty]
    private FlowConfig _config = new();

    /// <summary>Tracks how many times each app/file path has been launched.</summary>
    [ObservableProperty]
    private Dictionary<string, int> _usageMap = new();

    /// <summary>
    /// Content displayed in the detail panel. Can be a SearchResult, a string,
    /// or any object the detail panel knows how to render.
    /// </summary>
    [ObservableProperty]
    private object? _detailContent;

    // ─── Constructor ─────────────────────────────────────────────────

    public MainViewModel(AppDiscoveryService appDiscovery, FileSearchService fileSearch, ConfigService configService)
    {
        _appDiscovery = appDiscovery;
        _fileSearch = fileSearch;
        _configService = configService;

        // Kick off startup tasks.
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadConfigCommand.ExecuteAsync(null);
        await LoadAppsAsync();
    }

    private async Task LoadAppsAsync()
    {
        try
        {
            await _appDiscovery.LoadAppsAsync();
            _allApps = _appDiscovery.InstalledApps.ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load apps: {ex.Message}");
            _allApps = new List<SearchResult>();
        }
    }

    // ─── Query change handler ────────────────────────────────────────

    /// <summary>Called whenever <see cref="Query"/> changes.</summary>
    partial void OnQueryChanged(string value)
    {
        HandleQueryChanged(value);
    }

    private void HandleQueryChanged(string query)
    {
        // Cancel any in-flight debounced search.
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = null;

        if (string.IsNullOrWhiteSpace(query))
        {
            ResetSearchCommand.Execute(null);
            return;
        }

        // Check if the query triggers a built-in extension.
        var extension = DetectExtension(query);
        if (extension != null)
        {
            ActiveExtension = extension;
            Results.Clear();
            SelectedIndex = -1;
            DetailContent = null;
            AiError = null;
            AiIsLoading = false;

            if (extension == "ai")
            {
                var aiQuery = ExtractAiQuery(query);
                if (!string.IsNullOrWhiteSpace(aiQuery))
                {
                    DetailContent = $"AI: {aiQuery}";
                }
            }
            else if (extension == "settings")
            {
                DetailContent = "settings";
            }
            else
            {
                DetailContent = $"Extension: {extension} — {query}";
            }

            return;
        }

        ActiveExtension = null;
        DetailContent = null;
        AiError = null;
        AiIsLoading = false;

        // Debounced search.
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(DebounceDelay, token);
                if (token.IsCancellationRequested)
                    return;

                Application.Current?.Dispatcher.Invoke(() => IsLoading = true);

                // Fuzzy search across combined apps + files.
                var fileResults = await _fileSearch.SearchAsync(query, Config.MaxFileResults, token);
                var combined = PerformFuzzySearch(query, _allApps, fileResults);

                if (!token.IsCancellationRequested)
                {
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        Results = new ObservableCollection<SearchResult>(combined);
                        SelectedIndex = Results.Count > 0 ? 0 : -1;
                        IsLoading = false;

                        if (Results.Count > 0)
                            DetailContent = Results[0];
                    });
                }
            }
            catch (OperationCanceledException)
            {
                // Expected on debounce cancellation — swallow.
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Search error: {ex.Message}");
                Application.Current?.Dispatcher.Invoke(() => IsLoading = false);
            }
        }, token);
    }

    // ─── Extension detection ─────────────────────────────────────────

    /// <summary>
    /// Returns the extension identifier if the query triggers one; otherwise null.
    /// </summary>
    private static string? DetectExtension(string query)
    {
        var trimmed = query.TrimStart();

        // "ai " or "ai:" prefix
        if (trimmed.StartsWith("ai ", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("ai:", StringComparison.OrdinalIgnoreCase))
            return "ai";

        // "settings"
        if (trimmed.Equals("settings", StringComparison.OrdinalIgnoreCase))
            return "settings";

        // "calc " prefix
        if (trimmed.StartsWith("calc ", StringComparison.OrdinalIgnoreCase))
            return "calculator";

        // Math expression: starts with digit and contains operators
        if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^[\d\s\+\-\*\/\(\)\.\%\^]+$") &&
            System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"[\+\-\*\/\%\^]"))
            return "calculator";

        // "clip" — clipboard history
        if (trimmed.Equals("clip", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("clipboard", StringComparison.OrdinalIgnoreCase))
            return "clipboard";

        // "color #..." or "#RRGGBB"
        if (trimmed.StartsWith("color ", StringComparison.OrdinalIgnoreCase))
            return "color";
        if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^#[0-9a-fA-F]{3,8}$"))
            return "color";

        // "timer 10m" / "timer 30s"
        if (System.Text.RegularExpressions.Regex.IsMatch(trimmed,
                @"^timer\s+\d+\s*(s|sec|second|seconds|m|min|minute|minutes|h|hour|hours)\s*$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            return "timer";

        // "ip"
        if (trimmed.Equals("ip", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("ipaddress", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("my ip", StringComparison.OrdinalIgnoreCase))
            return "ip";

        return null;
    }

    private static string ExtractAiQuery(string query)
    {
        var trimmed = query.TrimStart();
        if (trimmed.StartsWith("ai ", StringComparison.OrdinalIgnoreCase))
            return trimmed[3..].Trim();
        if (trimmed.StartsWith("ai:", StringComparison.OrdinalIgnoreCase))
            return trimmed[3..].Trim();
        return trimmed;
    }

    // ─── Fuzzy search ────────────────────────────────────────────────

    private static List<SearchResult> PerformFuzzySearch(
        string query,
        List<SearchResult> apps,
        List<SearchResult> files)
    {
        var results = new List<SearchResult>();
        var lowerQuery = query.ToLowerInvariant();

        // Score apps — apps are weighted higher.
        foreach (var app in apps)
        {
            var score = ComputeFuzzyScore(app.Name, lowerQuery);
            if (score > 0)
            {
                app.Score = score * 1.5; // App weight boost
                results.Add(app);
            }
        }

        // Score files.
        foreach (var file in files)
        {
            var score = ComputeFuzzyScore(file.Name, lowerQuery);
            if (score > 0)
            {
                file.Score = score;
                results.Add(file);
            }
        }

        // Sort descending by score.
        results.Sort((a, b) => b.Score.CompareTo(a.Score));

        return results;
    }

    /// <summary>
    /// Simple fuzzy scoring: rewards contiguous matches and early matches.
    /// Returns 0.0 for no match, up to 1.0 for a perfect match.
    /// </summary>
    private static double ComputeFuzzyScore(string text, string lowerQuery)
    {
        var lowerText = text.ToLowerInvariant();
        if (lowerText == lowerQuery)
            return 1.0;

        // Contiguous substring match.
        int index = lowerText.IndexOf(lowerQuery, StringComparison.Ordinal);
        if (index >= 0)
        {
            double positionBonus = 1.0 - (double)index / lowerText.Length;
            double lengthRatio = (double)lowerQuery.Length / lowerText.Length;
            return 0.5 + (0.5 * positionBonus * (0.5 + 0.5 * lengthRatio));
        }

        // Abbreviation / initial-character matching.
        if (MatchesInitials(lowerText, lowerQuery))
            return 0.4;

        // Partial subsequence match.
        if (MatchesSubsequence(lowerText, lowerQuery))
            return 0.25;

        return 0.0;
    }

    private static bool MatchesInitials(string text, string query)
    {
        int qi = 0;
        bool started = false;
        foreach (var ch in text)
        {
            if (!started || ch == ' ' || ch == '.' || ch == '-' || ch == '_')
            {
                started = true;
                if (qi < query.Length && char.ToLowerInvariant(ch) == query[qi])
                {
                    qi++;
                    if (qi == query.Length)
                        return true;
                }
            }
        }
        return false;
    }

    private static bool MatchesSubsequence(string text, string query)
    {
        int qi = 0;
        foreach (var ch in text)
        {
            if (qi < query.Length && ch == query[qi])
            {
                qi++;
                if (qi == query.Length)
                    return true;
            }
        }
        return false;
    }

    // ─── Commands ────────────────────────────────────────────────────

    /// <summary>Toggles the Flow window between visible and hidden.</summary>
    [RelayCommand]
    private void ToggleWindow()
    {
        IsWindowOpen = !IsWindowOpen;
    }

    /// <summary>Opens the currently selected search result.</summary>
    [RelayCommand]
    private void OpenSelected()
    {
        if (SelectedIndex < 0 || SelectedIndex >= Results.Count)
            return;

        var result = Results[SelectedIndex];
        ExecuteOpen(result);
    }

    private void ExecuteOpen(SearchResult result)
    {
        switch (result.Type)
        {
            case "app":
                if (!string.IsNullOrEmpty(result.ExecutablePath))
                {
                    AppDiscoveryService.LaunchApp(result.ExecutablePath);
                    IncrementUsageCommand.Execute(result.ExecutablePath);
                }
                break;
            case "file":
                if (!string.IsNullOrEmpty(result.FullPath))
                {
                    FileSearchService.OpenFile(result.FullPath);
                    IncrementUsageCommand.Execute(result.FullPath);
                }
                break;
        }

        // Dismiss the window after launching.
        IsWindowOpen = false;
    }

    /// <summary>Moves the selection highlight up one row (with wrap-around).</summary>
    [RelayCommand]
    private void MoveSelectionUp()
    {
        if (Results.Count == 0)
            return;

        if (SelectedIndex <= 0)
            SelectedIndex = Results.Count - 1;
        else
            SelectedIndex--;
    }

    /// <summary>Moves the selection highlight down one row (with wrap-around).</summary>
    [RelayCommand]
    private void MoveSelectionDown()
    {
        if (Results.Count == 0)
            return;

        if (SelectedIndex >= Results.Count - 1)
            SelectedIndex = 0;
        else
            SelectedIndex++;
    }

    /// <summary>Increments the usage count for the given path.</summary>
    [RelayCommand]
    private void IncrementUsage(string path)
    {
        if (string.IsNullOrEmpty(path))
            return;

        if (UsageMap.TryGetValue(path, out int count))
            UsageMap[path] = count + 1;
        else
            UsageMap[path] = 1;
    }

    /// <summary>Programmatically sets the search query (e.g. from the settings button).</summary>
    [RelayCommand]
    private void SetQuery(string query)
    {
        Query = query ?? "";
    }

    /// <summary>Resets search state to its initial idle condition.</summary>
    [RelayCommand]
    private void ResetSearch()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = null;

        Query = "";
        Results.Clear();
        SelectedIndex = -1;
        IsLoading = false;
        ActiveExtension = null;
        AiResponse = "";
        AiIsLoading = false;
        AiError = null;
        DetailContent = null;
    }

    /// <summary>Persists the current config to disk.</summary>
    [RelayCommand]
    private async Task SaveConfig()
    {
        await _configService.SaveAsync(Config);
    }

    /// <summary>Loads configuration from disk (called on startup).</summary>
    [RelayCommand]
    private async Task LoadConfig()
    {
        Config = await _configService.LoadAsync();
    }
}
