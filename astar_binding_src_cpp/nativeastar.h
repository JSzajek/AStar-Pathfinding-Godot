#pragma once

#ifdef NATIVEASTAR_H
#define NATIVEASTAR_H __declspec(dllexport)
#else
#define NATIVEASTAR_H __declspec(dllimport)

#include "Linker.h"

/// <summary>
/// Initialize the native class. 
/// </summary>
extern "C" NATIVEASTAR_H void native_lib_init();

/// <summary>
/// Destructor for the native class.
/// </summary>
/// <returns></returns>
extern "C" NATIVEASTAR_H void native_lib_destroy();

/// <summary>
/// Releases pointer memory after external usage.
/// </summary>
/// <param name="arrayPtr">The pointer to free</param>
extern "C" NATIVEASTAR_H void releaseMemory(void* arrayPtr);

/// <summary>
/// Sets up the relevent parameters involved in the astar search
/// </summary>
/// <param name="gridSizeX">The width of the grid</param>
/// <param name="gridSizeY">The height of the grid</param>
/// <param name="_minPenalty">The minimum movement penalty</param>
/// <param name="_maxPenalty">The maximum movement penalty</param>
/// <param name="offsetX">The x axis coordinate offset</param>
/// <param name="offsetY">The y axis coordinate offset</param>
/// <param name="offsetZ">The z axis coordiante offset</param>
extern "C" NATIVEASTAR_H void setupGrid(int gridSizeX, int gridSizeY, int _minPenalty, int _maxPenalty, float offsetX, float offsetY, float offsetZ);

/// <summary>
/// Clears the points in the current grid
/// </summary>
extern "C" NATIVEASTAR_H void clearPoints();

/// <summary>
/// Adds the passed point into the astar grid
/// </summary>
/// <param name="pointX">The x axis coordinate of the point</param>
/// <param name="pointY">The y axis coordinate of the point</param>
/// <param name="pointZ">The z axis coordinate of the point</param>
/// <param name="gridX">The x axis grid coordinate of the point</param>
/// <param name="gridY">The y axis grid coordinate of the point</param>
/// <param name="walkable">Whether the point is walkable</param>
/// <param name="movePenalty">The movement penalty of the point</param>
extern "C" NATIVEASTAR_H void addGridPoint(float pointX, float pointY, float pointZ, int gridX, int gridY, bool walkable, int movePenalty);

/// <summary>
/// Adds a collection of grid points.
/// </summary>
/// <param name="points">The pointer to the collection of points</param>
/// <param name="d1">The dimension of the collection</param>
extern "C" NATIVEASTAR_H void addGridPoints(float* points, int d1);

/// <summary>
/// Retrieves the point closest to the passed coordinate.
/// </summary>
/// <param name="pointX">The x value</param>
/// <param name="pointY">The y value</param>
/// <param name="pointZ">The z value</param>
/// <returns>The collection of values representing the grid point</returns>
extern "C" NATIVEASTAR_H float* getPoint(float pointX, float pointY, float pointZ);

/// <summary>
/// Retrieves the point at the grid coordinates.
/// </summary>
/// <param name="gridX">The row index</param>
/// <param name="gridY">The column index</param>
/// <returns>The collection of values representing the grid point</returns>
extern "C" NATIVEASTAR_H float* getGridPoint(int gridX, int gridY);

/// <summary>
/// Retrieves the shortest path from the passed start coordinate to the passed end coordinate. With optional path smoothing.
/// </summary>
/// <param name="startX">The x value of the start coordinate</param>
/// <param name="startY">The y value of the start coordinate</param>
/// <param name="startZ">The z value of the start coordinate</param>
/// <param name="endX">The x value of the end coordinate</param>
/// <param name="endY">The y value of the end coordinate</param>
/// <param name="endZ">The z value of the end coordinate</param>
/// <param name="smooth">Whether to smooth the returned path</param>
/// <param name="turnDist">The maximum turn distance when travesing path (for smoothing)</param>
/// <param name="stopDist">The stopping distance (for smoothing)</param>
/// <returns>Collection of float values representing a collection of waypoints along the shortest path</returns>
extern "C" NATIVEASTAR_H float* path(float startX, float startY, float startZ, float endX, float endY, float endZ, bool smooth, float turnDist, float stopDist);

/// <summary>
/// Blurs the weight map of the grid to smooth edges.
/// </summary>
/// <param name="blurSize">The blurring window size</param>
/// <returns>Colleciton of int values representing the new minimum and maximum movement penalty values</returns>
extern "C" NATIVEASTAR_H int* blur(int blurSize);

/// <summary>
/// Exports the grid values.
/// </summary>
/// <returns>Collection of float values representing the grid</returns>
extern "C" NATIVEASTAR_H float* exportGrid();

/// <summary>
/// Initializes the grid from the passed collection of values.
/// </summary>
/// <param name="points">The pointer to the point values of the grid</param>
/// <param name="d1">The dimension of the collection</param>
extern "C" NATIVEASTAR_H void importGrid(float* points, int d1);

#endif