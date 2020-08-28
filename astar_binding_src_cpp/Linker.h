#pragma once

#include <iostream>
#include <fstream>
#include <string>
#include "AStar.h"
#include "SmoothPath.h"

/// <summary>
/// Singleton Linker class containing functionality
/// to perform obstacle pathing.
/// </summary>
class Linker
{
public:
	// Prevent assignment and creation
	Linker(const Linker&) = delete;
	void operator = (const Linker&) = delete;

	/// <summary>
	/// Retrieves the static instance of the linker.
	/// </summary>
	/// <returns>The instance of the linker</returns>
	static Linker& Get() {
		static Linker instance;
		return instance;
	}

	/// <summary>
	/// Destroys the linker.
	/// </summary>
	static void Destroy() { Get().DestroyImpl(); }

	/// <summary>
	/// Setting up the astar with the passed variables.
	/// </summary>
	/// <param name="gridSize">The grid size</param>
	/// <param name="minPenalty">The minimum movement penalty</param>
	/// <param name="maxPenalty">The maximum movement penalty</param>
	/// <param name="worldOffset">The world offset</param>
	static void SetUp(Vec2 gridSize, int minPenalty, int maxPenalty, Vec3 worldOffset)
	{
		Get().SetUpImpl(gridSize, minPenalty, maxPenalty, worldOffset);
	}

	/// <summary>
	///  Clears the grid.
	/// </summary>
	static void ClearGrid()
	{
		Get().ClearGridImpl();
	}

	/// <summary>
	/// Adding the grid to the grid
	/// with the passed variables.
	/// </summary>
	/// <param name="coordinate">The point world coordinate</param>
	/// <param name="gridCoord">The point grid coordiante</param>
	/// <param name="walkable">Whether the point is walkable</param>
	/// <param name="movementPenalty">The points movement penalty</param>
	static void AddGridPoint(Vec3 coordinate, Vec2 gridCoord, bool walkable, int movementPenalty)
	{
		Get().AddGridPointImpl(coordinate, gridCoord, walkable, movementPenalty);
	}

	/// <summary>
	/// Adding the collection of points to the grid.
	/// </summary>
	/// <param name="points">The collection of points to add</param>
	/// <param name="d1">The dimensions of the collection</param>
	static void AddGridPoints(float* points, int d1)
	{
		Get().AddGridPointsImpl(points, d1);
	}
	
	/// <summary>
	/// Retrieving the grid point closest to the passed coordinate.
	/// </summary>
	/// <param name="coordinate">The coordinate</param>
	/// <returns>The collection of float values making up the closest point</returns>
	static float* GetGridPoint(Vec3 coordinate)
	{
		return Get().GetGridPointImpl(coordinate);
	}

	/// <summary>
	/// Retrieving the grid point at the passed grid coordinates
	/// </summary>
	/// <param name="xGrid">The x grid coordinate</param>
	/// <param name="yGrid">The x grid coordinate</param>
	/// <returns>The collection of float values making up the closest point</returns>
	static float* GetGridPoint(unsigned int xGrid, unsigned int yGrid)
	{
		return Get().GetGridPointImpl(xGrid, yGrid);
	}
	
	/// <summary>
	/// Retrieving the nearest neighbors to the passed coordinate.
	/// </summary>
	/// <param name="coordinate">The world coordinate</param>
	/// <returns>The collection of floats representing the neartest neighbor points</returns>
	static float* GetNearestNeighbor(Vec3 coordinate)
	{
		return Get().GetNearestNeighborImpl(coordinate);
	}

	/// <summary>
	/// Finding the shortest path from the start to the end coordinates.
	/// </summary>
	/// <param name="start">The start coordinate</param>
	/// <param name="end">The end coordinate</param>
	/// <param name="smooth">Whether to smooth the path</param>
	/// <param name="turnDist">The turn distance (for smoothing)</param>
	/// <param name="stopDist">The stopping distance (for smoothing)</param>
	/// <returns>A collection of float values representing the path</returns>
	static float* FindPath(Vec3 start, Vec3 end, bool smooth, float turnDist, float stopDist)
	{
		return Get().FindPathImpl(start, end, smooth, turnDist, stopDist);
	}

	/// <summary>
	/// Blurring the weights.
	/// </summary>
	/// <param name="size">The blurring window size</param>
	/// <returns>Collection of int values representing the new min and max movement penalties</returns>
	static int* BlurWeights(int size)
	{
		return Get().BlurWeightsImpl(size);
	}

	/// <summary>
	/// Exporting the current grid state.
	/// </summary>
	/// <returns>The collection of float representing the grid</returns>
	static float* Export()
	{
		return Get().ExportImpl();
	}

	/// <summary>
	/// Initializing the grid with the passed values.
	/// </summary>
	/// <param name="points">The collection of values making up the grid</param>
	/// <param name="d1">The dimension of the collection</param>
	static void Import(float* points, int d1)
	{
		return Get().ImportImpl(points, d1);
	}

private:
	AStar astar;

	/// <summary>
	/// Initializes a new instance of the <see cref="Linker"/> class.
	/// </summary>
	Linker();

