using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Physics;
using Microsoft.MixedReality.Toolkit.Utilities.Gltf.Serialization;
using MulticastMessaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

#if ENABLE_WINMD_SUPPORT
using Windows.Storage;
using Windows.Media.SpeechRecognition;
#endif // ENABLE_WINMD_SUPPORT

public class ModelController : MonoBehaviour, IMixedRealityInputActionHandler
{
#pragma warning disable 0649
    [SerializeField]
    GameObject parentPrefabWithInteractionConfigured;

    [SerializeField]
    InputActionHandlerPair[] inputActionHandlers;
#pragma warning restore 0649

    RootParentProvider RootParentProvider => this.gameObject.GetComponent<RootParentProvider>();
    ModelPositioningManager ModelLoader => this.gameObject.GetComponent<ModelPositioningManager>();
    AudioManager AudioManager => this.gameObject.GetComponent<AudioManager>();
    ProgressRingManager ProgressRingManager => this.gameObject.GetComponent<ProgressRingManager>();
    CursorManager CursorManager => this.gameObject.GetComponent<CursorManager>();
    INetworkMessagingProvider NetworkMessagingProvider => MixedRealityToolkit.Instance.GetService<INetworkMessagingProvider>();
    ISingleShotSpeechRecognitionService SpeechService => MixedRealityToolkit.Instance.GetService<ISingleShotSpeechRecognitionService>();
    IGltfFilePickerService GltfFilePickerService => MixedRealityToolkit.Instance.GetService<IGltfFilePickerService>();
    IDialogService DialogService => MixedRealityToolkit.Instance.GetService<IDialogService>();

    void Start()
    {
#if !UNITY_EDITOR
        // Note: I have configured this service to only "support" the platform of 
        // "UWP" but I find it still runs in the editor so I switch it off here.
        // This does nothing if we have no IP address connected to a network.
        NetworkMessagingProvider.NewModelOnNetwork += this.OnNewModelFromNetwork;
        NetworkMessagingProvider.Initialise();

        // Note: I have configured the speech service so that it only "supports" the platform
        // of "UWP" and so I do not expect it to run inside the editor. But it does...
        // https://github.com/microsoft/MixedRealityToolkit-Unity/issues/5205
        // Consequently, I'm conditionally compiling it as well so that it *doesn't*
        if (this.SpeechService != null)
        {
            this.InitialiseSpeechCommandsAsync();
        }
#else
        // Register so that we pick up the input actions that are raised globabally...
        MixedRealityToolkit.InputSystem.Register(this.gameObject);
#endif // UNITY_EDITOR
    }
    public void OnActionStarted(BaseInputEventData eventData)
    {
        var actionHandler = this.inputActionHandlers.FirstOrDefault(a => a.action == eventData.MixedRealityInputAction);
        actionHandler?.handler.Invoke();
    }
    public void OnActionEnded(BaseInputEventData eventData)
    {
    }
    async void InitialiseSpeechCommandsAsync()
    {
        // No await here, we just fire and forget...
        _ = this.SpeechService.RecogniseAndDispatchCommandsAsync(this.inputActionHandlers);
    }
    protected virtual void OnDisable()
    {
        MixedRealityToolkit.InputSystem?.Unregister(gameObject);
    }
    public async void OnOpenSpeechCommand()
    {
        var filePath = await this.PickFileFrom3DObjectsFolderAsync();

        if (!string.IsNullOrEmpty(filePath))
        {
            var modelDetails = await this.ImportGLTFModelFromFileAsync(filePath);

            if (modelDetails != null)
            {
                // Our new model comes out of the file load.
                var newModel = modelDetails.GameObject;

                // Give the new model a new identity.
                var modelIdentifier = newModel.AddComponent<ModelIdentifier>();

                // Add positioning and parent.
                var positioningManager = newModel.AddComponent<ModelPositioningManager>();

                positioningManager.CreateAndPositionParentAndModel(
                    this.RootParentProvider.RootParent,
                    this.parentPrefabWithInteractionConfigured);

                // Add manipulations to the new model so it can be moved around.
                var manipManager = newModel.AddComponent<ModelUpdatesManager>();
                manipManager.AddHandManipulationsToModel();

                // Add an anchor to the new model's parent such that we can then
                // anchor the parent while moving the model around within it 
                // (anchored components can't move).
                var anchorManager = newModel.AddComponent<AnchorManager>();
                anchorManager.AddAnchorToModelParent();

                // Write out all the files that were part of loading this model
                // into a file in case they need sharing in future.
                await FileStorageManager.StoreFileListAsync(
                    (Guid)modelIdentifier.Identifier, modelDetails);

                // We don't play with world anchors in the editor, we just leave
                // that code out.
#if !UNITY_EDITOR
                // And export the anchor into the file system as well.
                // TODO: this will currently wait "for ever" for the world anchor to be
                // located which might be wildly optimistic, we should probably add some
                // notion of a timeout on that here too.
                var exportedAnchorBits = await anchorManager.ExportAnchorAsync();

                if (exportedAnchorBits != null)
                {
                    // Store that into the file system so that the web server can later
                    // serve it up on request.
                    await FileStorageManager.StoreExportedWorldAnchorAsync(
                        (Guid)modelIdentifier.Identifier,
                        exportedAnchorBits);

                    // Message out to the network that we have a new model that they
                    // can optionally grab if they want to.
                    NetworkMessagingProvider.SendNewModelMessage(
                        (Guid)modelIdentifier.Identifier);
                }
#endif // UNITY_EDITOR
            }
        }
    }
    public async void OnToggleProfilerSpeechCommand()
    {
        // TODO: take this out of here, it belongs somewhere more 'global'
        MixedRealityToolkit.DiagnosticsSystem.ShowProfiler =
            !MixedRealityToolkit.DiagnosticsSystem.ShowProfiler;
    }

