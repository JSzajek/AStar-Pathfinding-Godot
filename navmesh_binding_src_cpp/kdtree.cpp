#include "pch.h"
#include "kdtree.hpp"
#include <algorithm> 
#include <deque>

using namespace std;
using namespace astar;

// Initializes a new instance of KDTree
KDTree::KDTree() : dimensions(3), root(NULL), count(0), leaves(0) {
}

// Initializes a new instance of KDTree
KDTree::KDTree(int dimensions)
	: dimensions(dimensions), root(NULL), count(0), leaves(0) {
}

// Initializes a new instance of KDTree
KDTree::KDTree(int dimensions, PathNode* root)
	: dimensions(dimensions), root(root), count(0), leaves(0) {
}

// Initializes a new instance of KDTree
KDTree::KDTree(int dimensions, PathNode* root, int count, int leaves)
	: dimensions(dimensions), root(root), count(count), leaves(leaves) {
}

// Initializes a new instance of KDTree
KDTree::KDTree(deque<tuple<Vector3, bool, int>> points, bool inPlace)
{
	int leaves = 0;
	PathNode* root = CreateRoot(points, inPlace, leaves);
	this->dimensions = 3;
	this->root = root;
	this->count = points.size();
	this->leaves = leaves;
}

// Destructor
KDTree::~KDTree() {
	clearChild(root);
}

// Recursive Helper method that clears all the children of the node
void KDTree::clearChild(PathNode* node) {
	if (node != NULL) {
		if (node->left != NULL) {
			clearChild(node->left);
		}
		else if (node->right != NULL) {
			clearChild(node->right);
		}
		delete node;
	}
}

// Clears the tree
void KDTree::clear() {
	clearChild(root);
	root = NULL;
}

// Creates a root node of the tree
PathNode* KDTree::CreateRoot(deque<tuple<Vector3, bool, int>> _points, bool inPlace, int& leaves) {
	// Implement not copy of vector
	int dimensions = 3;
	PathNode* _root = createRoot(_points, 0, dimensions, 0, _points.size(), leaves);
	return _root;
}

// Finds the nearest point within the tree to the passed point
PathNode* KDTree::Nearest(Vector3 position)
{
	float distance;
	return Nearest(position, &distance);
}

// Finds the nearest point within the tree the passed
// distance
PathNode* KDTree::Nearest(Vector3 position, float* distance)
{
	PathNode* result = root;
	*distance = calcDistance(root->position, position);
	nearest(root, position, result, distance);
	return result;
}

// Finds the nearest neighbors to the passed position within
// the passed radius distance
vector<PathNode*> KDTree::Nearest(Vector3 position, float radius)
{
	vector<PathNode*> list = vector<PathNode*>();
	if (this->root != NULL) {
		nearest(root, position, radius, list);
	}
	return list;
}

PathNode* KDTree::AddNode(PathNode node) {
	return AddNode(make_tuple(node.position, node.getWalkable(), node.getMovementPenalty()));
}

// Adds the passed point to the tree
PathNode* KDTree::AddNode(tuple<Vector3, bool, int> vals)
{
	count++;
	PathNode* root = this->root;
	PathNode* node = insert(root, vals, 0);
	this->root = root;
	return node;
}

// Removes the passed position from the tree
void KDTree::RemoveNode(Vector3 position)
{
	root = deleteNode(root, position, 0);
}

// Creates a root node from the passed list of points
PathNode* KDTree::createRoot(deque<tuple<Vector3, bool, int>> points, int depth, int k, int start, int length, int& leaves)
{
	if (length <= 0)
	{
		return NULL;
	}
	int axis = depth % k;
	std::sort(points.begin(), points.begin() + length, ElementComparer(axis));

	int half = start + length / 2;
	int leftStart = start;
	int leftLen = half - start;
	int rightStart = half + 1;
	int rightLen = length - length / 2 - 1;

	tuple<Vector3, bool, int> median = points.at(half);
	depth++;
	PathNode* left = createRoot(points, depth, k, leftStart, leftLen, leaves);
	PathNode* right = createRoot(points, depth, k, rightStart, rightLen, leaves);

	if (left == NULL && right == NULL) {
		leaves++;
	}
	return new PathNode(get<0>(median), get<1>(median), get<2>(median), axis, left, right);
}

// Recursive helper method that finds the "nearest" node within the tree that is
// approximately equivalent to the passed position
void KDTree::nearest(PathNode* current, Vector3 position, PathNode*& match, float* minDistance)
{
	float dist = KDTree::calcDistance(position, current->position);

	if (dist < *minDistance) {
		*minDistance = dist;
		match = current;
	}

	float value = position[current->axis];
	float median = current->position[current->axis];
	float u = value - median;
	if (u <= 0) {
		if (current->left != NULL) {
			nearest(current->left, position, match, minDistance);
		}
		if (current->right != NULL && abs(u) <= *minDistance) {
			nearest(current->right, position, match, minDistance);
		}
	}
	else {
		if (current->right != NULL) {
			nearest(current->right, position, match, minDistance);
		}
		if (current->left != NULL && abs(u) <= *minDistance) {
			nearest(current->left, position, match, minDistance);
		}
	}
}

