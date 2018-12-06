using System.Net;
using System;

#if ENABLE_WINMD_SUPPORT
#endif // ENABLE_WINMD_SUPPORT

public class NewModelOnNetworkEventArgs : EventArgs
{
    public NewModelOnNetworkEventArgs(Guid modelIdentifier,
        IPAddress ipAddress)
    {
        this.ModelIdentifier = modelIdentifier;
        this.IPAddress = ipAddress;
    }
    public IPAddress IPAddress { get; private set; }
    public Guid ModelIdentifier { get; private set; }
}
