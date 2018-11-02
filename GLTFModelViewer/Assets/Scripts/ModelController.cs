using HoloToolkit.Unity.InputModule.Utilities.Interactions;
using HoloToolkit.Unity.UX;
using HoloToolkit.UX.Progress;
using System.Threading.Tasks;
using UnityEngine;
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
            ProgressIndicator.Instance.Open(
                IndicatorStyleEnum.AnimatedOrbs, 
                ProgressStyleEnum.ProgressBar, 
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
        this.PositionParentForModel();

        // Parent the new model off our parent object.
        loadedModel.transform.SetParent(this.GLTFModelParent.transform, false);
        loadedModel.transform.localPosition = Vector3.zero;

        this.AddManipulationsToModel(loadedModel);

        this.InitialSizeModel(loadedModel);
    }

    void InitialSizeModel(GameObject loadedModel)
    {
        // Need to do something about setting the model to a reasonable size.
        var collider = loadedModel.gameObject.GetComponentInChildren<Collider>();

        if (collider != null)
        {
            // TODO: does querying bounds like this work at this point?
            var bounds = collider.bounds;

            // what's the max extent here? 
            var maxExtent = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);

            // what the scale factor we need then (extent is half the size of the box).
            var scaleFactor = MODEL_START_SIZE / (maxExtent * 2.0f);

            // scale it.
            loadedModel.gameObject.transform.localScale = Vector3.one * scaleFactor;

            // record it so that we can put it back on the 'reset' command.
            this.initialScaleFactor = loadedModel.gameObject.transform.localScale;
        }
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
        this.GLTFModelParent.transform.position = parentPosition;
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
        }
    }
    static readonly float MODEL_START_SIZE = 0.5f;
    static readonly float MODEL_START_DISTANCE = 3.0f;
}
