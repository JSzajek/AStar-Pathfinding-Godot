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
		//static void writeToDebug(string str);

		static void setupGrid(float _nodeRadius, int _minPenalty, int _maxPenalty);
		static void clearGrid();
		static void addGridPoint(Vector3 pos, bool walkable, int movementPenalty);
		static void addGridPoints(float* points, int d1, int d2);
		static void removeGridPoint(Vector3 pos);
		static void removeGridPoints(float* points, int d1, int d2);
		static float* getGridPoint(Vector3 position);
		static float* getNearestNeighbor(Vector3 position);
		static float* findPath(Vector3 start, Vector3 end, bool smooth, float turnDist, float stopDist);
		static string ToString(); // For testing
		//static tuple<int, int> blurWeight(int _blurSize);
	};
}

#endif
