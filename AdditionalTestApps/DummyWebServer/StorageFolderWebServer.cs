using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Streams;

namespace UwpHttpServer
{
    public class StorageFolderWebServer
    {
        public StorageFolderWebServer(StorageFolder folder, int port=8088)
        {
            this.port = port;
            this.folder = folder;
        }
        public async Task RunAsync(CancellationToken? cancelToken = null)
        {
            var httpListener = new HttpListener();
            httpListener.Prefixes.Add($"http://*:{this.port}/");
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
        }
        async Task ServeFile(HttpListenerContext context)
        {
            // Note: context.Response.StatusCode (by definition at the time of writing)
            // comes into this function set to 200/OK.
            var requestPath = context.Request.RawUrl.TrimStart('/').Replace('/','\\');

            var fullPath = Path.Combine(this.folder.Path, requestPath);

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

        StorageFolder folder;
        int port;
    }
}
