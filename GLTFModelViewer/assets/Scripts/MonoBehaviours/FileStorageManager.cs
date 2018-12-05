using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

#if ENABLE_WINMD_SUPPORT
using Windows.Storage;
using System.Runtime.InteropServices.WindowsRuntime;
#endif // ENABLE_WINMD_SUPPORT

public class FileStorageManager : MonoBehaviour
{
    ModelIdentifier ModelIdentifier => this.gameObject.GetComponent<ModelIdentifier>();

    static readonly string SUBFOLDER_PATH = "gltfViewer";
    static readonly string FILE_LIST_FILE_EXTENSION = ".fil";
    static readonly string FILE_ANCHOR_FILE_EXTENSION = ".anc";

    string WorldAnchorFileName =>
        $"{this.ModelIdentifier.Identifier}{FILE_ANCHOR_FILE_EXTENSION}";

    string FileListFileName =>
        $"{this.ModelIdentifier.Identifier}{FILE_LIST_FILE_EXTENSION}";

    string AppSubFolderName => SUBFOLDER_PATH;

    public string GetFileListRelativeUri(Guid modelIdentifier)
        => $"{this.AppSubFolderName}/{modelIdentifier}{FILE_LIST_FILE_EXTENSION}";

    public string GetAnchorFileRelativeUri(Guid modelIdentifier) => 
        $"{this.AppSubFolderName}/{modelIdentifier}{FILE_ANCHOR_FILE_EXTENSION}";

    public async Task StoreFileListAsync(RecordingFileLoader fileRecorder)
    {
#if ENABLE_WINMD_SUPPORT
        var baseLoadPath = fileRecorder.BaseDirectoryPath.ToLower().TrimEnd('\\');
        var base3dObjectsPath = KnownFolders.Objects3D.Path.ToLower().TrimEnd('\\');

        var relativePath = baseLoadPath.Substring(
            base3dObjectsPath.Length,
            baseLoadPath.Length - base3dObjectsPath.Length);

        var relativePaths = fileRecorder.RelativeLoadedFilePaths.Select(
            path => Path.Combine("\\", relativePath, path).Replace('\\', '/'));

        var file = await CreateSubFolderFileAsync(this.FileListFileName);

        await FileIO.WriteLinesAsync(file, relativePaths);

#endif // ENABLE_WINMD_SUPPORT
    }
    public async Task StoreExportedWorldAnchorAsync(byte[] worldAnchorBits)
    {
#if ENABLE_WINMD_SUPPORT
        var file = await CreateSubFolderFileAsync(this.WorldAnchorFileName);

        await FileIO.WriteBytesAsync(file, worldAnchorBits);

#endif // ENABLE_WINMD_SUPPORT
    }    
    public async Task<byte[]> LoadExportedWorldAnchorAsync()
    {
        byte[] bits = null;

#if ENABLE_WINMD_SUPPORT
        
        var anchorFile = await KnownFolders.Objects3D.GetFileAsync(this.WorldAnchorFileName);

        var buffer = await FileIO.ReadBufferAsync(anchorFile);

        bits = buffer?.ToArray();

#endif // ENABLE_WINMD_SUPPORT

        return (bits);
    }
#if ENABLE_WINMD_SUPPORT
    async Task<StorageFile> CreateSubFolderFileAsync(string fileName)
    {
        var parentFolder = KnownFolders.Objects3D;

        var subFolder = await parentFolder.CreateFolderAsync(
            this.AppSubFolderName, CreationCollisionOption.OpenIfExists);

        var storageFile = await subFolder.CreateFileAsync(
            fileName, CreationCollisionOption.ReplaceExisting);

        return (storageFile);
    }    
    public async Task<StorageFile> GetStorageFileForRelativeUriAsync(string relativeUri)
    {
        var topFolder = KnownFolders.Objects3D;

        var pathPieces =
            relativeUri.Split('/').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToArray();

        // We have folders
        for (int i = 0; i < pathPieces.Length - 1; i++)
        {
            topFolder = 
                await topFolder.CreateFolderAsync(pathPieces[i], CreationCollisionOption.OpenIfExists);
        }
        var file = await topFolder.CreateFileAsync(pathPieces[pathPieces.Length - 1],
            CreationCollisionOption.OpenIfExists);

        return (file);
    }
#endif // ENABLE_WINMD_SUPPORT
}
