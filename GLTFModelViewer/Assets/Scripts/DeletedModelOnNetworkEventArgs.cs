using System.Net;
using System;

#if ENABLE_WINMD_SUPPORT
#endif // ENABLE_WINMD_SUPPORT

public class DeletedModelOnNetworkEventArgs : EventArgs
{
    public DeletedModelOnNetworkEventArgs(Guid modelIdentifier)
    {
        this.ModelIdentifier = modelIdentifier;
    }
    public Guid ModelIdentifier { get; private set; }
}
