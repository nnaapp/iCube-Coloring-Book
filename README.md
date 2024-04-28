This is a WebGL based "coloring book" app, which I wrote for a school internship. 
Credit for the GUI icons goes to Rodney.

This app is fully CPU based, as Unity WebGL does not play nicely with using the GPU.
Every feature is entirely on the CPU except for drawing to the screen, which uses Unity's
GPU functions like blit and such.

Functionality is built in to work with multiple stencils, but currently only one is active,
as I was only provided one. On mid-low modern PC/laptop specs, this should work at 45-60+ FPS, which is about as
good as I could get for CPU based highlighting, drawing, paint bucket, etc.

Any WebGL browser should support this, and a hosting can be found here: https://nnaapp.github.io/iCube-Coloring-Book/

I don't intend to work on this more, but I am happy to answer questions if anyone ever has them.
