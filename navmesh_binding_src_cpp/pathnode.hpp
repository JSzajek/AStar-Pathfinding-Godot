#ifndef PATHNODE_HPP
#define PATHNODE_HPP

#include <string>
#include "vector3.hpp"
#include "node.hpp"

using namespace std;

namespace astar
{
	class PathNode : public Node<PathNode>
	{
	private:
		PathNode* parent = NULL;
		int gCost = 0, hCost = 0;
		bool walkable = false;
		int movementPenalty = 0;
	public:
		size_t hash;
		PathNode(std::Vector3 _worldPos, bool _walkable, int _movementPenalty);
		PathNode(std::Vector3 _worldPos, bool _walkable, int _movementPenalty, int axis);
		PathNode(std::Vector3 _worldPos, bool _walkable, int _movementPenalty, int axis, PathNode* left, PathNode* right);
		~PathNode();

		bool getWalkable() { return walkable; }
		void setWalkable(bool _walkable) { walkable = _walkable; }

		PathNode* getParent() { return parent; }
		void setParent(PathNode* _parent) { parent = _parent; }

		int getMovementPenalty() { return movementPenalty; }
		void setMovementPenalty(int _movementPenalty) { movementPenalty = _movementPenalty; }
		int getGCost() { return gCost; }
		void setGCost(int _gCost) { gCost = _gCost; }
		int getHCost() { return hCost; }
		void setHCost(int _hCost) { hCost = _hCost; }

		int fCost();
		virtual int operator<(PathNode* node);
		virtual bool operator==(PathNode* node);
		virtual bool operator!=(PathNode* node);
		virtual void CleanUp();

		bool operator==(const PathNode& p) const;

		string ToString();
	};
}

#endif // !PATHNODE_HPP
