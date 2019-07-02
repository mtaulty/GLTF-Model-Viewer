using System.Linq;
using UnityEngine;

public class ModelPositioningManager : MonoBehaviour
{
    Vector3? initialScaleFactor;

    Transform initialLookPoint;

    public void ReturnModelToLoadedPosition()
    {
        this.gameObject.transform.localPosition = Vector3.zero;
        this.gameObject.transform.localRotation = Quaternion.identity;
        this.gameObject.transform.localScale = (Vector3)this.initialScaleFactor;
        this.gameObject.transform.LookAt(this.initialLookPoint);
    }
    public void InitialiseSizeAndPositioning(GameObject parentObject)
    {
        // Move the parent to be down the gaze vector.
        this.CreateAndPositionParentAndModel(parentObject);

        // Size the new model so that it displays reasonably.
        this.SizeModel();
    }
    void CreateAndPositionParentAndModel(GameObject parentObject)
    {
        // Move the parent to be approx 3m down the user's gaze.
        var parentPosition =
            Camera.main.transform.position +
            Camera.main.transform.forward * MODEL_START_DISTANCE;

        // Patch up the y-value to try and line it up with the head position.
        parentPosition.y = Camera.main.transform.position.y;

        // Move the parent to this new position. From there, the parent doesn't
        // get moved, scaled, rotated, only the model (child) will.
        var parent = new GameObject();

        // this parent is a child of our root for content within the scene.
        parent.transform.SetParent(parentObject.transform, false);

        // move to parent to be on the gaze vector from the camera
        parent.transform.position = parentPosition;

        // make the parent look at the camera
        this.initialLookPoint = Camera.main.transform;
        parent.transform.LookAt(this.initialLookPoint);

        // our model is now a child of the newly created parent
        this.gameObject.transform.SetParent(parent.transform, false);

        // move the model within the parent to be at the origin
        this.gameObject.transform.localPosition = Vector3.zero;
    }
    void SizeModel()
    {
        // Try to figure out how big the object is (this turns out to be
        // more of an art than a science :-S).
        var bounds = CalculateMeshRendererSizes(this.gameObject.transform);

        // what's the max extent here? 
        if (bounds.HasValue)
        {
            var maxDimension = Mathf.Max(
                bounds.Value.size.x, bounds.Value.size.y, bounds.Value.size.z);

            // what the scale factor we need then (extent is half the size of the box).
            var scaleFactor = MODEL_START_SIZE / maxDimension;

            // scale it.
            this.gameObject.transform.localScale = Vector3.one * scaleFactor;
        }
        // record it so that we can put it back on the 'reset' command.
        this.initialScaleFactor = this.gameObject.transform.localScale;
    }
    static Bounds? CalculateMeshRendererSizes(Transform objectTransform)
    {
        var thisFilter = objectTransform.GetComponent<Renderer>();
        var childFilters = objectTransform.GetComponentsInChildren<Renderer>(true).ToList();

        if (thisFilter != null)
        {
            childFilters.Insert(0, thisFilter);
        }
        Bounds? result = null;

        foreach (var filter in childFilters)
        {
            var bounds = filter.bounds;

            if (result == null)
            {
                result = bounds;
            }
            else
            {
                result.Value.Encapsulate(bounds.min);
                result.Value.Encapsulate(bounds.max);
            }
        }
        return result;
    }
    static readonly float MODEL_START_SIZE = 0.5f;
    static readonly float MODEL_START_DISTANCE = 3.0f;
}
