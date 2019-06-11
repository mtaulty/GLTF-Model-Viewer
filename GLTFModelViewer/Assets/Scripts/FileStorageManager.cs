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

public static class FileStorageManager
{
    static readonly string SUBFOLDER_PATH = "gltfViewer";
    static readonly string FILE_LIST_FILE_EXTENSION = ".fil";
    static readonly string FILE_ANCHOR_FILE_EXTENSION = ".anc";

    static string AppSubFolderName => SUBFOLDER_PATH;

    static string GetWorldAnchorFileName(Guid modelIdentifier) =>
        $"{modelIdentifier}{FILE_ANCHOR_FILE_EXTENSION}";

    static string GetFileListFileName(Guid modelIdentifier) =>
        $"{modelIdentifier}{FILE_LIST_FILE_EXTENSION}";

    public static string GetFileListRelativeUri(Guid modelIdentifier)
        => $"{AppSubFolderName}/{modelIdentifier}{FILE_LIST_FILE_EXTENSION}";

    public static string GetAnchorFileRelativeUri(Guid modelIdentifier) => 
        $"{AppSubFolderName}/{modelIdentifier}{FILE_ANCHOR_FILE_EXTENSION}";

    public static async Task StoreFileListAsync(Guid modelIdentifier,
        ImportedModelInfo importedModelInfo)
    {
        // TODO: Need to figure out this use of the fileRecorder, think it's 
        // perhaps just needing the ImportedModelInfo info to complete
        // the work here. 17th May 2019.
#if ENABLE_WINMD_SUPPORT
        var baseLoadPath = importedModelInfo.BaseDirectoryPath.ToLower().TrimEnd('\\');
        var base3dObjectsPath = KnownFolders.Objects3D.Path.ToLower().TrimEnd('\\');

        var relativePath = baseLoadPath.Substring(
            base3dObjectsPath.Length,
            baseLoadPath.Length - base3dObjectsPath.Length);

        var relativePaths = importedModelInfo.RelativeLoadedFilePaths.Select(
            path => Path.Combine("\\", relativePath, path).Replace('\\', '/'));

        var file = await GetSubFolderFileAsync(
            GetFileListFileName(modelIdentifier), true);

        await FileIO.WriteLinesAsync(file, relativePaths);

#endif // ENABLE_WINMD_SUPPORT
    }
    public static async Task StoreExportedWorldAnchorAsync(Guid modelIdentifier, byte[] worldAnchorBits)
    {
#if ENABLE_WINMD_SUPPORT
        var file = await GetSubFolderFileAsync(
            GetWorldAnchorFileName(modelIdentifier), true);

        await FileIO.WriteBytesAsync(file, worldAnchorBits);

#endif // ENABLE_WINMD_SUPPORT
    }    
    public static async Task<byte[]> LoadExportedWorldAnchorAsync(Guid modelIdentifier)
    {
        byte[] bits = null;

#if ENABLE_WINMD_SUPPORT
        var anchorFile = await GetSubFolderFileAsync(
            GetWorldAnchorFileName(modelIdentifier), false);

        var buffer = await FileIO.ReadBufferAsync(anchorFile);

        bits = buffer?.ToArray();

#endif // ENABLE_WINMD_SUPPORT

        return (bits);
    }
#if ENABLE_WINMD_SUPPORT   
    public static async Task<StorageFile> GetStorageFileForRelativeUriAsync(string relativeUri)
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
    static async Task<StorageFile> GetSubFolderFileAsync(string fileName, bool shouldCreateFile)
    {
        var parentFolder = KnownFolders.Objects3D;

        var subFolder = await parentFolder.CreateFolderAsync(
            AppSubFolderName, CreationCollisionOption.OpenIfExists);

        var openFlags = 
            shouldCreateFile ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.OpenIfExists;

        var storageFile = await subFolder.CreateFileAsync(fileName, openFlags);

        return (storageFile);
    } 
#endif // ENABLE_WINMD_SUPPORT
}
