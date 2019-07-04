using Microsoft.MixedReality.Toolkit;
using System.Threading;
using System.Threading.Tasks;

namespace UwpHttpServer
{
    public interface IStorageFolderWebServerService : IMixedRealityExtensionService
    {
        Task RunAsync(CancellationToken? cancelToken = null);
    }
}
