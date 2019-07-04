using Microsoft.MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UwpHttpServer
{
    [CreateAssetMenu(
          menuName = "Mixed Reality Toolkit/Web Server Service Profile",
          fileName = "WebServerServiceProfile")]
    [MixedRealityServiceProfile(typeof(StorageFolderWebServerService))]
    public class StorageFolderWebServerProfile : BaseMixedRealityProfile
    {
        [SerializeField]
        [Tooltip("The port to listen on")]
        public int port = 8088;

        [SerializeField]
        [Tooltip("The UWP folder to offer over HTTP")]
        public StorageFolderType folderToExpose = StorageFolderType.ObjectFolder3D;
    }
}
