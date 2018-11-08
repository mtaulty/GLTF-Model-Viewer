rem this script is likely to break over time :-(
mklink /j GLTFModelViewer\Assets\HoloToolkit .\MixedRealityToolkit-Unity\Assets\HoloToolkit

rem apologies you are going to need to install Nuget.exe here as one project here uses packages.config
rem and I do not know how to restore that with msbuild (seems to say it cannot).
cd .\UnityGLTF\GLTFSerialization\GLTFSerialization\
nuget.exe restore -PackagesDirectory .\
cd ..\..\..\

msbuild /target:restore .\UnityGLTF\GLTFSerialization\GLTFSerialization.sln

msbuild /target:GLTFSerializationUWP /p:Platform="x86" /p:Configuration="Release" .\UnityGLTF\GLTFSerialization\GLTFSerialization.sln

msbuild /target:GLTFSerialization /p:Platform="Any CPU" /p:Configuration="Release" .\UnityGLTF\GLTFSerialization\GLTFSerialization.sln

copy .\UnityGLTF\GLTFSerialization\GLTFSerialization\obj\release\*.dll .\UnityGLTF\UnityGLTF\Assets\UnityGLTF\plugins\
copy .\UnityGLTF\GLTFSerialization\GLTFSerializationUWP\obj\Release\*.dll .\UnityGLTF\UnityGLTF\Assets\UnityGLTF\plugins\UWP\

mkdir GLTFModelViewer\Assets\UnityGLTF
mklink /j GLTFModelViewer\Assets\UnityGLTF\Resources .\UnityGLTF\UnityGLTF\Assets\UnityGLTF\Resources
mklink /j GLTFModelViewer\Assets\UnityGLTF\Scripts .\UnityGLTF\UnityGLTF\Assets\UnityGLTF\Scripts
mklink /j GLTFModelViewer\Assets\UnityGLTF\Plugins .\UnityGLTF\UnityGLTF\Assets\UnityGLTF\Plugins

