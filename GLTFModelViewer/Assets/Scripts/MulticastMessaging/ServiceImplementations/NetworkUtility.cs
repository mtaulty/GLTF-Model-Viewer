//#if ENABLE_WINMD_SUPPORT
namespace MulticastMessaging
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
#if ENABLE_WINMD_SUPPORT
    using Windows.Networking.Connectivity;
    using Windows.Networking;
#endif // ENABLE_WINMD_SUPPORT

    public enum AddressFamilyType
    {
        IP4,
        IP6
    }
    public static class NetworkUtility 
    {
        // Note - using List<string> here because if I use IPAddress then I get into
        // a situation where Unity can't build because IPAddress seems to be in
        // System.dll on .NET 3.51 and in System.Net.Primitives in UWP.
        // I suspect that I'll see some other, similar problems.
        public static List<string> GetConnectedIpAddresses(
            bool wifiOnly,
            AddressFamilyType addressFamily = AddressFamilyType.IP4)
        {
            List<string> addresses = null;

#if ENABLE_WINMD_SUPPORT
            var profiles = NetworkInformation.GetConnectionProfiles();

            var connectedAdapterIds = NetworkInformation.GetConnectionProfiles()
                .Where(
                    p => (p.GetNetworkConnectivityLevel() != NetworkConnectivityLevel.None) &&
                         IsNetworkTypeCandidate(p.NetworkAdapter.IanaInterfaceType, wifiOnly)
                )
                .Select(
                    p => p.NetworkAdapter.NetworkAdapterId
                );

            addresses = NetworkInformation.GetHostNames()
                .Where(
                        name =>
                            (name.IPInformation != null) &&
                            (name.IPInformation.NetworkAdapter != null) &&
                            connectedAdapterIds.Contains(name.IPInformation.NetworkAdapter.NetworkAdapterId) &&
                            (name.Type == AddressTypeToHostNameType(addressFamily))
                )
                .Select(
                    name => name.CanonicalName
                )
                .ToList();

#endif // ENABLE_WINMD_SUPPORT

            return (addresses);
        }
#if ENABLE_WINMD_SUPPORT
        static HostNameType AddressTypeToHostNameType(AddressFamilyType type)
        {
            return (type == AddressFamilyType.IP4 ? HostNameType.Ipv4 : HostNameType.Ipv6);
        }
#endif // ENABLE_WINMD_SUPPORT

        static bool IsNetworkCandidate(NetworkInterface iface,
            bool wifiOnly, bool excludeVirtualNames)
        {
            bool candidate =
                (iface.OperationalStatus == OperationalStatus.Up) &&
                iface.SupportsMulticast &&
                IsNetworkTypeCandidate(iface.NetworkInterfaceType, wifiOnly) &&
                (iface.GetIPProperties().UnicastAddresses.Count > 0) &&
                (excludeVirtualNames && !iface.Description.ToLower().Contains(VIRTUAL_NETWORK)); 

            return (candidate);
        }
        static bool IsNetworkTypeCandidate(uint ianaType, bool wifiOnly)
        {
            bool candidate =
                (ianaType == IANA_WIFI) ||
                (!wifiOnly && (ianaType == IANA_ETHERNET));

            return (candidate);
        }
        static bool IsNetworkTypeCandidate(NetworkInterfaceType type,
            bool wifiOnly)
        {
            var candidate = false;

            switch (type)
            {
                case NetworkInterfaceType.Ethernet:
                case NetworkInterfaceType.GigabitEthernet:
                    candidate = !wifiOnly;
                    break;
                case NetworkInterfaceType.Wireless80211:
                    candidate = true;
                    break;
                default:
                    break;
            }
            return (candidate);

        }
        // TODO: these are probably wrong and/or not inclusive enough.
        static readonly int IANA_ETHERNET = 6;
        static readonly int IANA_WIFI = 71;
        static readonly string VIRTUAL_NETWORK = "virtual";
    }
}
// #endif // ENABLE_WINMD_SUPPORT