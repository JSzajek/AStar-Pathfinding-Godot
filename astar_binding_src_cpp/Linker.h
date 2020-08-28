#pragma once

#include <iostream>
#include <fstream>
#include <string>
#include "AStar.h"
#include "SmoothPath.h"

class Linker
{
public:
	Linker(const Linker&) = delete;
	void operator = (const Linker&) = delete;

	static Linker& Get() {
		static Linker instance;
		return instance;
	}

	static void Destroy() { Get().DestroyImpl(); }

	static void SetUp(Vec2 gridSize, int minPenalty, int maxPenalty, Vec3 worldOffset)
	{
		Get().SetUpImpl(gridSize, minPenalty, maxPenalty, worldOffset);
	}

	static void ClearGrid()
	{
		Get().ClearGridImpl();
	}

	static void AddGridPoint(Vec3 coordinate, Vec2 gridCoord, bool walkable, int movementPenalty)
	{
		Get().AddGridPointImpl(coordinate, gridCoord, walkable, movementPenalty);
	}

	static void AddGridPoints(float* points, int d1)
	{
		Get().AddGridPointsImpl(points, d1);
	}
	
	static float* GetGridPoint(Vec3 coordinate)
	{
		return Get().GetGridPointImpl(coordinate);
	}

	static float* GetGridPoint(unsigned int xGrid, unsigned int yGrid)
	{
		return Get().GetGridPointImpl(xGrid, yGrid);
	}
	
	static float* GetNearestNeighbor(Vec3 coordinate)
	{
		return Get().GetNearestNeighborImpl(coordinate);
	}

	static float* FindPath(Vec3 start, Vec3 end, bool smooth, float turnDist, float stopDist)
	{
		return Get().FindPathImpl(start, end, smooth, turnDist, stopDist);
	}

	static int* BlurWeights(int size)
	{
		return Get().BlurWeightsImpl(size);
	}

	static float* Export()
	{
		return Get().ExportImpl();
	}

	static void Import(float* points, int d1)
	{
		return Get().ImportImpl(points, d1);
	}

	static void Debug(std::string str)
	{
		Get().DebugImpl(str);
	}

private:
	AStar astar;
	std::fstream debugFile;

	void DebugImpl(std::string str)
	{
		debugFile << str << std::endl;
	}

	Linker();

	void DestroyImpl();

	void SetUpImpl(Vec2 gridSize, int minPenalty, int maxPenalty, Vec3 worldOffset);
	void ClearGridImpl();

	void AddGridPointImpl(Vec3 coordinate, Vec2 gridCoord, bool walkable, int movementPenalty);
	void AddGridPointsImpl(float* points, int d1);

	float* GetGridPointImpl(Vec3 coordinate);
	float* GetGridPointImpl(unsigned int xGrid, unsigned int yGrid);
	float* GetGridPointImpl(const PathPoint& point);

	float* GetNearestNeighborImpl(Vec3 coordinate);

	float* ConvertToFloatArray(const std::vector<PathPoint>& points);
	float* ConvertToFloatArray(const std::vector<Vec3>& vertices);

	float* FindPathImpl(Vec3 start, Vec3 end, bool smooth, float turnDist, float stopDist);
	float* UnpackSmoothPath(const SmoothPath& path);
	
	int* const BlurWeightsImpl(int size);

	float* ExportImpl();
	void ImportImpl(float* points, int d1);
};