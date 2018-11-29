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

#if ENABLE_WINMD_SUPPORT

    async void Start()
    {
        this.webServer = new StorageFolderWebServer(
            KnownFolders.Objects3D,
            this.listenPort);

        await this.webServer.RunAsync(null);
    }
    StorageFolderWebServer webServer;
#endif // ENABLE_WINMD_SUPPORT
}
