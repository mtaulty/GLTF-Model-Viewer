using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

#if ENABLE_WINMD_SUPPORT
using Windows.Storage;
#endif // ENABLE_WINMD_SUPPORT

public class ModelController : ExtendedMonoBehaviour
{
    bool Opening { get; set; }

    ModelLoader ModelLoader => this.gameObject.GetComponent<ModelLoader>();
    AudioManager AudioManager => this.gameObject.GetComponent<AudioManager>();
    ProgressRingManager ProgressRingManager => this.gameObject.GetComponent<ProgressRingManager>();
    CursorManager CursorManager => this.gameObject.GetComponent<CursorManager>();
    FileStorageManager FileStorageManager => this.gameObject.GetComponent<FileStorageManager>();
    ModelIdentifier ModelIdentifier => this.gameObject.GetComponent<ModelIdentifier>();
    AnchorManager AnchorManager => this.gameObject.GetComponent<AnchorManager>();
    NetworkMessageManager NetworkManager => this.gameObject.GetComponent<NetworkMessageManager>();

    void OnNewModelFromNetwork(object sender, NewModelOnNetworkEventArgs e)
    {
        // Someone has multicasted that there is a new model on the network
        // that we can now download to our local storage and potentially
        // open up.
    }
    public async void OnOpenSpeechCommand()
    {
        if (!this.Opening)
        {
            this.Opening = true;

            // Get rid of the previous model regardless of whether the user chooses
            // a new one or not with a review to avoiding cluttering the screen.
            this.ModelLoader.DisposeExistingGLTFModel();

            // Get rid of any spatial anchor on the parent so that we can re-add
            // it if necessary when the new model has loaded and the parent has
            // been moved to match the user's gaze.
            this.AnchorManager.RemoveAnchorFromModelParent();

            // Get rid of any identity associated with the model
            this.ModelIdentifier.Clear();

            // Note - this method will throw inside of the editor, only does something
            // on the UWP platform.
            var filePath = await this.PickFileFrom3DObjectsFolderAsync();

            if (!string.IsNullOrEmpty(filePath))
            {
                this.CursorManager.Hide();
                this.ProgressRingManager.Show("Loading...");

                // Load the model.
                this.ModelLoader.OpenNewModelAsync(
                    filePath,
                    this.OnLoadedCallback);
            }
            else
            {
                this.Opening = false;
            }
        }
    }
    async Task<string> PickFileFrom3DObjectsFolderAsync()
    {
        var filePath = string.Empty;

#if ENABLE_WINMD_SUPPORT
        var known3DObjectsFolder = KnownFolders.Objects3D.Path.ToLower().TrimEnd('\\');
#else
        var known3DObjectsFolder = string.Empty;

        throw new NotImplementedException("Sorry, only on UWP");
#endif // ENABLE_WINMD_SUPPORT

        do
        {
            filePath = await FileDialogHelper.PickGLTFFileAsync();

            if (!string.IsNullOrEmpty(filePath) &&
                !filePath.ToLower().StartsWith(known3DObjectsFolder))
            {
                filePath = string.Empty;
                this.AudioManager.PlayClipOnceOnly(AudioClipType.PickFileFrom3DObjectsFolder);
            }
        } while (filePath == string.Empty);

        return (filePath);
    }
    async void OnLoadedCallback(GameObject loadedObject, RecordingFileLoader fileRecorder)
    {
        this.Opening = false;
        this.ProgressRingManager.Hide();
        this.CursorManager.Show();

        if (loadedObject != null)
        {
            this.AudioManager?.PlayClipOnceOnly(AudioClipType.FirstModelOpened);

            // The new model should be on the gaze vector so the parent will have moved
            // so we need to put its anchor back
            this.AnchorManager.AddAnchorToModelParent();

            // We have a new model so we can reset our notion of its identity
            this.ModelIdentifier.CreateNew();

            // We can write out all the files that were part of loading this model
            // into a file in case they need sharing in future.
            await this.FileStorageManager.StoreFileListAsync(fileRecorder);

            // And export the anchor into the file system as well.
            var bits = await this.AnchorManager.ExportAnchorAsync();

            if (bits != null)
            {
                // Store that into the file system so that the web server can later
                // serve it up on request.
                await this.FileStorageManager.StoreExportedWorldAnchorAsync(bits);

                // Message out to the network that we have a new model that they
                // can optionally grab if they want to.
                this.NetworkManager.SendNewModelMessage((Guid)this.ModelIdentifier.Identifier);
            }
        }
        else
        {
            this.AudioManager?.PlayClip(AudioClipType.LoadError);
        }
    }
    public void OnResetSpeechCommand()
    {
        if (this.ModelIdentifier.HasModel)
        {
            this.AudioManager.PlayClip(AudioClipType.Resetting);

            this.ModelLoader.ReturnModelToLoadedPosition();
        }
    }   
}