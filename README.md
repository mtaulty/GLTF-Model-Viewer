# GLTF-Model-Viewer
A simple viewer for GLTF files on HoloLens, present in the Windows Store [here](https://t.co/giLhjFrQpv).

## The App
The viewer is speech driven, the user is prompted to say "open" to select a file from their 3D objects folder which the viewer then presents a few metres in front of the user and they can use hand gestures to scale, rotate, translate the model followed by saying "reset" to put it back where it originally was. They can then use "open" to open some more models.

The app also has a basic 'shared holographic' (or 'Multi-Lens') capability. Whenever a user opens a model on one device, the app multicasts around the network and other devices will display a "would you like to open this model" dialog.

If the secondary user says "yes" then their device will use HTTP to talk to a web server running on the first device to request all the files that make up the model which will then be copied to the secondary device and the model will then be opened from there. A spatial anchor will also be exported, requested and imported on the secondary device such that the object will appear in the same place in the physical world as it was on the originating device.

Lastly, from hereon in, any movements on the originating device will be replicated to the secondary device(s) until a device breaks the link by using the "open" command again or perhaps accepting a model from another device.

This allows for a shared holographic experience without having any form of local or cloud server to provide infrastructure.