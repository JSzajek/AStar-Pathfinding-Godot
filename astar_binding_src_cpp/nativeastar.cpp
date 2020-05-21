#pragma once

#include "pch.h"
#include "nativeastar.h"
#include "astarlinker.hpp"
#include "kdtree.hpp"
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
	// Implement destory here.
}

// Sets up the relevent parameters involved in the astar search
void setup(float _nodeRadius, int _minPenalty, int _maxPenalty)
{
	AstarLinker::setupGrid(_nodeRadius, _minPenalty, _maxPenalty);
}

// Clears the points in the current grid
void clearPoints()
{
	AstarLinker::clearGrid();
}

// Adds the passed point into the astar grid
void addPoint(float pointX, float pointY, float pointZ, bool walkable, int movePenalty)
{
	AstarLinker::addGridPoint(std::Vector3(pointX, pointY, pointZ), walkable, movePenalty);
}

// Adds the points structured in an float array into the astar grid
void addPoints(float* points, int d1, int d2)
{
	AstarLinker::addGridPoints(points, d1, d2);
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

//int* blur(int blurSize)
//{
//	tuple<int, int> weights = AstarLinker::blurWeight(blurSize);
//	return new int[2]{ get<0>(weights), get<1>(weights) };
//}