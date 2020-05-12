# AStar Implementation-Godot #
An AStar Pathfinding implementation in Godot game engine.

This is an rework of the previous attempt at 3d path finding within the Godot game engine - Version 3.2. 
It utilizes C# over gdscript along with loaded C++ libraries containing AStar pathfinding algorithm. In attempts
to bring a more performant 3d path finding in the Godot game engine.

### Capabilities / Advantages: ###
* Naive Binary Heap - to increase path-node comparison efficiency
* Path Tracking Smoothing - 'natural' like movement tracking
* Naive Obstacle Avoidance - increased obstacle avoidance utilizing weights
* Path Weights - favor paths over 'rough' terrain
* Gradient Slowdown - 'natural' slow down when approaching target
* Path Request Trimming - replaces old requests

### Task List: ###
- [x] Fix path smoothing
- [ ] Add layered map support (under and over - eg. rock cliff in current scene)
- [ ] Add naive form of 'navmeshs' (simplified quadrants to speed up path finding)
- [ ] Fix stuck on empty path bug
- [x] Increased favoring for weights
- [x] Fix slowdown after close distance
- [x] Move path following into seperate script
- [ ] Add Additional Environments
- [ ] Implement Jump Point Search w/ weights
- [ ] Implement Path Request results caching and relavent storing

#### Detailed Description ####
The loaded scene represents three enemies or seekers that will follow the red flag.
Clicking the scene will move the flag, causing them to follow. Thereby, exemplifying
the pathfinding algorithm.

#### Included ####
* Primary Scripts, Assets, Scenes
* Built Executable Project ([here](/_Build/Build_1.0/))


#### Example Pathing ####
![alt text](/Pathing_Example.png)
