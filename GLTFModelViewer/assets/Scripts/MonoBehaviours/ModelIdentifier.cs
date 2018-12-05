using System;
using UnityEngine;

public class ModelIdentifier : MonoBehaviour
{
    public Guid? Identifier { get; private set; }

    public void CreateNew()
    {
        this.isSharedFromNetwork = false;
        this.Identifier = Guid.NewGuid();
    }
    public void AssignExistingFromNetworkedShare(Guid identifier)
    {
        this.isSharedFromNetwork = true;
        this.Identifier = identifier;
    }
    public void Clear()
    {
        this.Identifier = null;
        this.isSharedFromNetwork = false;
    }
    public bool IsSharedFromNetwork => this.isSharedFromNetwork;
    public bool HasModel => this.Identifier.HasValue;

    bool isSharedFromNetwork;
}