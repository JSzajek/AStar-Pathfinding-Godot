#include "pch.h"

#include "PathPoint.h"

PathPoint::PathPoint()
	: m_worldCoord(Vec3()), m_gridCoord(Vec2()), m_walkable(false), 
		m_movePenalty(0), m_parent(NULL), m_gCost(0), m_hCost(0)
{
}

PathPoint::PathPoint(Vec3 worldCoord, Vec2 gridCoord, bool walkable, int movementPenalty)
	: m_worldCoord(worldCoord), m_gridCoord(gridCoord), m_walkable(walkable),
		m_movePenalty(movementPenalty), m_parent(NULL), m_gCost(0), m_hCost(0)
{
}

PathPoint::PathPoint(const PathPoint& other)
	: m_worldCoord(other.GetPosition()), m_gridCoord(Vec2(other.GetGridX(), other.GetGridY())), m_walkable(other.GetWalkable()),
		m_movePenalty(other.GetMovementPenalty()), m_parent(other.GetParent()), m_gCost(other.GetGCost()), m_hCost(other.GetHCost())
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
	int compare = FCost() - other.FCost();
	return compare == 0 ? m_hCost - other.GetHCost() < 0 : compare < 0;
}

const bool PathPoint::operator>(const PathPoint& other) const
{
	int compare = FCost() - other.FCost();
	return compare == 0 ? m_hCost - other.GetHCost() > 0 : compare > 0;
}

const bool PathPoint::operator==(const PathPoint& comparison) const
{
	return m_worldCoord == comparison.GetPosition();
}

const bool PathPoint::operator!=(const PathPoint& comparison) const
{
	return !(*this == comparison);
}