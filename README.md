# GLTF-Model-Viewer
A simple viewer for GLTF files on HoloLens

In order to build, first clone the repo with --recursive in order to bring down the Mixed Reality Toolkit for Unity as part of the clone along with UnityGLTF.

Then, you need to move into the GLTFModelViewer folder and run;

	setup.bat

It uses relative paths so ensure that it's run from that folder.

There's a challenge in this project in that I want to use the Mixed Reality Toolkit (MRTK) and Unity GLTF support but the MRTK has parts of the Unity GLTF support already embedded within it including scripts and binary plugins.

This means that, by default, I get duplication of the UnityGLTF plugins and some of the scripts (both of which are different across MRTK and UnityGLTF).

A solution is to get rid of those scripts/plugins from the MRTK but it leaves a dependency in the script;

	MotionControllerVisualizer.cs

which isn't relevant on the HoloLens that I am targeting. So, I replace this script in the toolkit by deleting the one which ships in the Toolkit and putting back a version of my own which takes out the dependencies on GLTF.






