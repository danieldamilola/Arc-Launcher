using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Flow.Services;

/// <summary>
/// Provides system network information such as local and public IP addresses.
/// </summary>
public static class SystemService
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(5)
    };

    /// <summary>
    /// Retrieves both the local (LAN) and public (WAN) IP addresses.
    /// </summary>
    /// <returns>
    /// A tuple containing the local IPv4 address and the public IP address.
    /// Each may be null if the address could not be determined.
    /// </returns>
    public static async Task<(string? LocalIp, string? PublicIp)> GetIpAddressesAsync()
    {
        var localIp = GetLocalIpAddress();
        var publicIp = await GetPublicIpAddressAsync();
        return (localIp, publicIp);
    }

    /// <summary>
    /// Finds the first active, non-loopback IPv4 address on this machine by
    /// enumerating all network interfaces.
    /// </summary>
    /// <returns>The local IPv4 address string, or null if none found.</returns>
    public static string? GetLocalIpAddress()
    {
        try
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
                .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                .FirstOrDefault(addr =>
                    addr.Address.AddressFamily == AddressFamily.InterNetwork &&
                    !System.Net.IPAddress.IsLoopback(addr.Address))
                ?.Address
                ?.ToString();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Fetches the public IP address from ipify.org.
    /// </summary>
    /// <returns>The public IP address string, or null if the request failed.</returns>
    public static async Task<string?> GetPublicIpAddressAsync()
    {
        try
        {
            var response = await HttpClient.GetStringAsync("https://api.ipify.org");
            return response.Trim();
        }
        catch
        {
            return null;
        }
    }
}
