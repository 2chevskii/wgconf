using System.Net.Sockets;

namespace WgConf.Extensions;

internal static class AddressFamilyExtensions
{
    extension(AddressFamily addressFamily)
    {
        public string FriendlyName =>
            addressFamily switch
            {
                AddressFamily.InterNetwork => "IPv4",
                AddressFamily.InterNetworkV6 => "IPv6",
                var _ => throw new ArgumentOutOfRangeException(
                    nameof(addressFamily),
                    $"Address family {addressFamily:G} is not supported for a friendly name classification"
                ),
            };

        public int MaxPrefixLength =>
            addressFamily switch
            {
                AddressFamily.InterNetwork => 32,
                AddressFamily.InterNetworkV6 => 128,
                var _ => throw new ArgumentOutOfRangeException(
                    nameof(addressFamily),
                    $"Address family {addressFamily:G} is not supported for a max prefix length classification"
                ),
            };
    }
}