    public async void OnResetSpeechCommand()
    {
        bool first = true;

        var modelPositioningManagers = this.GetFocusedObjectWithChildComponent<ModelPositioningManager>();

        foreach (var modelPositioningManager in modelPositioningManagers)
        {
            if (first)
            {
                this.AudioManager.PlayClip(AudioClipType.Resetting);
                first = !first;
            }
            modelPositioningManager.ReturnModelToLoadedPosition();
        }
    }
    public async void OnRemoveSpeechCommand()
    {
        var modelIdentifiers = this.GetFocusedObjectWithChildComponent<ModelIdentifier>();

        foreach (var modelIdentifier in modelIdentifiers)
        {
            // Send network message saying we have got rid of this object in case others
            // are displaying it.
            NetworkMessagingProvider.SendDeletedModelMessage((Guid)modelIdentifier.Identifier);

            modelIdentifier.GetComponent<ModelPositioningManager>().Destroy();
        }
    }
    void ShowBusy(string message)
    {
        this.CursorManager.Hide();
        this.ProgressRingManager.Show(message);
    }
    void HideBusy()
    {
        this.ProgressRingManager.Hide();
        this.CursorManager.Show();
    }
    IEnumerable<T> GetFocusedObjectWithChildComponent<T>() where T : MonoBehaviour
    {
        // TODO: I need to figure whether this is the right way to do things. Is it right
        // to get all the active pointers, ask them what is focused & then use that as
        // the list of focused objects?
        var pointers = MixedRealityToolkit.InputSystem.FocusProvider.GetPointers<IMixedRealityPointer>()
            .Where(p => p.IsActive);

        foreach (var pointer in pointers)
        {
            FocusDetails focusDetails;

            if (MixedRealityToolkit.InputSystem.FocusProvider.TryGetFocusDetails(
                pointer, out focusDetails))
            {
                var component = focusDetails.Object?.GetComponentInChildren<T>();

                if (component != null)
                {
                    yield return component;
                }
            }
        }
    }
    async Task<string> DownloadModelToLocalStorageAsync(
        Guid modelIdentifier, IPAddress ipAddress)
    {
        var modelFilePath = string.Empty;

        var modelDownloader = new HttpModelDownloader(modelIdentifier, ipAddress);

        try
        {
            // Try and download that model from the remote HoloLens
            modelFilePath = await modelDownloader.DownloadModelToLocalStorageAsync();
        }
        catch
        {
            // TODO: figure out sensible exceptions here.
        }
        return (modelFilePath);
    }
    async Task<string> PickFileFrom3DObjectsFolderAsync()
    {
        var filePath = string.Empty;

        filePath = await this.GltfFilePickerService.PickFileAsync();

        // Did they choose a path outside of the 3D objects folder?
        if (filePath == string.Empty)
        {
            this.AudioManager.PlayClip(AudioClipType.PickFileFrom3DObjectsFolder);
        }
        return (filePath);
    }
    async Task<ImportedModelInfo> ImportGLTFModelFromFileAsync(string filePath)
    {
        this.ShowBusy("Loading model...");

        ImportedModelInfo modelDetails = null;

        try
        {
            var gltfObject = await GltfUtility.ImportGltfObjectFromPathAsync(filePath);

            if (gltfObject != null)
            {
                modelDetails = new ImportedModelInfo(filePath, gltfObject);
            }
        }
        catch
        {
            // TODO: figure out sensible exceptions here.
        }
        this.HideBusy();

        if (modelDetails?.GameObject != null)
        {
            this.AudioManager?.PlayClipOnceOnly(AudioClipType.FirstModelOpened);
        }
        else
        {
            this.AudioManager?.PlayClip(AudioClipType.LoadError);
        }
        return (modelDetails);
    }
#if !UNITY_EDITOR
    async void OnNewModelFromNetwork(object sender, NewModelOnNetworkEventArgs e)
    {
        var acceptDownload = await DialogService.AskYesNoQuestionAsync(
            "New Model Available",
            "Someone on the network has opened a new model, do you want to access it?");

        if (acceptDownload)
        {
            this.ShowBusy("Downloading model files...");

            // Grab all the models files over the network from the original device.
            var modelFilePath = await this.DownloadModelToLocalStorageAsync(
                e.ModelIdentifier, e.IPAddress);

            if (!string.IsNullOrEmpty(modelFilePath))
            {
                // Open the model from the file as we would if the user had selected
                // it via a dialog.
                var modelDetails = await this.ImportGLTFModelFromFileAsync(modelFilePath);
                var newModel = modelDetails.GameObject;

                // Flag that the ID of this model came from over the network, we didn't
                // open it ourselves.
                var modelIdentifier = newModel.AddComponent<ModelIdentifier>();
                modelIdentifier.AssignExistingFromNetworkedShare(e.ModelIdentifier);

                // Add positioning.
                var positioningManager = newModel.AddComponent<ModelPositioningManager>();

                positioningManager.CreateAndPositionParentAndModel(
                    this.RootParentProvider.RootParent,
                    this.parentPrefabWithInteractionConfigured);

                // And updates (this handles incoming transformation messages along with removal messages too)
                newModel.AddComponent<ModelUpdatesManager>();

                // With this model coming from the network, we want to import
                // the anchor onto the parent to put it into the same place within
                // the space as on the device it originated from.
                var worldAnchorBits =
                    await FileStorageManager.LoadExportedWorldAnchorAsync(modelIdentifier.Identifier);

                if (worldAnchorBits != null)
                {
                    var anchorManager = newModel.AddComponent<AnchorManager>();

                    // TODO: if this fails then?...
                    await anchorManager.ImportAnchorToModelParent(worldAnchorBits);
                }
                // Let the user know about what to expect with their first shared model.
                this.AudioManager.PlayClipOnceOnly(AudioClipType.FirstSharedModelOpened);
            }
            else
            {
                this.AudioManager.PlayClip(AudioClipType.ErrorDownloadingModel);
            }
            this.HideBusy();
        }
    }
#endif // UNITY_EDITOR
}