#pragma once

#ifndef NAVMESHLINKER_HPP
#define NAVMESHLINKER_HPP

#include "kdtree.hpp"
#include "vector3.hpp"
#include <vector>
#include <deque>
#include <iostream>
#include <fstream>
#include <string>

using namespace std;
using namespace astar;

namespace navmeshlinker
{
	class NavmeshLinker
	{
	private:
		//static fstream debugFile;
		static KDTree* tree;
	public:
		~NavmeshLinker();
		//static void writeToDebug(string str);

		static void setupGrid();
		static void addGridPoint(Vector3 pos);
		static void addGridPoints(float* points, int d1, int d2);
		static float* getGridPoint(Vector3 position);
	};
}

#endif
