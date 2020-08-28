#pragma once

#ifdef NATIVEASTAR_H
#define NATIVEASTAR_H __declspec(dllexport)
#else
#define NATIVEASTAR_H __declspec(dllimport)

#include "Linker.h"

// Initialize the native class.
extern "C" NATIVEASTAR_H void native_lib_init();

// Destructor for the native class.
extern "C" NATIVEASTAR_H void native_lib_destroy();

extern "C" NATIVEASTAR_H void ReleaseMemory(void* arrayPtr);

// Sets up the relevent parameters involved in the astar search - Tree
extern "C" NATIVEASTAR_H void setup(float _nodeRadius, int _minPenalty, int _maxPenalty);

// Sets up the relevent parameters involved in the astar search - Grid
extern "C" NATIVEASTAR_H void setupGrid(int gridSizeX, int gridSizeY, int _minPenalty, int _maxPenalty, float offsetX, float offsetY, float offsetZ);

// Clears the points in the current grid
extern "C" NATIVEASTAR_H void clearPoints();

// Adds the passed point into the astar tree
extern "C" NATIVEASTAR_H void addPoint(float pointX, float pointY, float pointZ, bool walkable, int movePenalty);

// Adds the passed point into the astar grid
extern "C" NATIVEASTAR_H void addGridPoint(float pointX, float pointY, float pointZ, int gridX, int gridY, bool walkable, int movePenalty);

// Adds the points structured in an float array into the astar grid
extern "C" NATIVEASTAR_H void addPoints(float* points, int d1, int d2);

extern "C" NATIVEASTAR_H void addGridPoints(float* points, int d1);

// Removes the point from the astar grid
extern "C" NATIVEASTAR_H void removePoint(float pointX, float pointY, float pointZ);

// Removes the points from the astar grid
extern "C" NATIVEASTAR_H void removePoints(float* points, int d1, int d2);

// Gets the point closest to the passed point
extern "C" NATIVEASTAR_H float* getPoint(float pointX, float pointY, float pointZ);

// Gets the point in the grid
extern "C" NATIVEASTAR_H float* getGridPoint(int gridX, int gridY);

// Gets the astar shortest path from the passed start and end point (with smoothing options)
extern "C" NATIVEASTAR_H float* path(float startX, float startY, float startZ, float endX, float endY, float endZ, bool smooth, float turnDist, float stopDist);

// Grid blurring not included in KD tree implementation
extern "C" NATIVEASTAR_H int* blur(int blurSize);

// Exports the grid in a float array format
extern "C" NATIVEASTAR_H float* exportGrid();

// Takes a float array as input to import an astar grid from
extern "C" NATIVEASTAR_H void importGrid(float* points, int d1);

#endif