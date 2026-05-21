using System.Windows;
using System.Windows.Controls;
using Flow.Models;

namespace Flow.Views;

/// <summary>
/// Code-behind for the DetailPanel user control.
/// Reacts to changes in the parent ViewModel's DetailContent to show
/// contextual information about the selected result, AI response, or extension.
/// </summary>
public partial class DetailPanel : UserControl
{
    public DetailPanel()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is ViewModels.MainViewModel vm)
        {
            vm.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(vm.DetailContent) ||
                    args.PropertyName == nameof(vm.ActiveExtension) ||
                    args.PropertyName == nameof(vm.AiResponse) ||
                    args.PropertyName == nameof(vm.AiIsLoading) ||
                    args.PropertyName == nameof(vm.AiError) ||
                    args.PropertyName == nameof(vm.Config))
                {
                    RefreshContent(vm);
                }
            };

            RefreshContent(vm);
        }
    }

    /// <summary>Updates the visible content based on the current ViewModel state.</summary>
    private void RefreshContent(ViewModels.MainViewModel vm)
    {
        // Visibility of the entire panel.
        Root.Visibility = vm.Config.ShowDetailPanel ? Visibility.Visible : Visibility.Collapsed;

        // Determine what to show.
        if (vm.AiIsLoading || !string.IsNullOrEmpty(vm.AiResponse) || !string.IsNullOrEmpty(vm.AiError))
        {
            ShowAiContent(vm);
        }
        else if (!string.IsNullOrEmpty(vm.ActiveExtension))
        {
            ShowExtensionContent(vm);
        }
        else if (vm.DetailContent is SearchResult result)
        {
            ShowResultDetail(result);
        }
        else if (vm.DetailContent is string str && str == "settings")
        {
            ShowExtensionHeader("Settings", "Type to search settings or use the settings panel.");
        }
        else
        {
            ShowEmptyState();
        }
    }

    private void ShowResultDetail(SearchResult result)
    {
        HideAll();

        AppDetailSection.Visibility = Visibility.Visible;
        AppName.Text = result.Name;
        AppPath.Text = result.Type == "app"
            ? (result.ExecutablePath ?? result.ShortcutPath ?? "")
            : (result.FullPath ?? "");

        if (result.Type == "file")
        {
            var sizeText = FormatFileSize(result.Size);
            AppMeta.Text = $"{sizeText}  ·  {result.LastModified ?? "Unknown"}";
        }
        else
        {
            AppMeta.Text = "";
        }
    }

    private void ShowAiContent(ViewModels.MainViewModel vm)
    {
        HideAll();

        AiDetailSection.Visibility = Visibility.Visible;

        if (!string.IsNullOrEmpty(vm.AiError))
        {
            AiErrorText.Text = vm.AiError;
            AiErrorText.Visibility = Visibility.Visible;
            AiContent.Visibility = Visibility.Collapsed;
            AiLoadingIndicator.Visibility = Visibility.Collapsed;
        }
        else if (vm.AiIsLoading && string.IsNullOrEmpty(vm.AiResponse))
        {
            AiLoadingIndicator.Visibility = Visibility.Visible;
            AiErrorText.Visibility = Visibility.Collapsed;
            AiContent.Visibility = Visibility.Collapsed;
        }
        else
        {
            AiContent.Text = vm.AiResponse;
            AiContent.Visibility = Visibility.Visible;
            AiLoadingIndicator.Visibility = vm.AiIsLoading ? Visibility.Visible : Visibility.Collapsed;
            AiErrorText.Visibility = Visibility.Collapsed;
        }
    }

    private void ShowExtensionContent(ViewModels.MainViewModel vm)
    {
        HideAll();

        ExtensionDetailSection.Visibility = Visibility.Visible;
        ExtensionHeader.Text = vm.ActiveExtension ?? "Extension";

        if (vm.DetailContent is string content)
        {
            ExtensionContent.Text = content;
        }
        else
        {
            ExtensionContent.Text = "";
        }
    }

    private void ShowExtensionHeader(string header, string content)
    {
        HideAll();
        ExtensionDetailSection.Visibility = Visibility.Visible;
        ExtensionHeader.Text = header;
        ExtensionContent.Text = content;
    }

    private void ShowEmptyState()
    {
        HideAll();
        EmptyHint.Visibility = Visibility.Visible;
    }

    private void HideAll()
    {
        AppDetailSection.Visibility = Visibility.Collapsed;
        AiDetailSection.Visibility = Visibility.Collapsed;
        ExtensionDetailSection.Visibility = Visibility.Collapsed;
        EmptyHint.Visibility = Visibility.Collapsed;
        AiErrorText.Visibility = Visibility.Collapsed;
        AiLoadingIndicator.Visibility = Visibility.Collapsed;
        AiContent.Visibility = Visibility.Collapsed;
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F1} MB";
        return $"{bytes / (1024.0 * 1024.0 * 1024.0):F1} GB";
    }
}
