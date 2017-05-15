# Experimenting with Inverse Kinematics

This was an assignment for one of my courses during my studies; implementing an IK solver. I took it a few steps further and implemented some physics and a [Phong](https://en.wikipedia.org/wiki/Phong_shading)-shader.

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

Although it is based on classic IK solutions (rotating each bone towards the target iteratively), the concept of compensating angles helps it "back away" to reach nearby targets (the target is closer to the root point than the length of the sum of all bones in the chain) with relatively stable motor skills.

## Video

[![Inverse Kinematics](https://img.youtube.com/vi/1UKI7Xcm4Ow/0.jpg)](https://youtu.be/1UKI7Xcm4Ow)
