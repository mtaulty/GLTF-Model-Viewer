using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

#if ENABLE_WINMD_SUPPORT
using Windows.Storage;
using Windows.Web.Http;
#endif // ENABLE_WINMD_SUPPORT

public class HttpModelDownloader : MonoBehaviour
{
    public HttpModelDownloader(
        Guid modelIdentifier,
        IPAddress remoteHostIpAddress,
        int remotePort = 8088)
    {
        this.modelIdentifier = modelIdentifier;
        this.remoteHostIpAddress = remoteHostIpAddress;
        this.remotePort = remotePort;
    }
#pragma warning disable CS1998
    async Task<List<string>> DownloadRemoteUriListForModelAsync()
    {
#if ENABLE_WINMD_SUPPORT
        // We first download the file list which tells us which files make up the model.
        var fileListUri = FileStorageManager.GetFileListRelativeUri(this.modelIdentifier);

        // Get a local storage file within the 3D objects folder to store that into.
        var fileListLocalFile = await FileStorageManager.GetStorageFileForRelativeUriAsync(
            fileListUri);

        // Download the remote file list.
        await DownloadToStorageFileAsync(fileListUri, fileListLocalFile);

        // Read the bill of materials - the files that make up the model.
        var remoteFileUris = await FileIO.ReadLinesAsync(fileListLocalFile);

        return (remoteFileUris.ToList());
#else
        throw new InvalidOperationException();
#endif // ENABLE_WINMD_SUPPORT
    }
    public async Task<string> DownloadModelToLocalStorageAsync()
    {
        var worked = false;
        var mainFilePath = string.Empty;

#if ENABLE_WINMD_SUPPORT
        // Grab the list of Uris/Files that make up the remote model. Note that there
        // is an assumption that the first file here will be the actual GLB/GLTF file
        // otherwise the code below which sets mainFilePath will not work.
        var remoteUrisForModel = await DownloadRemoteUriListForModelAsync();

        if (remoteUrisForModel.Count > 0)
        {
            worked = true;

            // Add into that the URI for the anchor file so that we download this too
            var remoteAnchorFileUri = FileStorageManager.GetAnchorFileRelativeUri(
                this.modelIdentifier);

            remoteUrisForModel.Add(remoteAnchorFileUri);

            // Iterate through and download each one of those files if we need to.
            for (int i = 0; ((i < remoteUrisForModel.Count) && worked); i++)
            {
                // Recurse down & make sure we have a file in the right folder to store
                // this locally.
                var localFile = await FileStorageManager.GetStorageFileForRelativeUriAsync(
                    remoteUrisForModel[i]);   
        
                // Store off the first local file path as we assume that will be the .GLB or
                // GLTF file.
                if (i == 0)
                {
                    mainFilePath = localFile.Path;
                }

                // We want the size of that file.
                var properties = await localFile.GetBasicPropertiesAsync();

                // If the file was already there with > 0 size then we will not overwrite it.
                // (as a cheap form of caching)
                if (properties.Size == 0)
                {
                    // Go grab the file from the remote HoloLens web server and store
                    // the bits locally.
                    worked = await DownloadToStorageFileAsync(remoteUrisForModel[i], localFile);
                }
            }
        }
#endif // ENABLE_WINMD_SUPPORT

        return (worked ? mainFilePath : string.Empty);
    }
#pragma warning restore CS1998

#if ENABLE_WINMD_SUPPORT
    async Task<bool> DownloadToStorageFileAsync(
        string remoteRelativeUri,
        StorageFile localFile)
    {
        var downloaded = false;

        var uri = Uri.EscapeUriString($"{this.BaseUriPath}{remoteRelativeUri}");

        var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Accept.Add(
            new Windows.Web.Http.Headers.HttpMediaTypeWithQualityHeaderValue("application/octet-stream"));

        var response = await httpClient.GetAsync(new Uri(uri));

        if (response.IsSuccessStatusCode)
        {
            using (var fileStream = await localFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                await response.Content.WriteToStreamAsync(fileStream);
                await fileStream.FlushAsync();
                downloaded = true;
            }
        }
        return (downloaded);
    }
#endif // ENABLE_WINMD_SUPPORT

    string BaseUriPath => $"http://{this.remoteHostIpAddress}:{this.remotePort}/";

    Guid modelIdentifier;
    IPAddress remoteHostIpAddress;
    int remotePort;
}