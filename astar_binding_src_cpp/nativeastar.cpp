#pragma once

#include "pch.h"
#include "nativeastar.h"

void native_lib_init()
{
	// Implement initializer here.
}

void native_lib_destroy()
{
	Linker::Destroy();
}

void releaseMemory(void* arrayPtr)
{
	delete[] arrayPtr;
}

void setupGrid(int gridSizeX, int gridSizeY, int _minPenalty, int _maxPenalty, float offsetX, float offsetY, float offsetZ)
{
	Linker::SetUp(Vec2(gridSizeX, gridSizeY), _minPenalty, _maxPenalty, Vec3(offsetX, offsetY, offsetZ));
}

void clearPoints()
{
	Linker::ClearGrid();
}

void addGridPoint(float pointX, float pointY, float pointZ, int gridX, int gridY, bool walkable, int movePenalty)
{
	Linker::AddGridPoint(Vec3(pointX, pointY, pointZ), Vec2(gridX, gridY), walkable, movePenalty);
}

void addGridPoints(float* points, int d1)
{
	Linker::AddGridPoints(points, d1);
}

float* getPoint(float pointX, float pointY, float pointZ)
{
	return Linker::GetGridPoint(Vec3(pointX, pointY, pointZ));
}

float* getGridPoint(int gridX, int gridY)
{
	return Linker::GetGridPoint(gridX, gridY);
}

float* getNearestNeighbors(float pointX, float pointY, float pointZ)
{
	return Linker::GetNearestNeighbor(Vec3(pointX, pointY, pointZ));
}

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