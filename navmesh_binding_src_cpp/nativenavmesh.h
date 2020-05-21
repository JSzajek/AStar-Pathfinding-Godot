#pragma once

#ifdef NATIVENAVMESH_H
#define NATIVENAVMESH_H __declspec(dllexport)
#else
#define NATIVENAVMESH_H __declspec(dllimport)


// Initialize the native class.
extern "C" NATIVENAVMESH_H void native_lib_init();

// Destructor for the native class.
extern "C" NATIVENAVMESH_H void native_lib_destroy();

// Sets up the relevent parameters involved in navmesh
extern "C" NATIVENAVMESH_H void setup();

// Adds the passed point into the kdtree
extern "C" NATIVENAVMESH_H void addPoint(float pointX, float pointY, float pointZ);

// Adds the points structured in an float array into the kdtree
extern "C" NATIVENAVMESH_H void addPoints(float* points, int d1, int d2);

// Gets the point closest to the passed point
extern "C" NATIVENAVMESH_H float* getPoint(float pointX, float pointY, float pointZ);

#endif