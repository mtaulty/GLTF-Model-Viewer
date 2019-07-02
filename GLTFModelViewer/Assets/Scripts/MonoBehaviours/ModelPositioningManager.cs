using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.WSA;

public class ModelPositioningManager : MonoBehaviour
{
    Vector3? initialScaleFactor;
    Transform initialLookPoint;

    public GameObject AnchoredParent { get; private set; }
    public GameObject InteractableParent { get; private set; }

    public void ReturnModelToLoadedPosition()
    {
        this.InteractableParent.transform.localPosition = Vector3.zero;
        this.InteractableParent.transform.localRotation = Quaternion.identity;
        this.InteractableParent.transform.localScale = (Vector3)this.initialScaleFactor;
        this.InteractableParent.transform.LookAt(this.initialLookPoint);
    }
    public void Destroy()
    {
        Destroy(this.AnchoredParent.gameObject);
    }
    public void CreateAndPositionParentAndModel(
        GameObject sceneParentObject,
        GameObject configuredInteractionPrefab)
    {
        // We have a parent for the world anchor which lives under the 'root' parent in the scene.
        this.AnchoredParent = new GameObject("Anchored Parent");
        this.AnchoredParent.transform.SetParent(sceneParentObject.transform, false);

        // We have a parent which has the pre-configured interactions on it etc.
        // This comes with a BoxCollider, NearInteractionGrab and BoundingBox already on it.
        this.InteractableParent = GameObject.Instantiate(configuredInteractionPrefab);
        this.InteractableParent.transform.SetParent(this.AnchoredParent.transform, false);

        // Try to work out the size of the rendered model in local co-ords.
        var renderBounds = this.CalculateRendererBounds();

        // Size the model so as to scale it to be approx 0.5m in the largest direction.
        this.SizeModel(renderBounds);

        var boxCollider = this.InteractableParent.GetComponent<BoxCollider>();
        var nearGrabbable = this.InteractableParent.GetComponent<NearInteractionGrabbable>();
        var manipulationHandler = this.InteractableParent.GetComponent<ManipulationHandler>();
        boxCollider.enabled = nearGrabbable.enabled = manipulationHandler.enabled = false;

        // Try and set the position and size of the BoxCollider before we mess with
        // any other transformations.
        boxCollider.center = this.gameObject.transform.InverseTransformPoint(renderBounds.center);
        boxCollider.size = this.gameObject.transform.InverseTransformVector(renderBounds.size);

        // Move the parent to be approx 3m down the user's gaze.
        var parentPosition =
            Camera.main.transform.position +
            Camera.main.transform.forward * MODEL_START_DISTANCE;

        // Patch up the y-value to try and line it up with the head position.
        parentPosition.y = Camera.main.transform.position.y;

        // move to parent to be on the gaze vector from the camera
        this.AnchoredParent.transform.position = parentPosition;

        // make the parent look at the camera
        this.initialLookPoint = Camera.main.transform;
        this.AnchoredParent.transform.LookAt(this.initialLookPoint);

        // our model is now a child of the newly created parent
        this.gameObject.transform.SetParent(this.InteractableParent.transform, false);

        // move the model within the parent to be at the origin
        this.InteractableParent.transform.localPosition = Vector3.zero;
        this.gameObject.transform.localPosition = Vector3.zero;
    }
    void SizeModel(Bounds rendererBounds)
    {
        // what's the max extent here? 
        var maxDimension = Mathf.Max(
            rendererBounds.size.x, rendererBounds.size.y, rendererBounds.size.z);

        // what the scale factor we need then (extent is half the size of the box).
        var scaleFactor = MODEL_START_SIZE / maxDimension;

        // scale it.
        this.InteractableParent.transform.localScale = Vector3.one * scaleFactor;

        // record it so that we can put it back on the 'reset' command.
        this.initialScaleFactor = this.InteractableParent.transform.localScale;
    }
    Bounds CalculateRendererBounds()
    {
        var currentRenderer = this.gameObject.GetComponent<Renderer>();
        Bounds? bounds = null;

        if (currentRenderer != null)
        {
            bounds = currentRenderer.bounds;
        }
        foreach (var renderer in this.gameObject.transform.GetComponentsInChildren<Renderer>())
        {
            if (bounds == null)
            {
                bounds = renderer.bounds;
            }
            else
            {
                bounds.Value.Encapsulate(renderer.bounds);
            }
        }
        return (bounds.Value);
    }
    static readonly float MODEL_START_SIZE = 0.5f;
    static readonly float MODEL_START_DISTANCE = 1.5f;
}
