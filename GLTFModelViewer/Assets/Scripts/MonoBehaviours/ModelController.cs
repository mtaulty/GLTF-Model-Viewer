using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

#if ENABLE_WINMD_SUPPORT
using Windows.Storage;
using Windows.Media.SpeechRecognition;
#endif // ENABLE_WINMD_SUPPORT

public class ModelController : ExtendedMonoBehaviour
{
    ModelLoader ModelLoader => this.gameObject.GetComponent<ModelLoader>();
    AudioManager AudioManager => this.gameObject.GetComponent<AudioManager>();
    ProgressRingManager ProgressRingManager => this.gameObject.GetComponent<ProgressRingManager>();
    CursorManager CursorManager => this.gameObject.GetComponent<CursorManager>();
    FileStorageManager FileStorageManager => this.gameObject.GetComponent<FileStorageManager>();
    ModelIdentifier ModelIdentifier => this.gameObject.GetComponent<ModelIdentifier>();
    AnchorManager AnchorManager => this.gameObject.GetComponent<AnchorManager>();
    ManipulationsManager ManipulationsManager => this.gameObject.GetComponent<ManipulationsManager>();

    static readonly string OPEN_SPEECH_TEXT = "open";
    static readonly string RESET_SPEECH_TEXT = "reset";

#if ENABLE_WINMD_SUPPORT
    SpeechRecognizer recognizer;
#endif // ENABLE_WINMD_SUPPORT

    void Start()
    {
        // This does nothing if we have no IP address connected to a network
        NetworkMessagingProvider.NewModelOnNetwork += this.OnNewModelFromNetwork;
        NetworkMessagingProvider.Initialise();

#if ENABLE_WINMD_SUPPORT
        this.StartSpeechCommandHandlingAsync();
#endif // ENABLE_WINMD_SUPPORT
    }
#if ENABLE_WINMD_SUPPORT    
    /// <summary>
    /// Why am I using my own speech handling rather than relying on SpeechInputSource and
    /// SpeechInputHandler? I started using those and they worked fine.
    /// However, I found that my speech commands would stop working across invocations of
    /// the file open dialog. They would work *before* and *stop* after.
    /// I spent a lot of time on this and I found that things would *work* under the debugger
    /// but not without it.
    /// That led me to think that this related to suspend/resume and perhaps HoloLens suspends
    /// the app when you move to the file dialog because I notice that dialog running as its
    /// own app on HoloLens.
    /// I tried hard to do work with suspend/resume but I kept hitting problems and so I wrote
    /// my own code where I try quite hard to avoid a single instance of SpeechRecognizer being
    /// used more than once - i.e. I create it, recognise with it & throw it away each time
    /// as this seems to *actually work* better than any other approach I tried.
    /// I also find that SpeechRecognizer.RecognizeAsync can get into a situation where it
    /// returns "Success" and "Rejected" at the same time & once that happens you don't get
    /// any more recognition unless you throw it away and so that's behind my approach.
    /// </summary>
    async void StartSpeechCommandHandlingAsync()
    {
        while (true)
        {
            var command = await this.SelectSpeechCommandAsync(
                OPEN_SPEECH_TEXT, RESET_SPEECH_TEXT);

            if (string.Compare(OPEN_SPEECH_TEXT, command, true) == 0)
            {
                this.OnOpenSpeechCommand();
            }
            else if (string.Compare(RESET_SPEECH_TEXT, command, true) == 0)
            {
                this.OnResetSpeechCommand();
            }
            else
            {
                // Just being paranoid in case we start spinning around here
                // My expectatation is that this code should never/rarely
                // execute.
                await Task.Delay(250);
            }
        }
    }
    async Task<string> SelectSpeechCommandAsync(params string[] commands)
    {
        string command = string.Empty;

        using (var recognizer = new SpeechRecognizer())
        {
            recognizer.Constraints.Add(new SpeechRecognitionListConstraint(commands));
            await recognizer.CompileConstraintsAsync();

            var result = await recognizer.RecognizeAsync();

            if ((result.Status == SpeechRecognitionResultStatus.Success) &&
                ((result.Confidence == SpeechRecognitionConfidence.Medium) ||
                 (result.Confidence == SpeechRecognitionConfidence.High)))
            {
                command = result.Text;
            }                    
        }
        return (command);
    }

#endif // ENABLE_WINMD_SUPPORT