	/// <summary>
	/// Implements the destroy method.
	/// </summary>
	void DestroyImpl();

	/// <summary>
	/// Implements the Setup method. Setting up the astar with the 
	/// passed variables.
	/// </summary>
	/// <param name="gridSize">The grid size</param>
	/// <param name="minPenalty">The minimum movement penalty</param>
	/// <param name="maxPenalty">The maximum movement penalty</param>
	/// <param name="worldOffset">The world offset</param>
	void SetUpImpl(Vec2 gridSize, int minPenalty, int maxPenalty, Vec3 worldOffset);
	
	/// <summary>
	/// Implements the clear grid method. Clearing the grid.
	/// </summary>
	void ClearGridImpl();

	/// <summary>
	/// Implements the add grid point method. Adding the grid to the grid
	/// with the passed variables.
	/// </summary>
	/// <param name="coordinate">The point world coordinate</param>
	/// <param name="gridCoord">The point grid coordiante</param>
	/// <param name="walkable">Whether the point is walkable</param>
	/// <param name="movementPenalty">The points movement penalty</param>
	void AddGridPointImpl(Vec3 coordinate, Vec2 gridCoord, bool walkable, int movementPenalty);
	
	/// <summary>
	/// Implements the add grid points method. Adding the collection of points
	/// to the grid.
	/// </summary>
	/// <param name="points">The collection of points to add</param>
	/// <param name="d1">The dimensions of the collection</param>
	void AddGridPointsImpl(float* points, int d1);

	/// <summary>
	/// Implements the get grid point method. Retrieving the grid point
	/// closest to the passed coordinate.
	/// </summary>
	/// <param name="coordinate">The coordinate</param>
	/// <returns>The collection of float values making up the closest point</returns>
	float* GetGridPointImpl(Vec3 coordinate);
	
	/// <summary>
	/// Implements the get grid point method. Retrieving the grid point
	/// at the passed grid coordinates
	/// </summary>
	/// <param name="xGrid">The x grid coordinate</param>
	/// <param name="yGrid">The x grid coordinate</param>
	/// <returns>The collection of float values making up the closest point</returns>
	float* GetGridPointImpl(unsigned int xGrid, unsigned int yGrid);
	
	/// <summary>
	/// Implements the get neartest neighbor method. Retrieving the nearest
	/// neighbors to the passed coordinate.
	/// </summary>
	/// <param name="coordinate">The world coordinate</param>
	/// <returns>The collection of floats representing the neartest neighbor points</returns>
	float* GetNearestNeighborImpl(Vec3 coordinate);

	/// <summary>
	/// Implements the find path method. Finding the shortest path from 
	/// the start to the end coordinates.
	/// </summary>
	/// <param name="start">The start coordinate</param>
	/// <param name="end">The end coordinate</param>
	/// <param name="smooth">Whether to smooth the path</param>
	/// <param name="turnDist">The turn distance (for smoothing)</param>
	/// <param name="stopDist">The stopping distance (for smoothing)</param>
	/// <returns>A collection of float values representing the path</returns>
	float* FindPathImpl(Vec3 start, Vec3 end, bool smooth, float turnDist, float stopDist);
	
	/// <summary>
	/// Implements the blur weights method. Blurring the weights.
	/// </summary>
	/// <param name="size">The blurring window size</param>
	/// <returns>Collection of int values representing the new min and max movement penalties</returns>
	int* const BlurWeightsImpl(int size);

	/// <summary>
	/// Implements the export method. Exporting the current grid state.
	/// </summary>
	/// <returns>The collection of float representing the grid</returns>
	float* ExportImpl();
	
	/// <summary>
	/// Implement the import method. Initializing the grid with the passed values.
	/// </summary>
	/// <param name="points">The collection of values making up the grid</param>
	/// <param name="d1">The dimension of the collection</param>
	void ImportImpl(float* points, int d1);

private:

	/// <summary>
	/// Unpacks the smooth path into a collection of float values, 
	/// representing the smooth path.
	/// </summary>
	/// <param name="path">The path to convert</param>
	/// <returns>A collection of float values representing the path</returns>
	float* UnpackSmoothPath(const SmoothPath& path);
	
	/// <summary>
	/// Converting the passed grid point to a collection of float values.
	/// </summary>
	/// <param name="point">The point to convert</param>
	/// <returns>The collection of float values making up the closest point</returns>
	float* ConvertGridPoint(const PathPoint& point);
	
	/// <summary>
	/// Converts a collecion of path points into a collection of float values.
	/// </summary>
	/// <param name="points">The collection path points</param>
	/// <returns>The collection of float values making up the path point collection</returns>
	float* ConvertToFloatArray(const std::vector<PathPoint>& points);
	
	/// <summary>
	/// Converts a collection of vertices into a collection of float values.
	/// </summary>
	/// <param name="vertices">The collection of vertices</param>
	/// <returns>The collection of float values making up the collection of vertices</returns>
	float* ConvertToFloatArray(const std::vector<Vec3>& vertices);
};