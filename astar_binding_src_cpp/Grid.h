#pragma once

#include "pch.h"
#include "Matrix.h"
#include "Vec3.h"
#include "Vec2.h"

/// <summary>
/// Clamps the value between the lower and upper values.
/// </summary>
/// <param name="n">The value to clamp</param>
/// <param name="lower">The lower bound</param>
/// <param name="upper">The upper bound</param>
/// <returns>The clamped value</returns>
inline float clamp(float n, float lower, float upper)
{
	return n <= lower ? lower : n <= upper ? n : upper;
}

/// <summary>
/// Class representing a 2-dimensional grid.
/// </summary>
/// <typeparam name="T">The type of elements within the grid</typeparam>
template<typename T>
class Grid
{
protected:
	Matrix<T> m_matrix;
public:

	/// <summary>
	/// Initializes a new instance of the <see cref="Grid"/> class.
	/// </summary>
	/// <param name="width">The width of the grid</param>
	/// <param name="height">The height of the grid</param>
	Grid(size_t width, size_t height)
		: m_matrix(Matrix<T>(width, height))
	{
	}

	/// <summary>
	/// Retrieves the width of the grid.
	/// </summary>
	/// <returns>The width of the grid</returns>
	const size_t GetWidth() { return m_matrix.GetWidth(); }

	/// <summary>
	/// Retrieves the height of the grid.
	/// </summary>
	/// <returns>The height of the grid</returns>
	const size_t GetHeight() { return m_matrix.GetHeight(); }

	/// <summary>
	/// Retrieves the element within the grid at the passed
	/// row and column index.
	/// </summary>
	/// <param name="row">The row index</param>
	/// <param name="col">The column index</param>
	/// <returns>The reference to the element at the index</returns>
	T& Find(unsigned row, unsigned col)
	{
		return m_matrix(row, col);
	}

	/// <summary>
	/// Retrieves the element within the grid at the passed
	/// row and column index.
	/// </summary>
	/// <param name="row">The row index</param>
	/// <param name="col">The column index</param>
	/// <returns>The reference to the element at the index</returns>
	T& operator()(unsigned int row, unsigned int col)
	{
		return Find(row, col);
	}

	/// <summary>
	/// Retrieves the element within the grid at the estimated
	/// index to the passed world coordiante.
	/// </summary>
	/// <param name="coordinate">The world coordinate</param>
	/// <returns>The reference to the element at the index</returns>
	T& operator()(Vec3 coordinate)
	{
		float xPercent = (coordinate.x + (GetWidth() / 2.0f)) / GetWidth();
		float yPercent = (coordinate.z + (GetHeight() / 2.0f)) / GetHeight();

		xPercent = clamp(xPercent, 0.0f, 1.0f);
		yPercent = clamp(yPercent, 0.0f, 1.0f);

		return Find(std::round((GetWidth() - 1) * xPercent), std::round((GetHeight() - 1) * yPercent));
	}

	/// <summary>
	/// Retrieves a collection of neighbors to the passed grid
	/// row and column index.
	/// </summary>
	/// <param name="row">The row index</param>
	/// <param name="col">The column index</param>
	/// <returns>A collection of neighbors surrounding the element</returns>
	std::vector<T> GetNeighbors(unsigned int row, unsigned int col)
	{
		std::vector<T> neighbors;
		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				if (x == 0 && y == 0) { continue; }

				int xCheck = row + x;
				int yCheck = col + y;

				if (xCheck >= 0 && xCheck < GetWidth() && yCheck >= 0 && yCheck < GetHeight())
				{
					neighbors.push_back(m_matrix(xCheck, yCheck));
				}
			}
		}
		return neighbors;
	}

	/// <summary>
	/// Retrieves all of the elements within the grid.
	/// </summary>
	/// <returns>The collection of elements within the grid</returns>
	const std::vector<T> Export()
	{
		std::vector<T> results;
		for (unsigned int i = 0; i < GetWidth(); i++)
		{
			for (unsigned int j = 0; j < GetHeight(); j++)
			{
				results.push_back(m_matrix(i, j));
			}
		}
		return results;
	}

	/// <summary>
	/// Overloads the stream operator retrieving the grid's string.
	/// </summary>
	/// <param name="stream">The stream to append to</param>
	/// <param name="other">The grid to convert</param>
	/// <returns>The modified stream</returns>
	friend std::ostream& operator<<(std::ostream& stream, const Grid& other)
	{
		stream << other.m_matrix;
		return stream;
	}
};