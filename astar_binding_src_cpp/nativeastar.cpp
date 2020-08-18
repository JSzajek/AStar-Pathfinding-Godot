#pragma once

#include <vld.h>
#include "pch.h"
#include "nativeastar.h"
#include "astarlinker.hpp"
#include "kdtree.hpp"
#include "vector2.hpp"

#include <string>
#include <tuple>

using namespace astarlinker;
using namespace std;

// Initialize the native class.
void native_lib_init()
{
	// Implement initializer here.
}

// Destructor for the native class.
void native_lib_destroy()
{
	AstarLinker::Destroy();
}

// Sets up the relevent parameters involved in the astar search
void setup(float _nodeRadius, int _minPenalty, int _maxPenalty)
{
	AstarLinker::setupGrid(_nodeRadius, _minPenalty, _maxPenalty);
}

void setupGrid(int gridSizeX, int gridSizeY, int _minPenalty, int _maxPenalty, float offsetX, float offsetY, float offsetZ)
{
	AstarLinker::setupGrid(Vector2(gridSizeX, gridSizeY), _minPenalty, _maxPenalty, Vector3(offsetX, offsetY, offsetZ));
}

// Clears the points in the current grid
void clearPoints()
{
	AstarLinker::clearGrid();
}

// Adds the passed point into the astar tree
void addPoint(float pointX, float pointY, float pointZ, bool walkable, int movePenalty)
{
	AstarLinker::addGridPoint(std::Vector3(pointX, pointY, pointZ), walkable, movePenalty);
}

// Adds the passed point into the astar grid
void addGridPoint(float pointX, float pointY, float pointZ, int gridX, int gridY, bool walkable, int movePenalty)
{
	AstarLinker::addGridPoint(std::Vector3(pointX, pointY, pointZ), std::Vector2(gridX, gridY), walkable, movePenalty);
}

// Adds the points structured in an float array into the astar kdtree
void addPoints(float* points, int d1, int d2)
{
	AstarLinker::addGridPoints(points, d1, d2);
}

// Adds the points into the astar grid
void addGridPoints(float* points, int d1)
{
	AstarLinker::addGridPoints(points, d1);
}

// Removes the point from the astar grid
void removePoint(float pointX, float pointY, float pointZ)
{
	AstarLinker::removeGridPoint(Vector3(pointX, pointY, pointZ));
}

// Removes the points from the astar grid
void removePoints(float* points, int d1, int d2)
{
	AstarLinker::removeGridPoints(points, d1, d2);
}

// Gets the point closest to the passed point
float* getPoint(float pointX, float pointY, float pointZ)
{
	float* pointVals = AstarLinker::getGridPoint(std::Vector3(pointX, pointY, pointZ));
	return pointVals;
}

float* getGridPoint(int gridX, int gridY)
{
	return AstarLinker::getGridPoint(gridX, gridY);
}

// Gets the nearest neighbors of the passed point
float* getNearestNeighbors(float pointX, float pointY, float pointZ)
{
	return  AstarLinker::getNearestNeighbor(std::Vector3(pointX, pointY, pointZ));
}

// Gets the astar shortest path from the passed start and end point (with smoothing options)
float* path(float startX, float startY, float startZ, float endX, float endY, float endZ, bool smooth, float turnDist, float stopDist)
{
	float* waypoints = AstarLinker::findPath(std::Vector3(startX, startY, startZ), std::Vector3(endX, endY, endZ), smooth, turnDist, stopDist);
	return waypoints;
}

int* blur(int blursize)
{
	tuple<int, int> weights = AstarLinker::blurWeight(blursize);
	return new int[2]{ get<0>(weights), get<1>(weights) };
}

float* exportGrid() 
{
	return AstarLinker::exportGrid();
}

void importGrid(float* points, int d1)
{
	AstarLinker::importGrid(points, d1);
}