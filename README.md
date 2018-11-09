# GLTF-Model-Viewer
A simple viewer for GLTF files on HoloLens

In order to build, first clone the repo with --recursive in order to bring down the Mixed Reality Toolkit for Unity as part of the clone along with UnityGLTF.

Then, you need to move into the GLTFModelViewer folder and run;

	setup.bat

It uses relative paths so ensure that it's run from that folder.

There's a challenge in this project in that I want to use the Mixed Reality Toolkit (MRTK) and Unity GLTF support but the MRTK has parts of the Unity GLTF support already embedded within it including scripts and binary plugins.

This means that, by default, I get duplication of the UnityGLTF plugins and some of the scripts (both of which are different across MRTK and UnityGLTF).

A solution (which I applied) is to get rid of the GLTF pieces embedded in the MRTK but this does leave a script (MotionControllerVisualizer.cs) in a broken state so I also remove that script and have in my assets a 'dummy' replacement script which avoids the dependency on GLTF which I don't need anyway as I am targeting HoloLens.

Beyond that, the script also compiles the UnityGLTF serialization pieces, tries to put the resulting DLLs into the right places and removes a few pieces which, again, cause me problems & aren't needed here.

The script is quite brittle right now. It'd break as people version the UnityGLTF pieces and so any improvements to it would be welcome. It also uses nuget.exe and msbuild so please make sure they are on your path.

Note, at the end of the script it's highly likely that DLL metadata isn't quite right in Unity so you'd need to open up the project in Unity, apply Mixed Reality Toolkit settings to the project (via the menu) and then visit these 2 DLLs.

	Assets\UnityGLTF\Plugins\UWP\GLTFSerializationUWP.dll - should be marked for only the UWP platform & SDK.
	Assets\UnityGLTF\Plugins\UWP\GLTFSeriazliation.dll - should be marked for the editor only.








