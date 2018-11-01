# GLTF-Model-Viewer
A simple viewer for GLTF files on HoloLens

In order to build, first clone the repo with --recursive in order to bring down the Mixed Reality Toolkit for Unity as part of the clone.

Then, you need to set up a file link using mklink from the GLTFModelViewer\Assets\HoloToolkit folder to the MixedRealityToolkit-Unity\Assets\HoloToolkit folder.

A command something like;

	mklink /j .\GLTFModelViewer\Assets\HoloToolkit .\MixedRealityToolkit-Unity\Assets\HoloToolkit

then open in Unity and build.
