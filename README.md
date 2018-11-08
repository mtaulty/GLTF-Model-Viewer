# GLTF-Model-Viewer
A simple viewer for GLTF files on HoloLens

In order to build, first clone the repo with --recursive in order to bring down the Mixed Reality Toolkit for Unity as part of the clone.

Then, you need to move into the GLTFModelViewer folder and run;

	setup.bat

It uses relative paths so ensure that it's run from that folder.

What does that do? 

This project includes both the Mixed Reality Toolkit for Unity and UnityGLTF. 

The script sets up some file system links to try and bring those into my Unity project.

Unfortunately, the Mixed Reality Toolkit for Unity already includes some pieces of UnityGLTF and so I've had to do some moving around to try and make those things co-exist as similar scripts end up being included twice.

Specifically, I have scripts which change the namespace UnityGLTF to KhronosUnityGLTF in the UnityGLTF scripts and I change the namespace AssetGenerator to KhronosAssetGenerator as well.

This isn't nice, it just seemed to be one way of making these things live together. This is done by setup.ps1 which is run by setup.bat