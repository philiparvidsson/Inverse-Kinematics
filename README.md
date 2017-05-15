# Experimenting with Inverse Kinematics

This was an assignment for one of my courses during my studies; implementing an IK solver. I took it a few steps further and implemented some physics and a [Phong](https://en.wikipedia.org/wiki/Phong_shading)-shader.

## Building the demo from source

### Prerequisites
* [Mono](http://www.mono-project.com/) — *A software platform designed to allow developers to easily create cross platform applications part of the .NET Foundation.*
* [MonoGame](http://www.monogame.net/) — *An Open Source implementation of the [Microsoft XNA 4 Framework](https://en.wikipedia.org/wiki/Microsoft_XNA).*

### Linux/macOS
1. Clone this repository:  
   ```bash
   git clone https://github.com/philiparvidsson/Inverse-Kinematics
   ```
2. Compile the game:  
   ```bash
   cd Inverse-Kinematics
   make
   ```
3. You should now be able to run it:  
   ```bash
   make run
   ```

## Algorithm
I came up with a pretty simple algorithm for the IK system during the assignment:

```
n ← number of bones
for i from n-1 to 0
    a ← vector from end of segment to beginning of segment
    b ← vector from target to beginning of segment
    
    if length of b  < length of a then
        increase compensating angle for bone i by some amount
        
    relax compensating angle for bone i by some factor
    
    r ← 0
    f ← 1
    for j from i to n-1
        r += f * compensating angle of bone j
        f += 1
        
    rotate bone j around axis (a x b) by some amount minus r
```

Although it is based on classic IK solutions (rotating each bone towards the target iteratively), the concept of compensating angles helps it "back away" to reach nearby (closer to the root point than the length of the sum of all bones in the chain) targets while displaying relatively stable motor skills.

## Video
I implemented a simple 3D environment with some basic physics (a simple a posteriori symplectic Euler solver with a sphere vs. axis-aligned bounding box collision solver) to demonstrate the algorithm. The video below is a recording of the demo (clone this repository to build it yourself).

[![Inverse Kinematics](https://img.youtube.com/vi/1UKI7Xcm4Ow/0.jpg)](https://youtu.be/1UKI7Xcm4Ow)
