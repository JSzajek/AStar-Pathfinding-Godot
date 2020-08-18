#ifndef NODE_HPP
#define NODE_HPP

#include "Indexable.hpp"
#include "vector3.hpp"

using namespace std;

// Class representing a node
template <class T>
class Node : public Indexable
{
public:
	Vector3 position;
	int axis;
	T* left;
	T* right;

	// Initializes a new instance of Node
	Node() : axis(0), position(Vector3(0,0,0)), left(NULL), right(NULL) { }

	// Initializes a new instance of Node
	Node(int axis, Vector3 position) : 
		axis(axis), position(position), left(NULL), right(NULL) {
	}
	
	// Initializes a new instance of Node
	Node(int axis, Vector3 position, T* left, T* right) :
		axis(axis), position(position), left(left), right(right) {
	}

	// Clean up method
	virtual void CleanUp() {
	}

	// Comparison method
	virtual int operator<(T* comparison)
	{
		return (int)(this->position[axis] - comparison->position[axis]);
	}

	// Equality method
	virtual bool operator==(T* comparison)
	{
		return this->position[axis] == comparison->position[axis];
	}

	// Inequality method
	virtual bool operator!=(T* comparison)
	{
		return this->position[axis] != comparison->position[axis];
	}
};

#endif