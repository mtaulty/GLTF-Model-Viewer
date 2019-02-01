using System;
using UnityEngine;

public class ModelIdentifier : MonoBehaviour
{
    public Guid Identifier { get; private set; }

    public ModelIdentifier()
    {
        this.Identifier = Guid.NewGuid();
    }
    public void AssignExistingFromNetworkedShare(Guid identifier)
    {
        this.isSharedFromNetwork = true;
        this.Identifier = identifier;
    }
    public bool IsSharedFromNetwork => this.isSharedFromNetwork;

    bool isSharedFromNetwork;
}