using System.Windows.Controls;
using Flow.Services;

namespace Flow.Extensions;

/// <summary>
/// Displays both local (LAN) and public (WAN) IP addresses.
/// Fetches IPs asynchronously on load and shows loading states.
/// </summary>
public partial class IpAddressExtension : UserControl, IExtension
{
    public string Id => "ip";
    public string DisplayName => "IP Address";
    public object Trigger => "ip";

    public IpAddressExtension()
    {
        InitializeComponent();
    }

    public UserControl CreateControl(string query)
    {
        var control = new IpAddressExtension();
        _ = control.LoadAddressesAsync();
        return control;
    }

    private async Task LoadAddressesAsync()
    {
        // Set local IP immediately (synchronous)
        var localIp = SystemService.GetLocalIpAddress();
        LocalIpText.Text = localIp ?? "Not connected";

        // Animate loading dots for public IP
        _ = AnimateLoadingDotsAsync();

        try
        {
            var (_, publicIp) = await SystemService.GetIpAddressesAsync();
            PublicIpText.Text = publicIp ?? "Unavailable";
        }
        catch
        {
            PublicIpText.Text = "Unavailable";
        }

        LoadingDots.Text = "";
    }

    private async Task AnimateLoadingDotsAsync()
    {
        var dots = new[] { "", ".", "..", "..." };
        var index = 0;

        while (PublicIpText.Text == "Fetching...")
        {
            LoadingDots.Text = dots[index];
            index = (index + 1) % dots.Length;
            await Task.Delay(400);

            // Stop if the public IP has been resolved
            if (PublicIpText.Text != "Fetching...")
                break;
        }

        LoadingDots.Text = "";
    }
}
