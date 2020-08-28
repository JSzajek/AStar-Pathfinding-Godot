#include "pch.h"
#include "Linker.h"

void Linker::DestroyImpl()
{
	// To Do implement necessary memory managment
}

Linker::Linker()
	: astar(Vec2(), 0, 0, Vec3())
{
	debugFile = std::fstream();
	debugFile.open("astarlinker_debug.txt", std::fstream::in | std::fstream::out | std::fstream::trunc);
}

void Linker::SetUpImpl(Vec2 gridSize, int minPenalty, int maxPenalty, Vec3 worldOffset)
{
	astar = AStar(gridSize, minPenalty, maxPenalty, worldOffset);
}

void Linker::ClearGridImpl()
{
	astar.Clear();
}

void Linker::AddGridPointImpl(Vec3 coordinate, Vec2 gridCoord, bool walkable, int movementPenalty)
{
	astar.AddGridNode(PathPoint(coordinate, gridCoord, walkable, movementPenalty));
}

void Linker::AddGridPointsImpl(float* points, int d1)
{
	astar.AddGridNodes(points, d1);
}

float* Linker::GetGridPointImpl(Vec3 coordinate)
{
	return GetGridPointImpl(astar.GetGridNode(coordinate));
}

float* Linker::GetGridPointImpl(unsigned int xGrid, unsigned int yGrid)
{
	return GetGridPointImpl(astar.GetGridNode(xGrid, yGrid));
}

float* Linker::GetGridPointImpl(const PathPoint& point)
{
	Vec3 pos = point.GetPosition();
	return new float[5]
	{
		pos.x, pos.y, pos.z,
		(float)point.GetMovementPenalty(),
		(float)point.GetWalkable() ? 1.0f : 0.0f
	};
}

float* Linker::GetNearestNeighborImpl(Vec3 coordinate)
{
	return ConvertToFloatArray(astar.GetNearestNeighbors(coordinate));
}

float* Linker::FindPathImpl(Vec3 start, Vec3 end, bool smooth, float turnDist, float stopDist)
{
	std::vector<Vec3> points = astar.FindPath(start, end);
	if (smooth)
	{
		return UnpackSmoothPath(SmoothPath(points, start, turnDist, stopDist));
	}
	else
	{
		return ConvertToFloatArray(points);
	}
}

float* Linker::UnpackSmoothPath(const SmoothPath& path)
{
	std::vector<float> unpacked;
	std::vector<Vec3> points = path.GetLookPoints();
	std::vector<Line> turns = path.GetTurnBoundaries();

	// First index as indicator for size of array
	int size = (points.size() * 3) + (turns.size() * 7) + 3;
	unpacked.reserve(size);
	unpacked.emplace_back((points.size() * 3) + (turns.size() * 7) + 3);

	// Push back finish line and slow down indices
	unpacked.emplace_back(path.GetFinishLineIndex());
	unpacked.emplace_back(path.GetSlowDownIndex());

	for (int i = 0; i < points.size(); i++) {
		// Adding waypoint vector3
		unpacked.emplace_back(points[i].x);
		unpacked.emplace_back(points[i].y);
		unpacked.emplace_back(points[i].z);

		// Adding Line values
		unpacked.emplace_back(turns[i].GetGradient());
		unpacked.emplace_back(turns[i].GetPerpGradient());
		unpacked.emplace_back(turns[i].GetPointOnLine1().x);
		unpacked.emplace_back(turns[i].GetPointOnLine1().y);
		unpacked.emplace_back(turns[i].GetPointOnLine2().x);
		unpacked.emplace_back(turns[i].GetPointOnLine2().y);
		unpacked.emplace_back(turns[i].GetApproachSide() ? 1.0f : 0.0f);
	}

	float* data = new float[unpacked.size()];
	std::copy(unpacked.begin(), unpacked.end(), data);
	return data;
}

int* const Linker::BlurWeightsImpl(int size)
{
	std::tuple<int, int> weights = astar.BlurWeights(size);
	return new int[2]{ std::get<0>(weights), std::get<1>(weights) };
}

float* Linker::ExportImpl()
{
	std::tuple<std::vector<PathPoint>, Vec3, int, int, int, int> packed_astar = astar.ExportGrid();
	std::vector<PathPoint> packed_grid = std::get<0>(packed_astar);

	if (packed_grid.size() == 0) { return new float[0]{}; }

	// Total size, type of astar, Vector3 offset,
	// grid size x, grid size y, 
	// min penalty, max penalty, points...

	std::vector<float> unpacked;

	unpacked.push_back((packed_grid.size() * 7) + 9);
	unpacked.push_back(0); // Format type of grid
	unpacked.push_back(std::get<1>(packed_astar).x);
	unpacked.push_back(std::get<1>(packed_astar).y);
	unpacked.push_back(std::get<1>(packed_astar).z);

	unpacked.push_back(std::get<2>(packed_astar));
	unpacked.push_back(std::get<3>(packed_astar));
	unpacked.push_back(std::get<4>(packed_astar));
	unpacked.push_back(std::get<5>(packed_astar));

	for (int i = 0; i < packed_grid.size(); i++)
	{
		unpacked.push_back(packed_grid[i].GetGridX());
		unpacked.push_back(packed_grid[i].GetGridY());

		unpacked.push_back(packed_grid[i].GetPosition().x);
		unpacked.push_back(packed_grid[i].GetPosition().y);
		unpacked.push_back(packed_grid[i].GetPosition().z);

		unpacked.push_back(packed_grid[i].GetWalkable());
		unpacked.push_back(packed_grid[i].GetMovementPenalty());
	}
	float* data = new float[unpacked.size()];
	std::copy(unpacked.begin(), unpacked.end(), data);
	return data;
}

void Linker::ImportImpl(float* points, int d1)
{
	astar = AStar(points, d1);
}

float* Linker::ConvertToFloatArray(const std::vector<PathPoint>& points)
{
	std::vector<float> unpacked;
	unsigned int size = (points.size() * 3) + 1;
	unpacked.reserve(size);
	unpacked.emplace_back(size);
	Vec3 vec;
	for (int i = 0; i < points.size(); i++)
	{
		vec = points[i].GetPosition();
		unpacked.emplace_back(vec.x);
		unpacked.emplace_back(vec.y);
		unpacked.emplace_back(vec.z);
	}

	float* data = new float[unpacked.size()];
	std::copy(unpacked.begin(), unpacked.end(), data);
	return data;
}

float* Linker::ConvertToFloatArray(const std::vector<Vec3>& vertices)
{
	std::vector<float> unpacked;
	unsigned int size = (vertices.size() * 3) + 1;
	unpacked.reserve(size);
	unpacked.emplace_back(size);
	Vec3 vec;
	for (int i = 0; i < vertices.size(); i++)
	{
		unpacked.emplace_back(vertices[i].x);
		unpacked.emplace_back(vertices[i].y);
		unpacked.emplace_back(vertices[i].z);
	}

	float* data = new float[unpacked.size()];
	std::copy(unpacked.begin(), unpacked.end(), data);
	return data;
}