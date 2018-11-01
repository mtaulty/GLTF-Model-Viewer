msbuild /t:restore UnityGLTF\GLTFSerialization\GLTFSerializationUWP\GLTFSerializationUWP.csproj

msbuild /p:Configuration=Release UnityGLTF\GLTFSerialization\GLTFSerializationUWP\GLTFSerializationUWP.csproj

mklink /j GLTFModelViewer\Assets\HoloToolkit .\MixedRealityToolkit-Unity\Assets\HoloToolkit