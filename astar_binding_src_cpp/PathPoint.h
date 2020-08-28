#pragma once

#include "Vec3.h"
#include "Vec2.h"

class PathPoint
{
private:
	Vec3 m_worldCoord;
	Vec2 m_gridCoord;
	bool m_walkable;
	int m_movePenalty;
	int gCost, hCost;
	const PathPoint* m_parent;
public:
	PathPoint();
	PathPoint(Vec3 worldCoord, Vec2 gridCoord, bool walkable, int movementPenalty);
	PathPoint(const PathPoint& other);


	const float GetGridX() const { return m_gridCoord.x; }
	const float GetGridY() const { return m_gridCoord.y; }

	const int GetMovementPenalty() const { return m_movePenalty; }
	void SetMovementPenalty(const int value) { m_movePenalty = value; }

	const bool GetWalkable() const { return m_walkable; }
	void SetWalkable(const bool value) { m_walkable = value; }

	const PathPoint* GetParent() const { return m_parent; }
	void SetParent(const PathPoint* value) { m_parent = value; }

	const int GetGCost() const { return gCost; }
	void SetGCost(const int value) { gCost = value; }

	const int GetHCost() const { return hCost; }
	void SetHCost(const int value) { hCost = value; }


	const Vec2 GetGridCoord() const { return m_gridCoord; }
	void SetGridCoord(Vec2 coord) { m_gridCoord = coord; }

	const Vec3 GetPosition() const { return m_worldCoord; }
	void SetPosition(Vec3 coord) { m_worldCoord = coord; }

	const int fCost() const { return gCost + hCost; }

	const float DistanceTo(const PathPoint& otherNode);

	const float ManhattenDistanceTo(const PathPoint& otherNode);

	const bool operator==(const PathPoint& comparison) const;

	const bool operator!=(const PathPoint& comparison) const;

	const bool operator<(const PathPoint& other) const;

	const bool operator>(const PathPoint& other) const;
};