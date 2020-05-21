#ifndef GRID_HPP
#define GRID_HPP

#include <map>
#include <vector>
#include <string>
#include "vector3.hpp"
#include "vector2.hpp"
#include "matrix.hpp"
#include "pathnode.hpp"

using namespace std;

namespace astar
{
	class Grid
	{
	private:
		const int obstacleProximityPenalty = 10;
		
		Matrix<PathNode*>* grid;
		std::Vector3 centerPoint = std::Vector3();
		
		int gridSizeX;
		int gridSizeY;
		int minPenalty;
		int maxPenalty;

		map<int, int> walkableRegionDictionary = map<int, int>();
	public:
		Grid(int sizeX, int sizeY, int _minPenalty, int _maxPenalty);
		~Grid();

		void clearGrid();
		void addToGrid(int x, int y, PathNode* node);
		PathNode* getGridNode(int x, int y);

		int getMaxSize() { return gridSizeX * gridSizeY; }
		int logBase(float value, int base) { return (int)log(value) / (int)log(base); }
		vector<PathNode> getNeighbors(PathNode* node);
		PathNode* nodeFromWorldPoint(std::Vector3 worldPosition);
		const std::tuple<int, int> blurPenaltyMap(int blurSize);

		string ToString()
		{
			string str = "";
			for (int i = 0; i < gridSizeX; i++)
			{
				for (int j = 0; j < gridSizeY; j++)
				{
					str += "Node " + getGridNode(i, j)->ToString() + "\n";
				}
			}
			return str;
		}
	};

	inline float clamp(float n, float lower, float upper)
	{
		return n <= lower ? lower : n <= upper ? n : upper;
	}
}

#endif // !GRID_HPP
