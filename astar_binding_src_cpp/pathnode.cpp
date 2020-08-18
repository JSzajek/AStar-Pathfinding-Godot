#include"pch.h"
#include "pathnode.hpp"
#include "vector3.hpp"
#include <string>
#include <math.h>
#include "astarlinker.hpp"

using namespace std;
using namespace astar;

/// <summary>
/// Initializes a new instance of PathNode
/// </summary>
/// <param _worldPos>The world position of the node</param>
/// <param _walkable>The walkability of the node</param>
/// <param _movementPenalty>The movement penalty of the node</param>
PathNode::PathNode(std::Vector3 _worldPos, bool _walkable, int _movementPenalty) 
	: walkable(_walkable), movementPenalty(_movementPenalty)
{
	index = -1;
	position = _worldPos;
	axis = 0;
	left = NULL;
	right = NULL;
	hash = std::hash<string>()(this->position.ToString());
}

/// <summary>
/// Initializes a new instance of PathNode
/// </summary>
/// <param _worldPos>The world position of the node</param>
/// <param _gridPos>The grid position of the node</param>
/// <param _walkable>The walkability of the node</param>
/// <param _movementPenalty>The movement penalty of the node</param>
/// <returns></returns>
PathNode::PathNode(std::Vector3 _worldPos, std::Vector2 _gridPos, bool _walkable, int _movementPenalty)
	: PathNode(_worldPos, _walkable, _movementPenalty)
{
	gridX = _gridPos.x;
	gridY = _gridPos.y;
}

/// <summary>
/// Initializes a new instance of PathNode
/// </summary>
/// <param _worldPos>The world position of the node</param>
/// <param _walkable>The walkability of the node</param>
/// <param _movementPenalty>The movement penalty of the node</param>
PathNode::PathNode(std::Vector3 _worldPos, bool _walkable, int _movementPenalty, int _axis) 
	: PathNode(_worldPos, _walkable, _movementPenalty) 
{
	axis = axis;
	left = NULL;
	right = NULL;
}

/// <summary>
/// Initializes a new instance of PathNode
/// </summary>
/// <param _worldPos>The world position of the node</param>
/// <param _walkable>The walkability of the node</param>
/// <param _movementPenalty>The movement penalty of the node</param>
/// <param axis>The axis of the node</param>
/// <param left>The left child of the node</param>
/// <param right>The right child of the node</param>
PathNode::PathNode(std::Vector3 _worldPos, bool _walkable, int _movementPenalty, int _axis, PathNode* left, PathNode* right) :
	PathNode(_worldPos, _walkable, _movementPenalty, _axis) 
{
	this->left = left;
	this->right = right;
}

/// <summary>
/// PathNode destructor
/// </summary>
PathNode::~PathNode() { }

/// <summary>
/// F Cost of the node.
/// </summary>
const int PathNode::fCost()
{
	return this->gCost + this->hCost;
}

// Comparison method
int PathNode::operator<(PathNode* comparison)
{
	int compare = this->fCost() - comparison->fCost();
	return compare == 0 ? this->hCost - comparison->getHCost() : compare;
}

// Equality method
bool PathNode::operator==(PathNode* comparison)
{
	return this->position.Equals(comparison->position);
}

// Inequality method
bool PathNode::operator!=(PathNode* comparison)
{
	return !this->position.Equals(comparison->position);
}

// Equality method for hash
bool PathNode::operator==(const PathNode& other) const
{
	return abs(position.x - other.position.x) < FLT_EPSILON && abs(position.y - other.position.y) < FLT_EPSILON && abs(position.z - other.position.z) < FLT_EPSILON;
}

// Resets the pathnode
void PathNode::CleanUp() 
{
	index = -1;
	hCost = 0;
	gCost = 0;
	parent = NULL;
}

/// <summary>
/// Gets to string representation of PathNode
/// </summary>
string PathNode::ToString()
{
	return "Node " + position.ToString() + "\tMove Penalty " 
				   + to_string(movementPenalty) + "\tWalkable " 
				   + to_string(walkable) + "\tFCost " + to_string(fCost());
}