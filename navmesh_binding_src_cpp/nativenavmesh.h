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

// Starts the creation of a navmesh
extern "C" NATIVENAVMESH_H void startMesh(float* vertices, int d1);

// Clips a edge polygon from the current navmesh
extern "C" NATIVENAVMESH_H void clipEdge(float* vertices, int d1);

// Clips a hole polygon from the current navmesh
extern "C" NATIVENAVMESH_H void clipHole(float* vertices, int d1);

// Gets the debug triangles from the navmesh at the passed index
extern "C" NATIVENAVMESH_H float* getDebugMesh(int index = 0);

// Finishes the current navmesh, by triangulation and storing in navmesh dictionary
extern "C" NATIVENAVMESH_H void endMesh();

// Gets the shortest path from the start coordinate to the end coordinate
extern "C" NATIVENAVMESH_H float* getPath(float startX, float startY, float startZ, float endX, float endY, float endZ);

#endif