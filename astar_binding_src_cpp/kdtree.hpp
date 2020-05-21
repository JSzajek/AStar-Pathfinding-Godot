#ifndef KDTREE_HPP
#define KDTREE_HPP

#include <string>
#include <vector>
#include <tuple>
#include <deque>
#include <list>
#include "pathnode.hpp"
#include "vector3.hpp"

using namespace std;

namespace astar
{
	struct ElementComparer
	{
		int index;
		explicit ElementComparer(int index) : index(index) {}

		inline bool operator() (tuple<Vector3, bool, int> a, tuple<Vector3, bool, int> b)
		{
			return get<0>(a)[index] < get<0>(b)[index];
		}
	};

	/// KDTree implementation reference: http://accord-framework.net/docs/html/T_Accord_Collections_KDTree_1.htm
	class KDTree
	{
	private:
		PathNode* root;
		int count;
		int dimensions;
		int leaves;
		PathNode* createRoot(deque<tuple<Vector3, bool, int>> points, int depth, int k, int start, int length, int& leaves);
		void nearest(PathNode* current, Vector3 position, PathNode*& match, float* minDistance);
		void nearest(PathNode* current, Vector3 position, float radius, vector<PathNode*>& list);
		PathNode* deleteNode(PathNode* node, Vector3 position, int depth);
		PathNode* findMinNode(PathNode* node, int axis);
		PathNode* findMin(PathNode* node, int currentAxis, int depth);
		PathNode* minNode(PathNode* x, PathNode* y, PathNode* z, int axis);
		PathNode* insert(PathNode*& node, tuple<Vector3, bool, int> vals, int depth);
		void clearChild(PathNode* node);

		float calcDistance(float x, float y);
		float calcDistance(Vector3 x, Vector3 y);
	public:
		KDTree();
		KDTree(int dimensions);
		KDTree(int dimensions, PathNode* root);
		KDTree(int dimensions, PathNode* root, int count, int leaves);
		KDTree(deque<tuple<Vector3, bool, int>>  points, bool inPlace = false);
		~KDTree();

		void clear();
		bool IsEmpty() { return count == 0; }
		PathNode* CreateRoot(deque<tuple<Vector3, bool, int>> points, bool inPlace, int& leaves);
		PathNode* Nearest(Vector3 position);
		PathNode* Nearest(Vector3 position, float* distance);
		vector<PathNode*> Nearest(Vector3 position, float radius);
		PathNode* AddNode(tuple<Vector3, bool, int> vals);
		void RemoveNode(Vector3 position);

		string ToString();
	};
}

#endif // !KDTREE_HPP