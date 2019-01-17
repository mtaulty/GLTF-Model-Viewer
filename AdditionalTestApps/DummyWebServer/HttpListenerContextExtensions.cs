
using System.Linq;
using System.Net;

namespace UwpHttpServer
{
    internal static class HttpListenerContextExtensions
    {
        public static bool VerbIsGet(this HttpListenerContext context) =>
            (string.Compare(context?.Request?.HttpMethod, "get", true) == 0);

        public static bool AcceptsBinary(this HttpListenerContext context)
        {
            var valuesWithoutQuality = context.Request.AcceptTypes.Select(
                acceptType => acceptType.Split(';').First());

            return (
                valuesWithoutQuality.Contains("application/octet-stream") ||
                valuesWithoutQuality.Contains("*/*"));
        }
    }
}
