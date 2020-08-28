#pragma once

#include "pch.h"
#include "nativeastar.h"

// Initialize the native class.
void native_lib_init()
{
	// Implement initializer here.
}

// Destructor for the native class.
void native_lib_destroy()
{
	Linker::Destroy();
}

void ReleaseMemory(void* arrayPtr)
{
	Linker::Debug("Destroying pointer");
	delete[] arrayPtr;
}

// Sets up the relevent parameters involved in the astar search
//void setup(float _nodeRadius, int _minPenalty, int _maxPenalty)
//{
//	Linker::SetUp()
//	AstarLinker::setupGrid(_nodeRadius, _minPenalty, _maxPenalty);
//}

void setupGrid(int gridSizeX, int gridSizeY, int _minPenalty, int _maxPenalty, float offsetX, float offsetY, float offsetZ)
{
	Linker::SetUp(Vec2(gridSizeX, gridSizeY), _minPenalty, _maxPenalty, Vec3(offsetX, offsetY, offsetZ));
}

// Clears the points in the current grid
void clearPoints()
{
	Linker::ClearGrid();
}

// Adds the passed point into the astar tree
//void addPoint(float pointX, float pointY, float pointZ, bool walkable, int movePenalty)
//{
//	AstarLinker::addGridPoint(std::Vector3(pointX, pointY, pointZ), walkable, movePenalty);
//}

// Adds the passed point into the astar grid
void addGridPoint(float pointX, float pointY, float pointZ, int gridX, int gridY, bool walkable, int movePenalty)
{
	Linker::AddGridPoint(Vec3(pointX, pointY, pointZ), Vec2(gridX, gridY), walkable, movePenalty);
}

// Adds the points structured in an float array into the astar kdtree
//void addPoints(float* points, int d1, int d2)
//{
//	AstarLinker::addGridPoints(points, d1, d2);
//}

// Adds the points into the astar grid
void addGridPoints(float* points, int d1)
{
	Linker::AddGridPoints(points, d1);
}

// Removes the point from the astar grid
//void removePoint(float pointX, float pointY, float pointZ)
//{
//	AstarLinker::removeGridPoint(Vector3(pointX, pointY, pointZ));
//}

// Removes the points from the astar grid
//void removePoints(float* points, int d1, int d2)
//{
//	AstarLinker::removeGridPoints(points, d1, d2);
//}

// Gets the point closest to the passed point
float* getPoint(float pointX, float pointY, float pointZ)
{
	return Linker::GetGridPoint(Vec3(pointX, pointY, pointZ));
}

float* getGridPoint(int gridX, int gridY)
{
	return Linker::GetGridPoint(gridX, gridY);
}

// Gets the nearest neighbors of the passed point
float* getNearestNeighbors(float pointX, float pointY, float pointZ)
{
	return Linker::GetNearestNeighbor(Vec3(pointX, pointY, pointZ));
}

// Gets the astar shortest path from the passed start and end point (with smoothing options)
float* path(float startX, float startY, float startZ, float endX, float endY, float endZ, bool smooth, float turnDist, float stopDist)
{
	return Linker::FindPath(Vec3(startX, startY, startZ), Vec3(endX, endY, endZ), smooth, turnDist, stopDist);
}

int* blur(int blursize)
{
	return Linker::BlurWeights(blursize);
}

float* exportGrid() 
{
	return Linker::Export();
}

void importGrid(float* points, int d1)
{
	Linker::Import(points, d1);
}