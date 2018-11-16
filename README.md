# GLTF-Model-Viewer
A simple viewer for GLTF files on HoloLens, present in the Windows Store [here](https://t.co/giLhjFrQpv).

The viewer is speech driven, the user is prompted to say "open" to select a file which the viewer then presents a few metres in front of the user and they can use hand gestures to scale, rotate, translate the model followed by saying "reset" to put it back where it originally was. They can then use "open" to open some more models.

In terms of what's in this repo - it's not perfect in that it needs a step or two to make it build.

First, clone the repo with --recursive in order to bring down the Mixed Reality Toolkit for Unity as part of the clone along with UnityGLTF.

Then, you need to move into the GLTFModelViewer folder and run;

	setup.bat

It uses relative paths so ensure that it's run from that folder.

There's a challenge in that I want to use the [Mixed Reality Toolkit (MRTK)](https://github.com/Microsoft/MixedRealityToolkit-Unity) and [Unity GLTF](https://github.com/KhronosGroup/UnityGLTF) support but the MRTK has parts of the Unity GLTF support already embedded within it including scripts and binary plugins.

This means that, by default, I get duplication of the UnityGLTF plugins and some of the scripts (both of which are different across MRTK and UnityGLTF).

My solution is to get rid of the GLTF pieces embedded in the MRTK but this does leave a script (MotionControllerVisualizer.cs) in a broken state so I also remove that script and have in my assets a 'dummy' replacement script which avoids the dependency on GLTF which I don't need anyway as I am targeting HoloLens.

Beyond that, the script also compiles the UnityGLTF serialization pieces, tries to put the resulting DLLs into the right places and removes a few pieces which, again, cause me problems & aren't needed here.

The script is quite brittle right now and is likely to break as the underlying frameworks get versioned. It also uses nuget.exe and msbuild so please make sure they are on your path.

Note, at the end of the script it's highly likely that DLL metadata isn't quite right in Unity so you'd need to open up the project in Unity, apply Mixed Reality Toolkit settings to the project (via the menu) and then visit these 2 DLLs.

	Assets\UnityGLTF\Plugins\UWP\GLTFSerializationUWP.dll - should be marked for only the UWP platform & SDK.
	Assets\UnityGLTF\Plugins\UWP\GLTFSeriazliation.dll - should be marked for the editor only.

Note also that at the end of the Unity->Visual Studio->Make Appx Packages step it's possible that you have to rename two or three .png files to make the appx packager happy as, at the time of writing, I haven't quite figured how to get Unity to do what I want there.