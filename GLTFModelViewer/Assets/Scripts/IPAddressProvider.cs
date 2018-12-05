using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using System.Linq;

#if ENABLE_WINMD_SUPPORT
using MulticastMessaging;
#endif // ENABLE_WINMD_SUPPORT

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

#if ENABLE_WINMD_SUPPORT
                // NB: WIFI only seems fine here but won't help on the emulator hence
                // passing false.
                var addresses = NetworkUtility.GetConnectedIpAddresses(false);

                if (addresses.Count > 0)
                {
                    ipAddress = IPAddress.Parse(addresses.First());
                }
#endif // ENABLE_WINMD_SUPPORT
            }
            return (ipAddress);
        }            
    }
    static bool initialised;
    static IPAddress ipAddress;
}