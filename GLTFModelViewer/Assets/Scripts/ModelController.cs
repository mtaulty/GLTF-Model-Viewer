using HoloToolkit.Unity.InputModule.Utilities.Interactions;
using HoloToolkit.Unity.UX;
using HoloToolkit.UX.Progress;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.WSA;
using UnityGLTF;
using UnityGLTF.Loader;

public class ModelController : AwaitableMonoBehaviour
{
    [SerializeField]
    private GameObject GLTFModelParent;

    [SerializeField]
    private BoundingBox boundingBoxPrefab;

    bool Opening { get; set; }

    Vector3? initialScaleFactor;
    Transform initialLookPoint;

    public void OnOpenSpeechCommand()
    {
        if (!this.Opening)
        {
            this.OpenNewModelAsync(
                gameObject => this.Opening = false);
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
            this.CurrentModel.transform.LookAt(this.initialLookPoint);
        }
    }
    public async void OpenNewModelAsync(Action<GameObject> completionCallback)
    {
        // Get rid of the previous model regardless of whether the user chooses
        // a new one or not with a review to avoiding cluttering the screen.
        this.DisposeExistingGLTFModel();

        // Note - this method will throw inside of the editor, only does something
        // on the UWP platform.
        var filePath = await FileDialogHelper.PickGLTFFileAsync();

        if (!string.IsNullOrEmpty(filePath))
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

            // Try to load that model.
            var loader = new FileLoader(Path.GetDirectoryName(filePath));

            GLTFSceneImporter importer = new GLTFSceneImporter(
                Path.GetFileName(filePath), loader);

            await base.RunCoroutineAsync(
                importer.LoadScene(
                    -1,
                    gameObject =>
                    {
                        ProgressIndicator.Instance.Close();

                        if (gameObject != null)
                        { 
                            // Replace it with the new model
                            this.AddNewGLTFModel(gameObject);
                        }
                        else
                        {
                            var audioController = this.gameObject.GetComponent<AudioController>();
                            audioController?.PlayClip(AudioClipType.LoadError);
                        }
                    }
                )
            );
        }
        else
        {
            completionCallback?.Invoke(null);
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

        // Add the behaviours which let the user size, scale, rotate the model.
        this.AddManipulationsToModel(loadedModel);
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
        // Unanchor the parent.
        var anchor = this.GLTFModelParent.GetComponent<WorldAnchor>();

        if (anchor != null)
        {
            Destroy(anchor);
        }

        // Move the parent to be approx 3m down the user's gaze.
        var parentPosition =
            Camera.main.transform.position +
            Camera.main.transform.forward * MODEL_START_DISTANCE;

        // Patch up the y-value to try and line it up with the head position.
        parentPosition.y = Camera.main.transform.position.y;

        // Move the parent to this new position. From there, the parent doesn't
        // get moved, scaled, rotated, only the model (child) will.
        this.GLTFModelParent.transform.localPosition = parentPosition;

        // Anchor the parent.
        this.GLTFModelParent.AddComponent<WorldAnchor>();
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
            Destroy(this.CurrentModel);
            this.initialScaleFactor = null;
            this.initialLookPoint = null;
        }
    }
    static readonly float MODEL_START_SIZE = 0.5f;
    static readonly float MODEL_START_DISTANCE = 3.0f;
}
