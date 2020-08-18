#pragma once

#ifndef ASTARLINKER_HPP
#define ASTARLINKER_HPP

#include "astar.hpp"
#include "smoothpath.hpp"
#include <vector>
#include <deque>
#include <iostream>
#include <fstream>
#include <string>

using namespace std;
using namespace astar;

namespace astarlinker
{
	// A static class holding a static reference to an AStar
	// class for usage within the dll
	class AstarLinker
	{
	private:
		//static fstream debugFile;
		static AStar* astar;

		static float* unpackRegularPath(vector<Vector3> points);
		static float* unpackSmoothPath(SmoothPath* path);
	public:
		
		~AstarLinker();
		static void Destroy();
		//static void writeToDebug(string str);

		static void setupGrid(float _nodeRadius, int _minPenalty, int _maxPenalty);
		static void setupGrid(Vector2 gridSize, int _minPenalty, int _maxPenalty, Vector3 offset);
		static void clearGrid();
		static void addGridPoint(Vector3 pos, bool walkable, int movementPenalty);
		static void addGridPoint(Vector3 pos, Vector2 gridPos, bool walkable, int movePenalty);
		static void addGridPoints(float* points, int d1, int d2);
		static void addGridPoints(float* points, int d1);
		static void removeGridPoint(Vector3 pos);
		static void removeGridPoints(float* points, int d1, int d2);
		static float* getGridPoint(Vector3 position);
		static float* getGridPoint(int gridX, int gridY);
		static float* getNearestNeighbor(Vector3 position);
		static float* findPath(Vector3 start, Vector3 end, bool smooth, float turnDist, float stopDist);
		static string ToString(); // For testing
		static tuple<int, int> blurWeight(int _blurSize);

		static float* exportGrid();
		static void importGrid(float* points, int d1);
	};
}

#endif
