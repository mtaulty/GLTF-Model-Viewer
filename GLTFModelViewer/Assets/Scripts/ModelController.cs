using HoloToolkit.UX.Dialog;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityGLTF;

public class ModelController : MonoBehaviour, IRunCoRoutine
{
    [SerializeField]
    private GameObject GLTFModelParent;

    [SerializeField]
    private Dialog dialogPrefab;

    public async Task OpenNewModelAsync()
    {
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

            await AwaitableCoRoutine.RunCoroutineAsync(this, importer.Load(-1, true));

            // Did we load?
            if (importer.LastLoadedScene != null)
            {
                // Get rid of the previous model.
                this.DisposeExistingGLTFModel();

                // Replace it with the new model
                this.AddNewGLTFModel(importer.LastLoadedScene);
            }
            else
            {
                Dialog.Open(this.dialogPrefab.gameObject,
                    DialogButtonType.OK,
                    "Error loading model",
                    "This model did not load correctly, possibly due to GLTF incompatibility");
            }
        }
    }

    public void RunCoRoutineWithCallback(IEnumerator coRoutine, Action callback)
    {
        this.StartCoroutine(RunCoRoutine(coRoutine, callback));
    }
    IEnumerator RunCoRoutine(IEnumerator coRoutine, Action callback)
    {
        yield return base.StartCoroutine(coRoutine);
        callback();
    }

    void AddNewGLTFModel(GameObject lastLoadedScene)
    {
        // Parent the new model off our parent object.
        lastLoadedScene.transform.SetParent(this.GLTFModelParent.transform, false);

        // Need to do something about setting the model to a reasonable size.

        // Now need to add behaviours for rotate, transform, scale, etc.
    }
    void DisposeExistingGLTFModel()
    {

    }
}
