using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Linq;


using MulticastMessaging;

internal static class IPAddressProvider
{
    public static bool HasIpAddress => IPAddress != null;

    public static IPAddress IPAddress
    {
        get
        {
            if (!initialised)
            {
                initialised = true;

                // NB: WIFI only seems fine here but won't help on the emulator hence
                // passing false.
                var addresses = NetworkUtility.GetConnectedIpAddresses(false);

                if (addresses.Count > 0)
                {
                    ipAddress = IPAddress.Parse(addresses.First());
                }
            }
            return (ipAddress);
        }            
    }
    static bool initialised;
    static IPAddress ipAddress;
}