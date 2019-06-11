using System;

public class DeletedModelOnNetworkEventArgs : EventArgs
{
    public DeletedModelOnNetworkEventArgs(Guid modelIdentifier)
    {
        this.ModelIdentifier = modelIdentifier;
    }
    public Guid ModelIdentifier { get; private set; }
}
