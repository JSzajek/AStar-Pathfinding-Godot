# AStar Implementation-Godot #
An AStar Pathfinding implementations in Godot game engine.

This is an rework of the [previous attempt](https://gitlab.com/jszajek/astar-godot) at 3d path finding within the Godot game engine - Version 3.2 mono. 
It utilizes C# over gdscript along with loaded C++ libraries containing AStar pathfinding algorithm. In attempts
to bring a more performant 3d path finding in the Godot game engine. Furthermore, the compiled dlls have been made in such
a way that they can be utilized in other game engines as well (eg. Unity).


**Disclaimer:** All implementations refernce their relevent sources. As well as none of these implementations claim to be the
most efficent implementation - probably more efficent implementations within the engine. 
But these implementations are done in an exercise to learn about the involved algorithms. 

Image utilized for background in menu screen can be found [here](https://www.clipart.email/download/4510544.html)

### Capabilities / Advantages: ###
* Naive Binary Heap - to increase path-node comparison efficiency
* Path Tracking Smoothing - 'natural' like movement tracking
* Naive Obstacle Avoidance - increased obstacle avoidance utilizing weights
* Path Weights - favor paths over 'rough' terrain
* Gradient Slowdown - 'natural' slow down when approaching target
* Path Request Trimming - replaces old requests

### Task List: ###
- [x] Fix path smoothing
- [ ] Fix Memory leaks in dlls
- [ ] Add layered map support (under and over - eg. rock cliff in grid scene)
- [x] Add naive form of 'navmeshs' ~~(simplified quadrants to speed up path finding)~~
- [ ] Add weighting into navmesh pathfinding
- [ ] Fix stuck on empty path bug
- [ ] Look into adding boids behavior ([outlined here](https://cs.stanford.edu/people/eroberts/courses/soco/projects/2008-09/modeling-natural-systems/boids.html))
- [x] Increased favoring for weights
- [x] Fix slowdown after close distance
- [x] Move path following into seperate script
- [x] Add Additional Environments (accomplished with other implementations?)
- [ ] Implement Jump Point Search w/ weights
- [ ] Implement Path Request results caching and relavent storing

#### Detailed Description ####
The implementations scenes represents three enemies or seekers that will follow the red flag.
Clicking the scene will move the flag, causing them to follow. Thereby, exemplifying
the pathfinding algorithm.

#### Included ####
* Primary Scripts, Assets, Imports, Scenes
* Built Executable Project ([here](/_Build/Build_1.2/))


### Pathfinding Implementations ###

#### AStar with Grid ####
This implementation utilizes a basic 2 dimensional grid of path nodes to perform A* pathfinding. This attempt was
heavily based on this [tutorial](https://www.youtube.com/playlist?list=PLFt_AvWsXl0cq5Umv3pMC9SPnKjfp9eGW).

![alt text](/AStar_Grid_Example.png)

#### AStar with KDTree ####
This implementation utilizes a K-Dimensional Tree ([more information here](https://en.wikipedia.org/wiki/K-d_tree)).
This was done in an attempt to have a more efficient method of adding sections of the map and potentially removing chunks
for games with procedural map generation or just memory conservation by removing unlikely chunks. Since the KD tree 
represents an average search time of O(logn) as well as removal of the same time. This is better than potentially
recompiling the grid within the previous approach. But mostly this attempt was an attempt at getting easily layered
astar pathfinding without the need to implement complex 3 dimensional arrays.

![alt text](/AStar_KDTree_Example.png)

##### Issues #####
This implementation also presented significant issues. From my initial testing I believe it is due to the nature of 
axis splitting within the KD tree or may be an issue within my current KD tree implementation. But from
quick research it seems that KD trees don't always offer the ability to gather the 'exact' nearest neighbors
required for astar. Thus resulting in missing or enlongated routes. Nonetheless more testing is required - 
which may be unnecessary given the performance of the next implementation.

##### Nearest Neighbors Issue: #####
![alt text](/KDTree_Issue.gif)

As seen with the above gif, the red cube representing the nearest point to the clicked point and the 
blue cubes representing the nearest neighbors to that point (within a radius search of the node diameters).
It achieves great results until we reach a partition zone, which leads to a dead zone. Resulting in 
enlongated paths show below:

![alt text](/AStar_KDTree_Issue.png)


#### Navmesh Implementation ####
This is an early implementation of a navmesh (triangulation still has problems). But it reduces the amount of checks
within the pathfinding - thus finding efficent paths along the vertices, midpoints, and centriods of a triangulated
polygon depicted in red, blue, and green respectively. There still needs to be path simplifing and weighting within
the graph to produce better paths.

![alt text](/AStar_Navmesh_Example.png)
