#include "pch.h"
#include "navmeshlinker.hpp"

using namespace navmeshlinker;

KDTree* NavmeshLinker::tree;

// Destructor
NavmeshLinker::~NavmeshLinker()
{
	tree->clear();
}

// Initializes the kdtree
void NavmeshLinker::setupGrid()
{
	tree = new KDTree(3);
}

// Adds the passed point to the astar grid
void NavmeshLinker::addGridPoint(Vector3 pos)
{
	tree->AddNode(make_tuple(pos, true, 0));
}

// Adds the passed points to the astar grid
void NavmeshLinker::addGridPoints(float* points, int d1, int d2)
{
	deque<tuple<Vector3, bool, int>> deque;
	for (int i = 0; i < d1; i++) {
		deque.push_back(make_tuple(Vector3(points[(i * d2) + 0], points[(i * d2) + 1], points[(i * d2) + 2]), true, 0));
	}

	if (tree->IsEmpty()) {
		tree = new KDTree(deque);
	}
	else {
		while (!deque.empty()) {
			tuple<Vector3, bool, int> val = deque.front();
			tree->AddNode(val);
			deque.pop_front();
		}
	}
}

// Gets the grid point closest to the passed point
float* NavmeshLinker::getGridPoint(Vector3 position)
{
	PathNode* node = tree->Nearest(position);

	if (node == NULL) {
		return new float[5]{ 0,0,0,0,0 };
	}

	std::Vector3 worldPos = node->position;
	return new float[5]{
		worldPos.x,
		worldPos.y,
		worldPos.z,
		(float)node->getMovementPenalty(),
		(float)(node->getWalkable() ? 1 : 0)
	};
}