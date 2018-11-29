using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

#if ENABLE_WINMD_SUPPORT
using Windows.Storage;
#endif // ENABLE_WINMD_SUPPORT

public class FileStorageManager : MonoBehaviour
{
    ModelIdentifier ModelIdentifier => this.gameObject.GetComponent<ModelIdentifier>();

    static readonly string SUBFOLDER_PATH = "gltfViewer";
    static readonly string FILE_LIST_FILE_EXTENSION = ".fil";
    static readonly string FILE_ANCHOR_FILE_EXTENSION = ".anc";

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

        var fileName = $"{this.ModelIdentifier.Identifier}{FILE_LIST_FILE_EXTENSION}";

        var file = await CreateSubFolderFileAsync(fileName);

        await FileIO.WriteLinesAsync(file, relativePaths);

#endif // ENABLE_WINMD_SUPPORT
    }
    public async Task StoreExportedWorldAnchorAsync(byte[] worldAnchorBits)
    {
#if ENABLE_WINMD_SUPPORT
        var fileName = $"{this.ModelIdentifier.Identifier}{FILE_ANCHOR_FILE_EXTENSION}";

        var file = await CreateSubFolderFileAsync(fileName);

        await FileIO.WriteBytesAsync(file, worldAnchorBits);

#endif // ENABLE_WINMD_SUPPORT
    }    
#if ENABLE_WINMD_SUPPORT
    static async Task<StorageFile> CreateSubFolderFileAsync(string fileName)
    {
        var parentFolder = KnownFolders.Objects3D;

        var subFolder = await parentFolder.CreateFolderAsync(SUBFOLDER_PATH, CreationCollisionOption.OpenIfExists);

        var storageFile = await subFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

        return (storageFile);
    }    
#endif // ENABLE_WINMD_SUPPORT
}
