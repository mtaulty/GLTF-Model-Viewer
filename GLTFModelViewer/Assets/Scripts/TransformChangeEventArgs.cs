using UnityEngine;
using System;

#if ENABLE_WINMD_SUPPORT
#endif // ENABLE_WINMD_SUPPORT
public class TransformChangeEventArgs : EventArgs
{
    public TransformChangeEventArgs(Guid modelIdentifier,
        Vector3 scale, Quaternion rotation, Vector3 translation)
    {
        this.ModelIdentifier = modelIdentifier;
        this.Scale = scale;
        this.Rotation = rotation;
        this.Translation = translation;
    }
    public Guid ModelIdentifier { get; private set; }
    public Vector3 Scale { get; private set; }
    public Quaternion Rotation { get; private set; }
    public Vector3 Translation { get; private set; }
}
