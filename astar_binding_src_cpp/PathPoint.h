#pragma once

#include "Vec3.h"
#include "Vec2.h"

/// <summary>
/// Class representing the a potential pathway point.
/// </summary>
class PathPoint
{
private:
	Vec3 m_worldCoord;
	Vec2 m_gridCoord;
	bool m_walkable;
	int m_movePenalty;
	int m_gCost, m_hCost;
	const PathPoint* m_parent;

public:

#pragma region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="PathPoint"/> class.
	/// </summary>
	PathPoint();
	
	/// <summary>
	/// Initializes a new instance of the <see cref="PathPoint"/> class.
	/// </summary>
	/// <param name="worldCoord">The world coordinate of the point</param>
	/// <param name="gridCoord">The grid coordinate of the point</param>
	/// <param name="walkable">Whether the point is walkable</param>
	/// <param name="movementPenalty">The movement penalty of the point</param>
	PathPoint(Vec3 worldCoord, Vec2 gridCoord, bool walkable, int movementPenalty);
	
	/// <summary>
	/// Copy constructor of the <see cref="PathPoint"/> class.
	/// </summary>
	/// <param name="other">The other to copy</param>
	PathPoint(const PathPoint& other);

#pragma endregion Constructors

#pragma region Getters/Setters

	/// <summary>
	/// Retrieves the grid X coordinate of the path point.
	/// </summary>
	/// <returns>The grid X coordinate</returns>
	const float GetGridX() const { return m_gridCoord.x; }
	
	/// <summary>
	/// Retrieves the grid Y coordinate of the path point.
	/// </summary>
	/// <returns>The grid Y coordinate</returns>
	const float GetGridY() const { return m_gridCoord.y; }

	/// <summary>
	/// Retrieves the movement penalty of the path point.
	/// </summary>
	/// <returns>The movement penalty</returns>
	const int GetMovementPenalty() const { return m_movePenalty; }

	/// <summary>
	/// Sets the movement penalty of the path point to the passed value.
	/// </summary>
	/// <param name="value">The new movement penalty value</param>
	void SetMovementPenalty(const int value) { m_movePenalty = value; }

	/// <summary>
	/// Retrieves the walkable value of the path point.
	/// </summary>
	/// <returns>The walkable value</returns>
	const bool GetWalkable() const { return m_walkable; }

	/// <summary>
	/// Sets the walkable value of the path point to the passed value.
	/// </summary>
	/// <param name="value">The new walkable value</param>
	void SetWalkable(const bool value) { m_walkable = value; }

	/// <summary>
	/// Retrieves the parent of the path point.
	/// </summary>
	/// <returns>The parent pointer of the path point</returns>
	const PathPoint* GetParent() const { return m_parent; }
	
	/// <summary>
	/// Sets the parent of the path point.
	/// </summary>
	/// <param name="value">The new parent</param>
	void SetParent(const PathPoint* value) { m_parent = value; }

	/// <summary>
	/// Retrieves the G cost of the path point.
	/// </summary>
	/// <returns>The G cost</returns>
	const int GetGCost() const { return m_gCost; }
	
	/// <summary>
	/// Sets the G cost of the path point to the passed value.
	/// </summary>
	/// <param name="value">The new G cost value</param>
	void SetGCost(const int value) { m_gCost = value; }

	/// <summary>
	/// Retrieves the H cost of the path point.
	/// </summary>
	/// <returns>The H cost</returns>
	const int GetHCost() const { return m_hCost; }

	/// <summary>
	/// Sets the H cost of the path point to the passed value.
	/// </summary>
	/// <param name="value">The new H cost value</param>
	void SetHCost(const int value) { m_hCost = value; }

	/// <summary>
	/// Sets the grid coordinate of the path point to the passed coordiante.
	/// </summary>
	/// <param name="coord">The new grid coordinate</param>
	void SetGridCoord(Vec2 coord) { m_gridCoord = coord; }

	/// <summary>
	/// Retrieves the world coordiante of the path point.
	/// </summary>
	/// <returns>The world coordinate</returns>
	const Vec3 GetPosition() const { return m_worldCoord; }

	/// <summary>
	/// Sets the world coordinate of the path point to the passed coordinate.
	/// </summary>
	/// <param name="coord"><The new world coordinate/param>
	void SetPosition(Vec3 coord) { m_worldCoord = coord; }

#pragma endregion Getters/Setters

#pragma region Methods

	/// <summary>
	/// Calculates the F cost of the path point.
	/// </summary>
	/// <returns></returns>
	const int FCost() const { return m_gCost + m_hCost; }

	/// <summary>
	/// Calculates the euclidean distance to the passed path point.
	/// </summary>
	/// <param name="otherNode">The other path point to calculate to</param>
	/// <returns>The euclidean distance between the path points</returns>
	const float DistanceTo(const PathPoint& otherNode);

	/// <summary>
	/// Calculates the manhatten distance to the passed path point.
	/// </summary>
	/// <param name="otherNode">The other path point to calculate to</param>
	/// <returns>The manhatten distance between the path points</returns>
	const float ManhattenDistanceTo(const PathPoint& otherNode);

#pragma endregion Methods

#pragma region Operators

	/// <summary>
	/// Equality operator override.
	/// </summary>
	/// <param name="comparison">The other object to compare to</param>
	/// <returns>Whether they are are equal</returns>
	const bool operator==(const PathPoint& comparison) const;

	/// <summary>
	/// Inequality operator override.
	/// </summary>
	/// <param name="comparison">The other object to compare to</param>
	/// <returns>Whether they are are inequal</returns>
	const bool operator!=(const PathPoint& comparison) const;

	/// <summary>
	/// Less than operator override.
	/// </summary>
	/// <param name="other">The other object to compare to</param>
	/// <returns>Whether the path point is less than the passed path point</returns>
	const bool operator<(const PathPoint& other) const;

	/// <summary>
	/// Greater than operator override.
	/// </summary>
	/// <param name="other">The other object to compare to</param>
	/// <returns>Whether the path point is greater than the passed path point</returns>
	const bool operator>(const PathPoint& other) const;

#pragma endregion Operators

};