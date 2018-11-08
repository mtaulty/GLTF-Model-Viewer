mklink /j GLTFModelViewer\Assets\HoloToolkit .\MixedRealityToolkit-Unity\Assets\HoloToolkit

msbuild /target:restore .\UnityGLTF\GLTFSerialization\GLTFSerialization.sln
msbuild /target:GLTFSerializationUWP /p:Platform="x86" /p:Configuration="Release" .\UnityGLTF\GLTFSerialization\GLTFSerialization.sln

copy .\UnityGLTF\GLTFSerialization\GLTFSerializationUWP\obj\Release\*.dll .\UnityGLTF\UnityGLTF\Assets\UnityGLTF\plugins

mkdir GLTFModelViewer\Assets\UnityGLTF
mklink /j GLTFModelViewer\Assets\UnityGLTF\Resources .\UnityGLTF\UnityGLTF\Assets\UnityGLTF\Resources
mklink /j GLTFModelViewer\Assets\UnityGLTF\Scripts .\UnityGLTF\UnityGLTF\Assets\UnityGLTF\Scripts
mklink /j GLTFModelViewer\Assets\UnityGLTF\Plugins .\UnityGLTF\UnityGLTF\Assets\UnityGLTF\Plugins
Powershell.exe -File setup.ps1
