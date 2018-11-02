using HoloToolkit.Unity.InputModule.Utilities.Interactions;
using HoloToolkit.Unity.UX;
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
            this.CurrentModel.transform.localScale = Vector3.one;
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
            // Try to load that model
            // TODO: what's the root path here?
            // TODO: set the parent here
            GLTFSceneImporter importer = new GLTFSceneImporter(
                "/", stream, null, true);

            await base.RunCoroutineAsync(importer.Load(-1, true));

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

    void AddNewGLTFModel(GameObject lastLoadedScene)
    {
        // Move the parent to be approx 3m down the user's gaze.
        var parentPosition =
            Camera.main.transform.position +
            Camera.main.transform.forward * MODEL_START_DISTANCE;

        // Patch up the y-value to try and line it up with the head position.
        parentPosition.y = Camera.main.transform.position.y;

        this.GLTFModelParent.transform.position = parentPosition;

        // Parent the new model off our parent object.
        lastLoadedScene.transform.SetParent(this.GLTFModelParent.transform, false);

        // Need to do something about setting the model to a reasonable size.

        // Now need to add behaviours for rotate, transform, scale, etc.
        var twoHandManips = lastLoadedScene.gameObject.AddComponent<TwoHandManipulatable>();
        twoHandManips.BoundingBoxPrefab = this.boundingBoxPrefab;
        twoHandManips.ManipulationMode = ManipulationMode.MoveScaleAndRotate;
        twoHandManips.RotationConstraint = AxisConstraint.None;
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
        }
    }
    static readonly float MODEL_START_DISTANCE = 3.0f;
}
