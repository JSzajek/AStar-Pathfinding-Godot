#pragma once

#ifdef NATIVEASTAR_H
#define NATIVEASTAR_H __declspec(dllexport)
#else
#define NATIVEASTAR_H __declspec(dllimport)

// Initialize the native class.
extern "C" NATIVEASTAR_H void native_lib_init();

// Destructor for the native class.
extern "C" NATIVEASTAR_H void native_lib_destroy();

// Sets up the relevent parameters involved in the astar search
extern "C" NATIVEASTAR_H void setup(float _nodeRadius, int _minPenalty, int _maxPenalty);

// Clears the points in the current grid
extern "C" NATIVEASTAR_H void clearPoints();

// Adds the passed point into the astar grid
extern "C" NATIVEASTAR_H void addPoint(float pointX, float pointY, float pointZ, bool walkable, int movePenalty);

// Adds the points structured in an float array into the astar grid
extern "C" NATIVEASTAR_H void addPoints(float* points, int d1, int d2);

// Removes the point from the astar grid
extern "C" NATIVEASTAR_H void removePoint(float pointX, float pointY, float pointZ);

// Removes the points from the astar grid
extern "C" NATIVEASTAR_H void removePoints(float* points, int d1, int d2);

// Gets the point closest to the passed point
extern "C" NATIVEASTAR_H float* getPoint(float pointX, float pointY, float pointZ);

// Gets the nearest neighbors of the passed point
extern "C" NATIVEASTAR_H float* getNearestNeighbors(float pointX, float pointY, float pointZ);

// Gets the astar shortest path from the passed start and end point (with smoothing options)
extern "C" NATIVEASTAR_H float* path(float startX, float startY, float startZ, float endX, float endY, float endZ, bool smooth, float turnDist, float stopDist);

// Grid blurring not included in KD tree implementation
//extern "C" NATIVEASTAR_H int* blur(int blurSize);

#endif