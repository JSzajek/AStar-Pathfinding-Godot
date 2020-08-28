#include "pch.h"

#include "AStar.h"

AStar::AStar(Vec2 gridDimension, int minPenalty, int maxPenalty, Vec3 offset)
	: m_minPenalty(minPenalty), m_maxPenalty(maxPenalty),
		m_grid(Grid<PathPoint>((int)gridDimension.x, (int)gridDimension.y)),
		m_worldOffset(offset)
{
}

AStar::AStar(float* nodes, int d1)
	: m_worldOffset(Vec3(nodes[2], nodes[3], nodes[4])), m_grid(Grid<PathPoint>(nodes[5], nodes[6])),
		m_minPenalty(nodes[7]), m_maxPenalty(nodes[8])
{
	ImportGrid(nodes, d1);
}

void AStar::Clear()
{
	m_grid = Grid<PathPoint>(0,0);
}

void AStar::AddGridPoint(PathPoint point)
{
	m_grid(point.GetGridX(), point.GetGridY()) = PathPoint(point);
}

void AStar::AddGridPoints(float* points, int d1)
{
	for (int i = 0; i < (points[0] - 1) / 7; i++)
	{
		int base = (i * 7) + 1;
		Vec2 pos(points[base], points[base + 1]);
		Vec3 coord(points[base + 2], points[base + 3], points[base + 4]);
		m_grid(pos.x, pos.y) = PathPoint(coord, pos, points[base + 5], points[base + 6]);
	}
}

PathPoint AStar::GetGridPoint(Vec3 coordinate)
{
	return m_grid(coordinate - m_worldOffset);
}

PathPoint AStar::GetGridPoint(unsigned xGrid, unsigned yGrid)
{
	return m_grid(xGrid, yGrid);
}

std::vector<PathPoint> AStar::GetNearestNeighbors(const Vec3& coordinate)
{
	return GetNearestNeighbors(m_grid(coordinate));
}

std::vector<PathPoint> AStar::GetNearestNeighbors(const PathPoint& center)
{
	return m_grid.GetNeighbors(center.GetGridX(), center.GetGridY());
}

const std::vector<Vec3> AStar::FindPath(const Vec3& startCoordinate, const Vec3& targetCoordinate)
{
	unsigned safety = 0;
	bool success = false;
	PathPoint start = m_grid(startCoordinate);
	PathPoint target = m_grid(targetCoordinate);

	MinHeap<PathPoint> open;
	HeapItem<PathPoint>* foundPoint;
	std::unordered_set<PathPoint, PathPointHashFunction> closed;
	closed.reserve(25);
	if (start.GetWalkable() && target.GetWalkable())
	{
		// A* Path finding algorithm
		open.Add(start);

		while (open.Size() > 0)
		{
			// This path is taking too long to compute so finding failed
			if (safety > 10000) { break; }

			PathPoint current = open.RemoveFirst();
			closed.emplace(current);

			if (current == target)
			{
				target = *closed.find(target);
				success = true;
				break;
			}

			std::vector<PathPoint> neighbors = GetNearestNeighbors(current);

			for (int i = 0; i < neighbors.size(); i++)
			{
				PathPoint neighbor = neighbors[i];

				if (!neighbor.GetWalkable() || closed.find(neighbor) != closed.end()) { continue; }

				int newMoveCost = current.GetGCost() + current.ManhattenDistanceTo(neighbor) + neighbor.GetMovementPenalty();
				foundPoint = open.Find(neighbor);

				if (foundPoint != NULL && newMoveCost < foundPoint->GetItemPtr()->GetGCost())
				{
					PathPoint* ptr = foundPoint->GetItemPtr();
					ptr->SetGCost(newMoveCost);
					ptr->SetHCost(neighbor.ManhattenDistanceTo(target));
					ptr->SetParent(&(*closed.find(current)));
					open.UpdateItem(foundPoint);
				}
				else if (foundPoint == NULL)
				{
					neighbor.SetGCost(newMoveCost);
					neighbor.SetHCost(neighbor.ManhattenDistanceTo(target));
					neighbor.SetParent(&(*closed.find(current)));
					open.Add(neighbor);
				}
			}
			safety++;
		}
	}
	if (success)
	{
		std::vector<Vec3> temp = RetracePath(start, target);
		return temp;
	}
	return {};
}

