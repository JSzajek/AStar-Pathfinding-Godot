#include <unordered_set>

#include "Grid.h"
#include "PathPoint.h"
#include "MinHeap.h"

class PathPointHashFunction
{
public:
	size_t operator()(const PathPoint& node) const
	{
		return std::hash<std::string>()(node.GetPosition().ToString());
	}
};

class AStar
{
private:
	int m_minPenalty, m_maxPenalty;
	Vec3 m_worldOffset;
	Grid<PathPoint> m_grid;

public:
	AStar(Vec2 gridDimension, int minPenalty, int maxPenalty, Vec3 offset);
	AStar(float* nodes, int d1);

	void Clear();

	void AddGridNode(PathPoint node);
	void AddGridNodes(float* nodes, int d1);

	PathPoint GetGridNode(Vec3 coordinate);

	PathPoint GetGridNode(unsigned xGrid, unsigned yGrid);

	std::vector<PathPoint> GetNearestNeighbors(const Vec3& coordinate);
	std::vector<PathPoint> GetNearestNeighbors(const PathPoint& center);

	const std::vector<Vec3> FindPath(const Vec3& startCoordinate, const Vec3& targetCoordinate);
	const std::vector<Vec3> RetracePath(PathPoint start, PathPoint end);

	const std::tuple<int, int> BlurWeights(int size);

	const std::tuple<std::vector<PathPoint>, Vec3, int, int, int, int> ExportGrid();
	void ImportGrid(float* nodes, int d1);
};