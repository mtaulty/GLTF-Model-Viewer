using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityGLTF.Loader;
using System.Threading.Tasks;

#if ENABLE_WINMD_SUPPORT
using Windows.Storage;
using Windows.Storage.Streams;
#endif // ENABLE_WINMD_SUPPORT

public class RecordingFileLoader : ILoader
{
    /// <summary>
    /// Note: this class used to delegate down to the FileLoader which is part of the
    /// UnityGLTF code that that class used calls like File.Exists() and File.OpenRead()
    /// and I found that the behaviour of those calls changes across SDKs < 16299 and 
    /// SDKS > 16299 and so I effectively encapsulated a *version* of that FileLoader
    /// class into this class when I moved up to SDK 17763 and took away any calls
    /// to File.*
    /// </summary>
    /// <param name="rootDirectoryPath"></param>
    public RecordingFileLoader(string rootDirectoryPath) 
    {
        this.BaseDirectoryPath = rootDirectoryPath;
        this.relativeLoadedFilePaths = new List<string>();
    }
    public string BaseDirectoryPath { get; private set; }

    public IReadOnlyList<string> RelativeLoadedFilePaths => this.relativeLoadedFilePaths.AsReadOnly();

    public Stream LoadedStream { get; private set; } 

    public bool HasSyncLoadMethod => false;

    public IEnumerator LoadStream(string gltfFilePath)
    {
        if (gltfFilePath == null)
        {
            throw new ArgumentNullException("gltfFilePath");
        }
        if (!this.relativeLoadedFilePaths.Contains(gltfFilePath))
        {
            this.relativeLoadedFilePaths.Add(gltfFilePath);
        }    
        yield return LoadFileStream(gltfFilePath);
    }
    IEnumerator LoadFileStream(string fileToLoad)
    {
	// This isn't "very nice" code but I found myself a little stuck between a rock
	// and a hard place in that the base class wants an enumerator and the APIs
	// I can use are async so...I do what I've done here.
#if ENABLE_WINMD_SUPPORT
        var task = Task.Run(
            async () =>
            {        
                string pathToLoad = Path.Combine(this.BaseDirectoryPath, fileToLoad);

                var file = await StorageFile.GetFileFromPathAsync(pathToLoad);

                var fileStream = await file.OpenReadAsync();

                this.LoadedStream = fileStream.AsStreamForRead();
            }
        );
        while (!task.IsCompleted)
        {
            yield return null;
        } 
#else
        yield return null;
#endif // ENABLE_WINMD_SUPPORT
    }
    public void LoadStreamSync(string jsonFilePath)
    {
        // Hoping to avoid the 'sync' path here given we return false to the
        // HasSyncLoadMethod.
        throw new NotImplementedException();
    }
    List<string> relativeLoadedFilePaths;
}