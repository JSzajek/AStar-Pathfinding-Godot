#pragma once

#ifndef NAVMESHLINKER_HPP
#define NAVMESHLINKER_HPP

#include "kdtree.hpp"
#include "vector3.hpp"
#include "navmesh.hpp"
#include <vector>
#include <deque>
#include <map>
#include <iostream>
#include <fstream>
#include <string>

using namespace std;
using namespace astar;

namespace navmesh
{
	class NavmeshLinker
	{
	private:
		//static fstream debugFile;
		static KDTree* tree;
		static NavMesh* mesh;
		static vector<NavMesh*>* meshes;
		static map<Vector3, NavMesh*>* meshDictionary;
		static VertexGraph* meshGraph;
		
		static void finishMeshes();
	public:
		~NavmeshLinker();
		//static void writeToDebug(string str);

		static void setupGrid();
		static void startMesh(vector<Vector3> vertices);
		static void clipEdge(vector<Vector3> vertices);
		static void clipHole(vector<Vector3> vertices);
		static float* getDebugMesh(int index);
		static void endMesh();
		static float* getPath(Vector3 start, Vector3 end);
	};
}

#endif
