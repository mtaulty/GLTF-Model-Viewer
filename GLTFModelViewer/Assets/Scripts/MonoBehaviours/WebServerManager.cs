using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

#if ENABLE_WINMD_SUPPORT
using UwpHttpServer;
using Windows.Storage;
#endif 

public class WebServerManager : MonoBehaviour
{
    [SerializeField]
    int listenPort = 8088;

    public int ListenPort => this.listenPort;

    // Note - this defines a dependency such that before we hit Start() here we need the
    // IPAddressProvider component to be instantiated.
    async void Start()
    {
#if ENABLE_WINMD_SUPPORT
        if (IPAddressProvider.HasIpAddress)
        {
            this.webServer = new StorageFolderWebServer(
                KnownFolders.Objects3D,
                this.listenPort);

            await this.webServer.RunAsync(null);
        }
#endif // ENABLE_WINMD_SUPPORT
    }
#if ENABLE_WINMD_SUPPORT
    StorageFolderWebServer webServer;
#endif // ENABLE_WINMD_SUPPORT
}
