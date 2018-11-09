rem this script is likely to break over time :-(

rem link the HoloToolkit into our project.

mklink /j GLTFModelViewer\Assets\HoloToolkit .\MixedRealityToolkit-Unity\Assets\HoloToolkit

rem get rid of the GLTF embedded into the toolkit.

rmdir /s /q .\MixedRealityToolkit-Unity\Assets\HoloToolkit\Utilities\Scripts\GLTF\

rem get rid of files which will now cause a problem because the GLTF serializer that they are
rem dependent upon has changed since they took their dependency.

del .\MixedRealityToolkit-Unity\Assets\HoloToolkit\Input\Scripts\Utilities\MotionControllerVisualizer.cs

rem build the GLTF serializer.
rem apologies you are going to need to install Nuget.exe here as one project here uses packages.config
rem and I do not know how to restore that with msbuild (seems to say it cannot).

msbuild /target:restore .\UnityGLTF\GLTFSerialization\GLTFSerialization.sln
nuget restore .\UnityGLTF\GLTFSerialization\GLTFSerialization.sln

msbuild /target:GLTFSerializationUWP /p:Platform="x86" /p:Configuration="Release" .\UnityGLTF\GLTFSerialization\GLTFSerialization.sln

msbuild /target:GLTFSerialization /p:Platform="Any CPU" /p:Configuration="Release" .\UnityGLTF\GLTFSerialization\GLTFSerialization.sln

rem copy the outputs out of there into the UnityGLTF project structure.

copy /Y .\UnityGLTF\GLTFSerialization\GLTFSerialization\obj\release\*.dll .\UnityGLTF\UnityGLTF\Assets\UnityGLTF\plugins\
copy /Y .\UnityGLTF\GLTFSerialization\GLTFSerializationUWP\obj\Release\*.dll .\UnityGLTF\UnityGLTF\Assets\UnityGLTF\plugins\UWP\

rem now link that into the right place under our project structure.

rem no longer need this next step I hope
rem mkdir GLTFModelViewer\Assets\UnityGLTF

rem bring in the pieces of UnityGLTF that I think I need.
mklink /j GLTFModelViewer\Assets\Resources\UnityGLTF .\UnityGLTF\UnityGLTF\Assets\Resources
mklink /j GLTFModelViewer\Assets\UnityGLTF\Scripts .\UnityGLTF\UnityGLTF\Assets\UnityGLTF\Scripts
mklink /j GLTFModelViewer\Assets\UnityGLTF\Plugins .\UnityGLTF\UnityGLTF\Assets\UnityGLTF\Plugins
mklink /j GLTFModelViewer\Assets\UnityGLTF\Shaders .\UnityGLTF\UnityGLTF\Assets\UnityGLTF\Shaders

rem get rid of the pieces of UnityGLTF that I do not think I need
del GLTFModelViewer\Assets\Resources\UnityGLTF\*.cs
del GLTFModelViewer\Assets\Resources\UnityGLTF\*.prefab

rem try to get hold of the right JSON library, this is horribly brittle right now
copy /Y ".\UnityGLTF\GLTFSerialization\packages\Newtonsoft.Json.9.0.1\lib\portable-net40+sl5+wp80+win8+wpa81\Newtonsoft.Json.dll" .\GLTFModelViewer\Assets\UnityGLTF\plugins\

