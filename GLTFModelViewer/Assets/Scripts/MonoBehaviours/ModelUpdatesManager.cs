using HoloToolkit.Unity.InputModule.Utilities.Interactions;
using HoloToolkit.Unity.UX;
using System;
using UnityEngine;

public class ModelUpdatesManager : MonoBehaviour
{
    [SerializeField]
    BoundingBox boundingBoxPrefab;

    ModelIdentifier ModelIdentifier => this.gameObject.GetComponent<ModelIdentifier>();

    internal void Destroy()
    {
        // Get rid of the parent.
        Destroy(this.gameObject.transform.parent);

        // Get rid of the object itself.
        Destroy(this.gameObject);
    }
    void Start()
    {
        NetworkMessagingProvider.TransformChange += this.OnTransformChangeMessage;
        NetworkMessagingProvider.DeletedModelOnNetwork += this.OnDeletedModelMessage;
    }
    void OnDestroy()
    {
        NetworkMessagingProvider.TransformChange -= this.OnTransformChangeMessage;
        NetworkMessagingProvider.DeletedModelOnNetwork -= this.OnDeletedModelMessage;
    }
    void OnDeletedModelMessage(object sender, DeletedModelOnNetworkEventArgs e)
    {
        if (this.ModelIdentifier.IsSharedFromNetwork &&
            this.ModelIdentifier.Identifier == e.ModelIdentifier)
        {
            // This model has been deleted remotely, we need to get rid of it.
            this.Destroy();
        }
    }
    void OnTransformChangeMessage(object sender, TransformChangeEventArgs e)
    {
        if (this.ModelIdentifier.IsSharedFromNetwork &&
            (this.ModelIdentifier.Identifier == e.ModelIdentifier) &&
            (!this.isMulticastingTransforms)) // last clasuse should be redundant really
        {
            // We're interested...
            this.gameObject.transform.localScale = e.Scale;
            this.gameObject.transform.localRotation = e.Rotation;
            this.gameObject.transform.localPosition = e.Translation;
        }
    }
    public void AddHandManipulationsToModel()
    {
        this.isMulticastingTransforms = true;

        // Now need to add behaviours for rotate, transform, scale, etc.
        var twoHandManips = this.gameObject.AddComponent<TwoHandManipulatable>();
        twoHandManips.BoundingBoxPrefab = this.boundingBoxPrefab;
        twoHandManips.ManipulationMode = ManipulationMode.MoveScaleAndRotate;
        twoHandManips.RotationConstraint = AxisConstraint.None;
    }
    void Update()
    {
        // Note - our gameObject is *not* the gameObject that we are watching here, that's a
        // different gameObject which is provided by the CurrentModelProvider.
        if (this.isMulticastingTransforms)
        {
            var transform = this.gameObject.transform;

            if (!this.currentRotation.HasValue ||
                !this.currentRotation.Value.EqualToTolerance(transform.localRotation, ROTATION_TOLERANCE) ||
                !this.currentTranslation.Value.EqualToTolerance(transform.localPosition, POSITION_TOLERANCE) ||
                !this.currentScale.Value.EqualToTolerance(transform.localScale, SCALE_TOLERANCE))
            {
                // We need to broadcast.
                Debug.Log("We have a change in transform to talk about");
                this.currentRotation = transform.localRotation;
                this.currentTranslation = transform.localPosition;
                this.currentScale = transform.localScale;

                NetworkMessagingProvider.SendTransformChangeMessage(
                    (Guid)this.ModelIdentifier.Identifier,
                    (Vector3)this.currentScale,
                    (Quaternion)this.currentRotation,
                    (Vector3)this.currentTranslation);
            }
        }
    }
    Quaternion? currentRotation;
    Vector3? currentTranslation;
    Vector3? currentScale;
    bool isMulticastingTransforms;

    static readonly double ROTATION_TOLERANCE = 0.5d;
    static readonly double POSITION_TOLERANCE = 0.01d;
    static readonly double SCALE_TOLERANCE = 0.05d;
}