    public async void OnOpenSpeechCommand()
    {
        this.ClearExistingModel();

        var filePath = await this.PickFileFrom3DObjectsFolderAsync();

        if (!string.IsNullOrEmpty(filePath))
        {
            var modelDetails = await this.OpenModelFileAsync(filePath);

            if (modelDetails != null)
            {
                // The new model should be on the gaze vector so the parent will have moved
                // so we need to put its anchor back
                this.AnchorManager.AddAnchorToModelParent();

                // We have a new model so we can reset our notion of its identity
                this.ModelIdentifier.CreateNew();

                // Make it so that this model can be manipulated with gestures.
                this.ManipulationsManager.AddHandManipulationsToModel();

                // We can write out all the files that were part of loading this model
                // into a file in case they need sharing in future.
                await this.FileStorageManager.StoreFileListAsync(modelDetails.FileLoader);

                // And export the anchor into the file system as well.
                // TODO: this will currently wait "for ever" for the world anchor to be
                // located which might be wildly optimistic, we should probably add some
                // notion of a timeout on that here too.
                var exportedAnchorBits = await this.AnchorManager.ExportAnchorAsync();

                if (exportedAnchorBits != null)
                {
                    // Store that into the file system so that the web server can later
                    // serve it up on request.
                    await this.FileStorageManager.StoreExportedWorldAnchorAsync(exportedAnchorBits);

                    // Message out to the network that we have a new model that they
                    // can optionally grab if they want to.
                    NetworkMessagingProvider.SendNewModelMessage((Guid)this.ModelIdentifier.Identifier);
                }
            }
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
    async Task<string> DownloadModelToLocalStorageAsync(
        Guid modelIdentifier, IPAddress ipAddress)
    {
        var modelFilePath = string.Empty;

        var modelDownloader = new HttpModelDownloader(
            this.FileStorageManager, modelIdentifier, ipAddress);

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
    async void OnNewModelFromNetwork(object sender, NewModelOnNetworkEventArgs e)
    {
        var acceptDownload = await MessageDialogHelper.AskYesNoQuestionAsync(
            "New Model Available",
            "Someone on the network has opened a new model, do you want to access it?");

        if (acceptDownload)
        {
            this.ClearExistingModel();

            this.ShowBusy("Downloading model files...");

            var modelFilePath = await this.DownloadModelToLocalStorageAsync(
                e.ModelIdentifier, e.IPAddress);

            if (!string.IsNullOrEmpty(modelFilePath))
            {
                // Flag that the ID of this model came from over the network, we didn't
                // open it ourselves.
                this.ModelIdentifier.AssignExistingFromNetworkedShare(e.ModelIdentifier);

                // Open the model from the file as we would if the user had selected
                // it via a dialog.
                await this.OpenModelFileAsync(modelFilePath);

                // With this model coming from the network, we want to import
                // the anchor onto the parent to put it into the same place within
                // the space as on the device it originated from.
                var worldAnchorBits =
                    await this.FileStorageManager.LoadExportedWorldAnchorAsync();

                if (worldAnchorBits != null)
                {
                    // TODO: if this fails then?...
                    await this.AnchorManager.ImportAnchorToModelParent(worldAnchorBits);
                }
            }
            else
            {
                this.AudioManager.PlayClip(AudioClipType.ErrorDownloadingModel);
                this.HideBusy();
            }
        }
    }
    void ClearExistingModel()
    {
        // Get rid of any manipulations on the model.
        this.ManipulationsManager.RemoveManipulationsFromModel();

        // Get rid of the previous model regardless of whether the user chooses
        // a new one or not with a review to avoiding cluttering the screen.
        this.ModelLoader.DisposeExistingGLTFModel();

        // Get rid of any spatial anchor on the parent so that we can re-add
        // it if necessary when the new model has loaded and the parent has
        // been moved to match the user's gaze.
        this.AnchorManager.RemoveAnchorFromModelParent();

        // Get rid of any identity associated with the model
        this.ModelIdentifier.Clear();
    }
    async Task<LoadedModelInfo> OpenModelFileAsync(string filePath)
    {
        this.ShowBusy("Loading model...");

        LoadedModelInfo modelDetails = null;

        // Load the model.
        try
        {
            modelDetails = await this.ModelLoader.OpenNewModelAsync(filePath);
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

    async Task<string> PickFileFrom3DObjectsFolderAsync()
    {
        var filePath = string.Empty;

#if ENABLE_WINMD_SUPPORT
        var known3DObjectsFolder = KnownFolders.Objects3D.Path.ToLower().TrimEnd('\\');

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

#endif // ENABLE_WINMD_SUPPORT

        return (filePath);
    }
}