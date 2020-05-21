#include "pch.h"
#include "nativenavmesh.h"
#include "navmeshlinker.hpp"
#include <tuple>

using namespace navmeshlinker;
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

// Adds the passed point into the kdtree
void addPoint(float pointX, float pointY, float pointZ)
{
	NavmeshLinker::addGridPoint(Vector3(pointX, pointY, pointZ));
}

// Adds the points structured in an float array into the kdtree
void addPoints(float* points, int d1, int d2)
{
	NavmeshLinker::addGridPoints(points, d1, d2);
}

// Gets the point closest to the passed point
float* getPoint(float pointX, float pointY, float pointZ)
{
	return NavmeshLinker::getGridPoint(Vector3(pointX, pointY, pointZ));
}