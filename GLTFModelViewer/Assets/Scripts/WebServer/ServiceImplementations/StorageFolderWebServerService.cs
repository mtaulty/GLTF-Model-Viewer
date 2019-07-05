using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

#if ENABLE_WINMD_SUPPORT
using Windows.Storage;
using Windows.Storage.Streams;
#endif // ENABLE_WINMD_SUPPORT

namespace UwpHttpServer
{
    /// <summary>
    /// Note, I don't want/expect this to run in the editor and I'm trying to turn it off here
    /// but I suspect the framework will still start it so...
    /// </summary>
    [MixedRealityExtensionService(SupportedPlatforms.WindowsUniversal)]
    public class StorageFolderWebServerService : BaseExtensionService, IStorageFolderWebServerService
    {
        public StorageFolderWebServerService(
            IMixedRealityServiceRegistrar registrar, 
            string name = null, 
            uint priority = 10, 
            BaseMixedRealityProfile profile = null) : base(registrar, name, priority, profile)
        {

        }
        StorageFolderWebServerProfile Profile => base.ConfigurationProfile as StorageFolderWebServerProfile;

#pragma warning disable CS1998
        public async Task RunAsync(CancellationToken? cancelToken = null)
        {
#if ENABLE_WINMD_SUPPORT
            this.folderPath = MapPath(this.Profile.folderToExpose);

            var httpListener = new HttpListener();
            httpListener.Prefixes.Add($"http://*:{this.Profile.port}/");
            httpListener.Start();

            var contextConstraints = new HttpListenerContextConstraintList();

            contextConstraints.Add(
                ctx => ctx.VerbIsGet(), HttpStatusCode.MethodNotAllowed);

            contextConstraints.Add(
                ctx => ctx.AcceptsBinary(), HttpStatusCode.NotAcceptable);

            try
            {
                while (true)
                {
                    cancelToken?.ThrowIfCancellationRequested();

                    // This method doesn't take a cancellation token so not sure what
                    // I can do really.
                    var context = await httpListener.GetContextAsync();

                    context.Response.StatusCode = (int)contextConstraints.CheckConstraints(context);

                    if (context.Response.StatusCode == (int)HttpStatusCode.OK)
                    {
                        await this.ServeFile(context);
                    }
                    context.Response.Close();
                }
            }
            finally
            {
                httpListener.Stop();
                httpListener.Close();
            }
#endif // ENABLE_WINMD_SUPPORT
        }
#pragma warning restore CS1998

#if ENABLE_WINMD_SUPPORT
        async Task ServeFile(HttpListenerContext context)
        {
            // Note: context.Response.StatusCode (by definition at the time of writing)
            // comes into this function set to 200/OK.
            var decodedUrl = Uri.UnescapeDataString(context.Request.RawUrl);

            var requestPath = decodedUrl.TrimStart('/').Replace('/','\\');

            var fullPath = Path.Combine(this.folderPath, requestPath);

            try
            {
                var file = await StorageFile.GetFileFromPathAsync(fullPath);

                var properties = await file.GetBasicPropertiesAsync();

                using (var fileStream = await file.OpenSequentialReadAsync())
                {
                    context.Response.ContentType = "application/octet-stream";
                    context.Response.AddHeader("Content-Disposition", $"attachment; filename=\"{file.Name}\"");
                    context.Response.ContentLength64 = (long)properties.Size;

                    await RandomAccessStream.CopyAsync(fileStream,
                        context.Response.OutputStream.AsOutputStream());

                    await context.Response.OutputStream.FlushAsync();
                }
            }
            catch
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }
        static string MapPath(StorageFolderType folderType)
        {
            return (folderMap[folderType].Path);
        }
        static Dictionary<StorageFolderType, StorageFolder> folderMap = new Dictionary<StorageFolderType, StorageFolder>()
        {
            { StorageFolderType.ObjectFolder3D , KnownFolders.Objects3D }
        };
#endif // ENABLE_WINMD_SUPPORT
        string folderPath;
    }
}
