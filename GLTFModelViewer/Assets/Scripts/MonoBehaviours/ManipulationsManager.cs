using HoloToolkit.Unity.InputModule.Utilities.Interactions;
using HoloToolkit.Unity.UX;
using System;
using UnityEngine;

public class ManipulationsManager : MonoBehaviour
{
    [SerializeField]
    BoundingBox boundingBoxPrefab;

    CurrentModelProvider CurrentModelProvider => this.gameObject.GetComponent<CurrentModelProvider>();
    ModelIdentifier ModelIdentifier => this.gameObject.GetComponent<ModelIdentifier>();

    void Start()
    {
        NetworkMessagingProvider.TransformChange += OnTransformChangeMessage;
    }
    void OnTransformChangeMessage(object sender, TransformChangeEventArgs e)
    {
        if (this.ModelIdentifier.HasModel &&
            this.ModelIdentifier.IsSharedFromNetwork &&
            (this.ModelIdentifier.Identifier == e.ModelIdentifier) &&
            (!this.isMulticastingTransforms)) // last clasuse should be redundant really
        {
            // We're interested...
            var gameObject = this.CurrentModelProvider.CurrentModel;

            if (gameObject != null)
            {
                gameObject.transform.localScale = e.Scale;
                gameObject.transform.localRotation = e.Rotation;
                gameObject.transform.localPosition = e.Translation;
            }
        }
    }
    public void AddHandManipulationsToModel()
    {
        this.isMulticastingTransforms = true;

        // Now need to add behaviours for rotate, transform, scale, etc.
        var twoHandManips = this.CurrentModelProvider.CurrentModel.AddComponent<TwoHandManipulatable>();
        twoHandManips.BoundingBoxPrefab = this.boundingBoxPrefab;
        twoHandManips.ManipulationMode = ManipulationMode.MoveScaleAndRotate;
        twoHandManips.RotationConstraint = AxisConstraint.None;
    }
    public void RemoveManipulationsFromModel()
    {
        this.isMulticastingTransforms = false;
        this.currentRotation = null;
        this.currentScale = null;
        this.currentTranslation = null;

        var manipulations = this.CurrentModelProvider.CurrentModel?.GetComponent<TwoHandManipulatable>();

        if (manipulations != null)
        {
            Destroy(manipulations);
        }
    }
    void Update()
    {
        // Note - our gameObject is *not* the gameObject that we are watching here, that's a
        // different gameObject which is provided by the CurrentModelProvider.
        if (this.isMulticastingTransforms)
        {
            var transform = this.CurrentModelProvider.CurrentModel.transform;
            
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