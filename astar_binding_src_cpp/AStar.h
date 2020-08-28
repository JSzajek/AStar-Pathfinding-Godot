#include <unordered_set>

#include "Grid.h"
#include "PathPoint.h"
#include "MinHeap.h"

/// <summary>
/// Hash function class for hashing path points
/// </summary>
class PathPointHashFunction
{
public:
	/// <summary>
	/// Function call operator override
	/// </summary>
	/// <param name="node">The point to hash</param>
	/// <returns>The unique hash for the passed point</returns>
	size_t operator()(const PathPoint& point) const
	{
		return std::hash<std::string>()(point.GetPosition().ToString());
	}
};

/// <summary>
/// Class representing AStar algorithm for
/// obstacle pathing. Utilizing a 2-dimensional
/// grid to represent possible walking points
/// in a 3-dimensional world.
/// </summary>
class AStar
{
private:
	int m_minPenalty, m_maxPenalty;
	Vec3 m_worldOffset;
	Grid<PathPoint> m_grid;

public:

	/// <summary>
	/// Initializes a new instance of the <see cref="AStar"/> class.
	/// </summary>
	/// <param name="gridDimension">The grid dimensions</param>
	/// <param name="minPenalty">The minimum movement penalty</param>
	/// <param name="maxPenalty">The maximum movement penalty</param>
	/// <param name="offset">The world offset</param>
	AStar(Vec2 gridDimension, int minPenalty, int maxPenalty, Vec3 offset);
	
	/// <summary>
	/// Initializes a new instance of the <see cref="AStar"/> class.
	/// </summary>
	/// <param name="nodes">The collection of nodes to initialize with</param>
	/// <param name="d1">The dimension of the collection</param>
	AStar(float* nodes, int d1);

	/// <summary>
	/// Clears the grid utilized by the algorithm.
	/// </summary>
	void Clear();

	/// <summary>
	/// Adds a grid node to the grid utilized by the algorithm.
	/// </summary>
	/// <param name="point">The point to add</param>
	void AddGridPoint(PathPoint point);
	
	/// <summary>
	/// Adds a collection of grid points to the grid utilized by the algorithm.
	/// </summary>
	/// <param name="points">The collection of points to add</param>
	/// <param name="d1">The dimension of th collection</param>
	void AddGridPoints(float* points, int d1);

	/// <summary>
	/// Retrieves the grid point closest to the passed world coordinate.
	/// </summary>
	/// <param name="coordinate">The world coordinate</param>
	/// <returns>The grid point closest to the coordinate</returns>
	PathPoint GetGridPoint(Vec3 coordinate);

	/// <summary>
	/// Retrieves the grid point at the passed grid coordinates
	/// </summary>
	/// <param name="xGrid">X grid coordinate</param>
	/// <param name="yGrid">Y grid coordinate</param>
	/// <returns>The grid point at the passed grid coordinate</returns>
	PathPoint GetGridPoint(unsigned xGrid, unsigned yGrid);

	/// <summary>
	/// Retrieves the nearest neighbors to the passed coordinate.
	/// </summary>
	/// <param name="coordinate">The coordinate</param>
	/// <returns>A collection of points near the coordinate</returns>
	std::vector<PathPoint> GetNearestNeighbors(const Vec3& coordinate);

	/// <summary>
	/// Finds the shortest path from the passed starting coordinate to the
	/// passed target coordinate.
	/// </summary>
	/// <param name="startCoordinate">The starting coordinate</param>
	/// <param name="targetCoordinate">The target coordinate</param>
	/// <returns>The collection of the points outlining the shortest path</returns>
	const std::vector<Vec3> FindPath(const Vec3& startCoordinate, const Vec3& targetCoordinate);

	/// <summary>
	/// Blurs the weight map of the grid utilized by the algorithm.
	/// </summary>
	/// <param name="size">The blurr window size</param>
	/// <returns>The new minimum and maximum movement penalty</returns>
	const std::tuple<int, int> BlurWeights(int size);

	/// <summary>
	/// Exports the grid utilized by the algorithm.
	/// </summary>
	/// <returns>A tuple containing the points and necessary information
	///			 for reconstructing the grid</returns>
	const std::tuple<std::vector<PathPoint>, Vec3, int, int, int, int> ExportGrid();
	
	/// <summary>
	/// Initializes the grid utilized by the algorithm with 
	/// the passed colleciton.
	/// </summary>
	/// <param name="points">The collection of points to initialize with</param>
	/// <param name="d1">The dimension of the collection</param>
	void ImportGrid(float* points, int d1);
	
private:

	/// <summary>
	/// Retrieves the nearest neighbors to the passed grid point.
	/// </summary>
	/// <param name="center">The grid point</param>
	/// <returns>A collection of points near the grid point</returns>
	std::vector<PathPoint> GetNearestNeighbors(const PathPoint& center);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="start"></param>
	/// <param name="end"></param>
	/// <returns></returns>
	const std::vector<Vec3> RetracePath(PathPoint start, PathPoint end);
};