const std::vector<Vec3> AStar::RetracePath(PathPoint start, PathPoint end)
{
	std::vector<PathPoint> nodes;
	PathPoint& current = end;
	while (current != start)
	{
		nodes.emplace_back(current);
		current = *current.GetParent();
	}

	std::vector<Vec3> waypoints;
	waypoints.reserve(nodes.size());
	float oldDir = FLT_EPSILON; // Impossible direction
	for (size_t i = 0; i < nodes.size(); i++)
	{
		if (i == 0)
		{
			waypoints.push_back(nodes[i].GetPosition());
			continue;
		}

		float newDir = nodes[i - 1].GetPosition().DirectionTo(nodes[i].GetPosition());
		if (!(abs(oldDir - newDir) < FLT_EPSILON))
		{
			waypoints.push_back(nodes[i].GetPosition());
		}
		oldDir = newDir;
	}
	std::reverse(waypoints.begin(), waypoints.end());
	return waypoints;
}

const std::tuple<int, int> AStar::BlurWeights(int size)
{
	int kernelSize = size * 2 + 1;
	int kernelExtents = (kernelSize - 1) / 2.0f;

	int gridWidth = m_grid.GetWidth();
	int gridHeight = m_grid.GetHeight();

	Matrix<int> horizontalPass(gridWidth, gridHeight);
	Matrix<int> verticalPass(gridWidth, gridHeight);

	for (int y = 0; y < gridHeight; y++)
	{
		for (int x = -kernelExtents; x <= kernelExtents; x++)
		{
			int sampleX = clamp(x, 0, kernelExtents);
			horizontalPass(0, y) += m_grid(sampleX, y).GetMovementPenalty();
		}

		for (int x = 1; x < gridWidth; x++)
		{
			int removeIndex = clamp(x - kernelExtents - 1, 0, gridWidth);
			int addIndex = clamp(x + kernelExtents, 0, gridWidth - 1);
			horizontalPass(x, y) = horizontalPass(x - 1, y)
				- m_grid(removeIndex, y).GetMovementPenalty()
				+ m_grid(addIndex, y).GetMovementPenalty();
		}
	}
	for (int x = 0; x < gridWidth; x++)
	{
		for (int y = -kernelExtents; y <= kernelExtents; y++)
		{
			int sampleY = clamp(y, 0, kernelExtents);
			verticalPass(x, 0) += horizontalPass(x, sampleY);
		}
		int blurredPenalty = std::round(verticalPass(x, 0) / (float)(kernelSize * kernelSize));
		m_grid(x, 0).SetMovementPenalty(blurredPenalty);
		for (int y = 1; y < gridHeight; y++)
		{
			int removeIndex = clamp(y - kernelExtents - 1, 0, gridHeight);
			int addIndex = clamp(y + kernelExtents, 0, gridHeight - 1);

			verticalPass(x, y) = verticalPass(x, y - 1)
				- horizontalPass(x, removeIndex)
				+ horizontalPass(x, addIndex);
			blurredPenalty = std::round(verticalPass(x, y) / (float)(kernelSize * kernelSize));
			m_grid(x, y).SetMovementPenalty(blurredPenalty);

			if (blurredPenalty > m_maxPenalty)
			{
				m_maxPenalty = blurredPenalty;
			}
			if (blurredPenalty < m_minPenalty)
			{
				m_minPenalty = blurredPenalty;
			}
		}
	}
	return std::make_tuple(m_minPenalty, m_maxPenalty);
}

const std::tuple<std::vector<PathPoint>, Vec3, int, int, int, int> AStar::ExportGrid()
{
	return make_tuple(m_grid.Export(), m_worldOffset,
					  m_grid.GetWidth(), m_grid.GetHeight(),
					  m_minPenalty, m_maxPenalty);
}

void AStar::ImportGrid(float* points, int d1)
{
	for (int i = 0; i < (points[0] - 9) / 7; i++)
	{
		int base = 9 + (i * 7);
		Vec2 pos = Vec2(points[base], points[base + 1]);
		Vec3 coord = Vec3(points[base + 2], points[base + 3], points[base + 4]);
		PathPoint& point = m_grid(pos.x, pos.y);
		point.SetGridCoord(pos);
		point.SetPosition(coord);
		point.SetWalkable(points[base + 5]);
		point.SetMovementPenalty(points[base + 6]);
	}
}