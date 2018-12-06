using HoloToolkit.Unity.InputModule.Utilities.Interactions;
using HoloToolkit.Unity.UX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.WSA;
using UnityGLTF;

public class ModelLoader : ExtendedMonoBehaviour
{
    Vector3? initialScaleFactor;

    Transform initialLookPoint;

    ParentProvider ParentProvider => this.gameObject.GetComponent<ParentProvider>();
    CurrentModelProvider CurrentModelProvider => this.gameObject.GetComponent<CurrentModelProvider>();

    public async Task<LoadedModelInfo> OpenNewModelAsync(string filePath)
    {
        var modelDetails = new LoadedModelInfo(
            new RecordingFileLoader(Path.GetDirectoryName(filePath)));

        GLTFSceneImporter importer = new GLTFSceneImporter(Path.GetFileName(filePath), modelDetails.FileLoader);

        importer.Collider = GLTFSceneImporter.ColliderType.Box;

        await base.RunCoroutineAsync(
            importer.LoadScene(
                -1,
                gameObject => modelDetails.GameObject = gameObject)
        );        

        if (modelDetails.GameObject != null)
        {
            this.AddNewGLTFModel(modelDetails.GameObject);
        }
        return (modelDetails);
    }
    public void DisposeExistingGLTFModel()
    {
        if (this.CurrentModelProvider.HasModel)
        {
            Destroy(this.CurrentModelProvider.CurrentModel);
            this.initialScaleFactor = null;
            this.initialLookPoint = null;
        }
    }
    public void ReturnModelToLoadedPosition()
    {
        if (this.CurrentModelProvider.HasModel)
        {
            this.CurrentModelProvider.CurrentModel.transform.localPosition = Vector3.zero;
            this.CurrentModelProvider.CurrentModel.transform.localRotation = Quaternion.identity;
            this.CurrentModelProvider.CurrentModel.transform.localScale = (Vector3)this.initialScaleFactor;
            this.CurrentModelProvider.CurrentModel.transform.LookAt(this.initialLookPoint);
        }
    }
    void AddNewGLTFModel(GameObject loadedModel)
    {
        // Move the parent to be down the gaze vector.
        this.PositionParentForModel();

        // Parent the new model off the parent & position it.
        this.ParentAndPositionModel(loadedModel);

        // Size the new model so that it displays reasonably.
        this.InitialSizeModel(loadedModel);

        // Point the model towards the camera.
        this.initialLookPoint = Camera.main.transform;
        loadedModel.transform.LookAt(this.initialLookPoint);
    }
    void ParentAndPositionModel(GameObject loadedModel)
    {
        // Parent the new model off our parent object.
        loadedModel.transform.SetParent(this.ParentProvider.GLTFModelParent.transform, false);
        loadedModel.transform.localPosition = Vector3.zero;
    }
    void InitialSizeModel(GameObject loadedModel)
    {
        // Try to figure out how big the object is (this turns out to be
        // more of an art than a science :-S).
        var bounds = CalculateMeshRendererSizes(loadedModel.transform);

        // what's the max extent here? 
        if (bounds.HasValue)
        {
            var maxDimension = Mathf.Max(
                bounds.Value.size.x, bounds.Value.size.y, bounds.Value.size.z);

            // what the scale factor we need then (extent is half the size of the box).
            var scaleFactor = MODEL_START_SIZE / maxDimension;

            // scale it.
            loadedModel.gameObject.transform.localScale = Vector3.one * scaleFactor;
        }
        // record it so that we can put it back on the 'reset' command.
        this.initialScaleFactor = loadedModel.gameObject.transform.localScale;
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
    void PositionParentForModel()
    {
        // Move the parent to be approx 3m down the user's gaze.
        var parentPosition =
            Camera.main.transform.position +
            Camera.main.transform.forward * MODEL_START_DISTANCE;

        // Patch up the y-value to try and line it up with the head position.
        parentPosition.y = Camera.main.transform.position.y;

        // Move the parent to this new position. From there, the parent doesn't
        // get moved, scaled, rotated, only the model (child) will.
        this.ParentProvider.GLTFModelParent.transform.localPosition = parentPosition;
    }
    static readonly float MODEL_START_SIZE = 0.5f;
    static readonly float MODEL_START_DISTANCE = 3.0f;
}
