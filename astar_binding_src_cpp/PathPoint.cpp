#include "pch.h"

#include "PathPoint.h"

PathPoint::PathPoint()
{
}

PathPoint::PathPoint(Vec3 worldCoord, Vec2 gridCoord, bool walkable, int movementPenalty)
	: m_worldCoord(worldCoord), m_gridCoord(gridCoord), m_walkable(walkable),
	m_movePenalty(movementPenalty), m_parent(NULL), gCost(0), hCost(0)
{
}

PathPoint::PathPoint(const PathPoint& other)
	: m_worldCoord(other.GetPosition()), m_gridCoord(other.GetGridCoord()), m_walkable(other.GetWalkable()),
	m_movePenalty(other.GetMovementPenalty()), m_parent(other.GetParent()), gCost(other.GetGCost()), hCost(other.GetHCost())
{
}

const float PathPoint::DistanceTo(const PathPoint& otherNode)
{
	return ceil(m_worldCoord.DistanceTo(otherNode.GetPosition()));
}

const float PathPoint::ManhattenDistanceTo(const PathPoint& otherNode)
{
	return ceil(m_worldCoord.ManhattenDistanceTo(otherNode.GetPosition()));
}

const bool PathPoint::operator<(const PathPoint& other) const
{
	int compare = fCost() - other.fCost();
	return compare == 0 ? hCost - other.GetHCost() < 0 : compare < 0;
}

const bool PathPoint::operator>(const PathPoint& other) const
{
	int compare = fCost() - other.fCost();
	return compare == 0 ? hCost - other.GetHCost() > 0 : compare > 0;
}

const bool PathPoint::operator==(const PathPoint& comparison) const
{
	return m_worldCoord == comparison.GetPosition();
}

const bool PathPoint::operator!=(const PathPoint& comparison) const
{
	return !(*this == comparison);
}