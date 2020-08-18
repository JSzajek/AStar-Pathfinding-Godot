#ifndef PATHNODE_HPP
#define PATHNODE_HPP

#include <string>
#include "vector3.hpp"
#include "minheap.hpp"
#include "vector2.hpp"
#include "node.hpp"

using namespace std;

namespace astar
{
	class PathNode : public Node<PathNode>
	{
	private:
		PathNode* parent = NULL;
		int gCost;
		int hCost;
		int gridX;
		int gridY;
		bool walkable;
		int movementPenalty;
	public:
		size_t hash;
		PathNode(std::Vector3 _worldPos, bool _walkable, int _movementPenalty);
		PathNode(std::Vector3 _worldPos, std::Vector2 _gridPos, bool _walkable, int _movementPenalty);
		PathNode(std::Vector3 _worldPos, bool _walkable, int _movementPenalty, int axis);
		PathNode(std::Vector3 _worldPos, bool _walkable, int _movementPenalty, int axis, PathNode* left, PathNode* right);
		~PathNode();

		const bool getWalkable() { return walkable; }
		void setWalkable(bool _walkable) { walkable = _walkable; }

		const PathNode* getParent() { return parent; }
		void setParent(PathNode* _parent) { parent = _parent; }

		const int getGridX() { return gridX; }
		const int getGridY() { return gridY; }

		const int getMovementPenalty() { return movementPenalty; }
		void setMovementPenalty(int _movementPenalty) { movementPenalty = _movementPenalty; }
		
		const int getGCost() { return gCost; }
		void setGCost(int _gCost) { gCost = _gCost; }
		
		const int getHCost() { return hCost; }
		void setHCost(int _hCost) { hCost = _hCost; }

		const int fCost();

		/// <summary>
		/// Gets the euclidean distance between two PathNodes.
		/// </summary>
		/// <param otherNode>The other node</param>
		const float DistanceTo(const PathNode& otherNode)
		{
			return ceil(position.DistanceTo(otherNode.position));
		}

		/// <summary>
		/// Gets the manhatten distance between two PathNodes.
		/// </summary>
		/// <param otherNode>The other node</param>
		const float ManhattenDistanceTo(const PathNode& otherNode)
		{
			return ceil(position.ManhattenDistanceTo(otherNode.position));
		}

		virtual int operator<(PathNode* node);
		virtual bool operator==(PathNode* node);
		virtual bool operator!=(PathNode* node);
		virtual void CleanUp();

		bool operator==(const PathNode& p) const;

		string ToString();
	};
}

#endif // !PATHNODE_HPP
