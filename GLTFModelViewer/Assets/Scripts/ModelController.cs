using HoloToolkit.Unity.InputModule.Utilities.Interactions;
using HoloToolkit.Unity.UX;
using HoloToolkit.UX.Progress;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.WSA;
using UnityGLTF;

public class ModelController : AwaitableMonoBehaviour
{
    [SerializeField]
    private GameObject GLTFModelParent;

    [SerializeField]
    private BoundingBox boundingBoxPrefab;

    bool Opening { get; set; }

    Vector3? initialScaleFactor;

    public async void OnOpenSpeechCommand()
    {
        if (!this.Opening)
        {
            try
            {
                this.Opening = true;
                await this.OpenNewModelAsync();
            }
            finally
            {
                this.Opening = false;
            }
        }
    }
    public void OnResetSpeechCommand()
    {
        if (this.CurrentModel != null)
        {
            var audioController = this.gameObject.GetComponent<AudioController>();
            audioController?.PlayClip(AudioClipType.Resetting);

            this.CurrentModel.transform.localPosition = Vector3.zero;
            this.CurrentModel.transform.localRotation = Quaternion.identity;
            this.CurrentModel.transform.localScale = (Vector3)this.initialScaleFactor;
        }
    }
    public async Task OpenNewModelAsync()
    {
        // Get rid of the previous model regardless of whether the user chooses
        // a new one or not with a review to avoiding cluttering the screen.
        this.DisposeExistingGLTFModel();

        // Note - this method will throw inside of the editor, only does something
        // on the UWP platform.
        var stream = await FileDialogHelper.PickGLTFFileAsync();

        if (stream != null)
        {
            // TODO: this progress indicator is broken at the moment. I've 
            // parented it off the camera for now which I'm not meant to do
            // but the examples in the toolkit don't work for me either -
            // the progress opens up in weird places, doesn't tag-a-long
            // and I'm not sure of the best way to fix it yet without
            // just making my own progress indicator.
            ProgressIndicator.Instance.Open(
                IndicatorStyleEnum.AnimatedOrbs,
                ProgressStyleEnum.None,
                ProgressMessageStyleEnum.Visible,
                "Loading...");

            // Try to load that model
            GLTFSceneImporter importer = new GLTFSceneImporter(
                "/", stream, null, true);

            await base.RunCoroutineAsync(importer.Load(-1, true));

            ProgressIndicator.Instance.Close();

            // Did we load?
            if (importer.LastLoadedScene != null)
            {
                // Replace it with the new model
                this.AddNewGLTFModel(importer.LastLoadedScene);
            }
            else
            {
                var audioController = this.gameObject.GetComponent<AudioController>();
                audioController?.PlayClip(AudioClipType.LoadError);
            }
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
        loadedModel.transform.LookAt(Camera.main.transform);

        // Add the behaviours which let the user size, scale, rotate the model.
        this.AddManipulationsToModel(loadedModel);

        // Add a world anchor (we don't try to be smart with this, we just add one)
        this.AddWorldAnchorToModel(loadedModel);

    }
    void AddWorldAnchorToModel(GameObject loadedModel)
    {
        var worldAnchor = loadedModel.gameObject.AddComponent<WorldAnchor>();        
    }
    void ParentAndPositionModel(GameObject loadedModel)
    {
        // Parent the new model off our parent object.
        loadedModel.transform.SetParent(this.GLTFModelParent.transform, false);
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
        var thisFilter = objectTransform.GetComponent<MeshRenderer>();
        var childFilters = objectTransform.GetComponentsInChildren<MeshRenderer>(true).ToList();
        
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

    void AddManipulationsToModel(GameObject loadedModel)
    {
        // Now need to add behaviours for rotate, transform, scale, etc.
        var twoHandManips = loadedModel.gameObject.AddComponent<TwoHandManipulatable>();
        twoHandManips.BoundingBoxPrefab = this.boundingBoxPrefab;
        twoHandManips.ManipulationMode = ManipulationMode.MoveScaleAndRotate;
        twoHandManips.RotationConstraint = AxisConstraint.None;
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
        this.GLTFModelParent.transform.localPosition = parentPosition;
    }
    GameObject CurrentModel
    {
        get
        {
            GameObject currentModel = null;
            if (this.GLTFModelParent.transform.childCount > 0)
            {
                currentModel = this.GLTFModelParent.transform.GetChild(0).gameObject;
            }
            return (currentModel);
        }
    }
    void DisposeExistingGLTFModel()
    {
        if (this.CurrentModel != null)
        {
            Destroy(this.CurrentModel.GetComponent<TwoHandManipulatable>());
            Destroy(this.CurrentModel.GetComponent<WorldAnchor>());
            Destroy(this.CurrentModel);
            this.initialScaleFactor = null;
        }
    }
    static readonly float MODEL_START_SIZE = 0.5f;
    static readonly float MODEL_START_DISTANCE = 3.0f;
}
