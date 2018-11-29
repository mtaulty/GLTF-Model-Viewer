using System;
using UnityEngine;

public class ModelIdentifier : MonoBehaviour
{
    public Guid? Identifier { get; set; }

    public void CreateNew()
    {
        this.Identifier = Guid.NewGuid();
    }
    public void Clear()
    {
        this.Identifier = null;
    }
    public bool HasModel => this.Identifier.HasValue;

}