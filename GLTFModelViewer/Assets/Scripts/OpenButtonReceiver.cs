using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.Receivers;
using UnityEngine;
using UnityGLTF;

public class OpenButtonReceiver : InteractionReceiver
{
    protected override async void InputClicked(GameObject obj, InputClickedEventData eventData)
    {
        base.InputClicked(obj, eventData);

        if (obj.name == this.interactables[OPEN_BUTTON_INDEX].name)
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

                await AwaitableCoRoutine.RunCoroutineAsync(importer.Load(-1, true));

                // Did we load?
                if (importer.LastLoadedScene != null)
                {
                }
            }
        }
    }
    // I don't really like hard-coding this, should do something better.
    static readonly int OPEN_BUTTON_INDEX = 0;
}
