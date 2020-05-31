#include "pch.h"
#include "nativenavmesh.h"
#include "navmeshlinker.hpp"
#include <tuple>

using namespace navmesh;
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

// Sets up the relevent parameters involved in navmesh
void setup()
{
	NavmeshLinker::setupGrid();
}

// Starts the creation of a navmesh
void startMesh(float* vertices, int d1) 
{
	vector<Vector3> verts;
	for (int i = 0; i < d1; i++) {
		verts.push_back(Vector3(vertices[(i * 3) + 0], vertices[(i * 3) + 1], vertices[(i * 3) + 2]));
	}
	NavmeshLinker::startMesh(verts);
}

// Clips a edge polygon from the current navmesh
void clipEdge(float* vertices, int d1) 
{
	vector<Vector3> verts;
	for (int i = 0; i < d1; i++) {
		verts.push_back(Vector3(vertices[(i * 3) + 0], vertices[(i * 3) + 1], vertices[(i * 3) + 2]));
	}
	NavmeshLinker::clipEdge(verts);
}

// Clips a hole polygon from the current navmesh
void clipHole(float* vertices, int d1) 
{
	vector<Vector3> verts;
	for (int i = 0; i < d1; i++) {
		verts.push_back(Vector3(vertices[(i * 3) + 0], vertices[(i * 3) + 1], vertices[(i * 3) + 2]));
	}
	NavmeshLinker::clipHole(verts);
}

// Gets the debug triangles from the navmesh at the passed index
float* getDebugMesh(int index) {
	return NavmeshLinker::getDebugMesh(index);
}

// Finishes the current navmesh, by triangulation and storing in navmesh dictionary
void endMesh() {
	NavmeshLinker::endMesh();
}

// Gets the shortest path from the start coordinate to the end coordinate
float* getPath(float startX, float startY, float startZ, float endX, float endY, float endZ)
{
	return NavmeshLinker::getPath(Vector3(startX, startY, startZ), Vector3(endX, endY, endZ));
}