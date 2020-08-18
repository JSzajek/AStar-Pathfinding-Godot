#include"pch.h"
#include "grid.hpp"
#include "astarlinker.hpp"

using namespace std;
using namespace astar;

/// <summary>
/// Initializes a new instance of PathNode
/// </summary>
/// <param sizeX>The number of rows of the Grid</param>
/// <param sizeY>The number of columns of the Grid</param>
/// <param _minPenalty>The total minimum penalty within the Grid</param>
/// <param _maxPenalty>The total maximum penalty within the Grid</param>
Grid::Grid(int sizeX, int sizeY, int _minPenalty, int _maxPenalty)
{
	this->gridSizeX = sizeX;
	this->gridSizeY = sizeY;
	this->grid = new Matrix<PathNode*>(gridSizeX, gridSizeY);

	this->minPenalty = _minPenalty;
	this->maxPenalty = _maxPenalty;
}

/// <summary>
/// Grid destructor
/// </summary>
Grid::~Grid() 
{
	if (grid != NULL) {
		clearGrid();
		grid = NULL;
	}
}

/// <summary>
/// Clears the grid
/// </summary>
void Grid::clearGrid() 
{
	for (int i = 0; i < gridSizeX; i++)
	{
		for (int j = 0; j < gridSizeY; j++)
		{
			if (grid->operator()(i, j) != NULL) {
				delete grid->operator()(i, j);
			}
		}
	}
	delete grid;
	grid = NULL;
}

/// <summary>
/// Adds a new PathNode to the grid at specified indices.
/// </summary>
/// <param x>The row index</param>
/// <param y>The column index</param>
/// <param node>The PathNode to be added</param>
void Grid::addToGrid(int x, int y, PathNode* node)
{
	grid->operator()(x,y) = node;
}

/// <summary>
/// Gets the PathNode in the grid at specified indices.
/// </summary>
/// <param x>The row index</param>
/// <param y>The column index</param>
PathNode* Grid::getGridNode(int x, int y)
{
	return grid->operator()(x, y);
}

/// <summary>
/// Gets the neighbors around the PathNode.
/// </summary>
/// <param node>The center PathNode</param>
vector<PathNode*> Grid::getNeighbors(PathNode* node)
{
	vector<PathNode*> neighbors;
	for (int x = -1; x <= 1; x++)
	{
		for (int y = -1; y <= 1; y++)
		{
			if (x == 0 && y == 0)
			{
				continue;
			}
			int checkX = node->getGridX() + x;
			int checkY = node->getGridY() + y;

			if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
			{
				if (grid->operator()(checkX, checkY) != NULL)
				{
					neighbors.push_back(grid->operator()(checkX, checkY));
				}
			}
		}
	}
	return neighbors;
}

/// <summary>
/// Gets the corresponding PathNode to the passed world position.
/// </summary>
/// <param worldPosition>The world position to be evaluated</param>
PathNode* Grid::nodeFromWorldPoint(Vector3 worldPosition)
{
	float percentX = (worldPosition.x + (gridSizeX / 2.0)) / gridSizeX;
	float percentY = (-worldPosition.z + (gridSizeY / 2.0)) / gridSizeY;

	percentX = clamp(percentX, 0, 1);
	percentY = clamp(percentY, 0, 1);
	
	int x = round((gridSizeX - 1) * percentX);
	int y = round((gridSizeY - 1) * percentY);
	
	return grid->operator()(x,y);
}

/// <summary>
/// Blurs the penalty values of the grid.
/// </summary>
/// <param blurSize>The blur kernel size</param>
const std::tuple<int, int> Grid::blurPenaltyMap(int blurSize)
{
	int kernelSize = blurSize * 2 + 1;
	int kernelExtents = (kernelSize - 1) / 2.0;

	Matrix<int> *penaltiesHorizonalPass = new Matrix<int>(gridSizeX, gridSizeY);
	Matrix<int> *penaltiesVerticalPass = new Matrix<int>(gridSizeX, gridSizeY);

	for (int y = 0; y < gridSizeY; y++)
	{
		for (int x = -kernelExtents; x <= kernelExtents; x++)
		{
			int sampleX = clamp(x, 0, kernelExtents);
			penaltiesHorizonalPass->operator()(0, y) += grid->operator()(sampleX, y)->getMovementPenalty();
		}

		for (int x = 1; x < gridSizeX; x++)
		{
			int removeIndex = clamp(x - kernelExtents - 1, 0, gridSizeX);
			int addIndex = clamp(x + kernelExtents, 0, gridSizeX - 1);
			penaltiesHorizonalPass->operator()(x, y) = penaltiesHorizonalPass->operator()(x - 1, y) 
													   - grid->operator()(removeIndex, y)->getMovementPenalty() 
													   + grid->operator()(addIndex, y)->getMovementPenalty();
		}
	}
	for (int x = 0; x < gridSizeX; x++)
	{
		for (int y = -kernelExtents; y <= kernelExtents; y++)
		{
			int sampleY = clamp(y, 0, kernelExtents);
			penaltiesVerticalPass->operator()(x, 0) += penaltiesHorizonalPass->operator()(x, sampleY);
		}
		int blurredPenalty = round(penaltiesVerticalPass->operator()(x, 0) / (float)(kernelSize * kernelSize));
		grid->operator()(x, 0)->setMovementPenalty(blurredPenalty);
		for (int y = 1; y < gridSizeY; y++)
		{
			int removeIndex = clamp(y - kernelExtents - 1, 0, gridSizeY);
			int addIndex = clamp(y + kernelExtents, 0, gridSizeY - 1);

			penaltiesVerticalPass->operator()(x, y) = penaltiesVerticalPass->operator()(x, y - 1) 
													  - penaltiesHorizonalPass->operator()(x, removeIndex) 
													  + penaltiesHorizonalPass->operator()(x, addIndex);
			blurredPenalty = round(penaltiesVerticalPass->operator()(x, y) / (float)(kernelSize * kernelSize));
			grid->operator()(x, y)->setMovementPenalty(blurredPenalty);

			if (blurredPenalty > maxPenalty)
			{
				maxPenalty = blurredPenalty;
			}
			if (blurredPenalty < minPenalty)
			{
				minPenalty = blurredPenalty;
			}
		}
	}
	return std::make_tuple(minPenalty, maxPenalty);
}

/// <summary>
/// Exports the nodes from the grid in a vector collection.
/// </summary>
/// <returns>The nodes of the grid</returns>
const vector<PathNode> Grid::exportGrid()
{
	vector<PathNode> results;
	for (int i = 0; i < gridSizeX; i++)
	{
		for (int j = 0; j < gridSizeY; j++)
		{
			if (grid->operator()(i, j) != NULL) {
				results.push_back(*grid->operator()(i, j));
			}
		}
	}
	return results;
}
