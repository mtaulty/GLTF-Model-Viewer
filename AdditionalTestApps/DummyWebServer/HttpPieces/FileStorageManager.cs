using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;

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
}
