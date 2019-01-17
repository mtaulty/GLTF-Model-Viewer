This 2D UWP solution is a test app which I use as part of development for when I only have 1 HoloLens so this app 'mocks' up the actions of being a 2nd HoloLens in the sense that it;

1) Sits on the network listening for the broadcast messages from another device.

2) Can simulate a "new model" message and respond to a request for that model's files as it is also running the same web server code as on the device.

3) Can record 'transform' messages received from another HoloLens and play them back.

It's a bit poor because it duplicates the networking code at the moment rather than having a shared library between this solution and the main one and I suspect that anyone but me would have trouble getting it working as it's a bit "write once" type code.