// Recursive helper method that finds the "nearest" nodes within the tree that is
// close to the passed position
void KDTree::nearest(PathNode* current, Vector3 position, float radius, vector<PathNode*>& list)
{
	float dist = KDTree::calcDistance(position, current->position);

	if (dist <= radius) {
		list.push_back(current);
	}

	float value = position[current->axis];
	float median = current->position[current->axis];
	float u = value - median;

	if (u <= 0) {
		if (current->left != NULL) {
			nearest(current->left, position, radius, list);
		}
		if (current->right != NULL && abs(u) <= radius) {
			nearest(current->right, position, radius, list);
		}
	}
	else {
		if (current->right != NULL) {
			nearest(current->right, position, radius, list);
		}
		if (current->left != NULL) {
			nearest(current->left, position, radius, list);
		}
	}
}

// Recursive helper method that deletes the passed position from the passed node
PathNode* KDTree::deleteNode(PathNode* node, Vector3 position, int depth)
{
	if (node == NULL) {
		return NULL;
	}

	int axis = depth % dimensions;
	if (position.Equals(node->position)) {
		if (node->right != NULL) {
			PathNode* min = findMinNode(node->right, axis);
			node->position = *min->position.Clone();
			node->right = deleteNode(node->right, min->position, depth + 1);
		}
		else if (node->left != NULL) {
			PathNode* min = findMinNode(node->left, axis);
			node->position = *min->position.Clone();
			node->left = deleteNode(node->left, min->position, depth + 1);
		}
		else {
			node = NULL;
		}
		return node;
	}

	if (position[axis] < node->position[axis]) {
		node->left = deleteNode(node->left, position, depth + 1);
	}
	else {
		node->right = deleteNode(node->right, position, depth + 1);
	}
	return node;
}

// Finds the minimum node from the passed node
PathNode* KDTree::findMinNode(PathNode* node, int axis)
{
	return findMin(node, axis, 0);
}

// Recursive Helper method to find the minimum node from the passed node on the passed axis
PathNode* KDTree::findMin(PathNode* node, int currentAxis, int depth)
{
	if (node == NULL) {
		return NULL;
	}

	int axis = depth % dimensions;
	if (axis == currentAxis) {
		if (node->left == NULL) {
			return node;
		}
		return findMin(node->left, currentAxis, depth + 1);
	}
	return minNode(node,
		findMin(node->left, currentAxis, depth + 1),
		findMin(node->right, currentAxis, depth + 1), currentAxis);
}

// Helper method to find the minimum node of the passed nodes based on the passed axis
PathNode* KDTree::minNode(PathNode* x, PathNode* y, PathNode* z, int axis)
{
	PathNode* result = x;
	if (y != NULL && y->position[axis] < result->position[axis]) {
		result = y;
	}
	if (z != NULL && z->position[axis] < result->position[axis]) {
		result = z;
	}
	return result;
}

// Recursive helper method that inserts the node at the passed node
PathNode* KDTree::insert(PathNode*& node, tuple<Vector3, bool, int> vals, int depth)
{
	Vector3 position = get<0>(vals);
	if (node == NULL) {
		node = new PathNode(position, get<1>(vals), get<2>(vals), depth % dimensions);
		return node;
	}
	else {
		if (position[node->axis] < node->position[node->axis]) {
			PathNode* child = node->left == NULL ? NULL : node->left;
			PathNode* newNode = insert(child, vals, depth + 1);
			node->left = child;
			return newNode;
		}
		else {
			PathNode* child = node->right == NULL ? NULL : node->right;
			PathNode* newNode = insert(child, vals, depth + 1);
			node->right = child;
			return newNode;
		}
	}
}

// Distance method 
float KDTree::calcDistance(float x, float y)
{
	return abs(x - y);
}

// Distance method 
float KDTree::calcDistance(Vector3 a, Vector3 b)
{
	return a.DistanceTo(b);
}

// ToString method for debugging (BFS)
string KDTree::ToString()
{
	string output = "";
	if (root == NULL) {
		return "Empty Tree";
	}

	// Breadth-First Search 
	list<PathNode*> queue;
	queue.push_back(root);

	while (!queue.empty()) {
		PathNode* current = queue.front();

		if (current != NULL) {
			output += current->position.ToString() + "\n";
			queue.pop_front();

			if (current->left != NULL) {
				queue.push_back(current->left);
			}
			if (current->right != NULL) {
				queue.push_back(current->right);
			}
		}
	}
	return output;
}