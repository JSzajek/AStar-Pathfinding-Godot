#pragma once

#include "pch.h"
#include "Matrix.h"
#include "Vec3.h"
#include "Vec2.h"

inline float clamp(float n, float lower, float upper)
{
	return n <= lower ? lower : n <= upper ? n : upper;
}

template<typename T>
class Grid
{
protected:
	Matrix<T> m_matrix;
public:
	Grid(size_t width, size_t height)
		: m_matrix(Matrix<T>(width, height))
	{
	}

	const size_t GetWidth() { return m_matrix.GetWidth(); }
	const size_t GetHeight() { return m_matrix.GetHeight(); }
	const size_t GetMaxSize() const { return GetWidth() * GetHeight(); }

	T& Find(unsigned row, unsigned col)
	{
		return m_matrix(row, col);
	}

	T& operator()(unsigned int row, unsigned int col)
	{
		return Find(row, col);
	}

	T& operator()(Vec3 coordinate)
	{
		float xPercent = (coordinate.x + (GetWidth() / 2.0f)) / GetWidth();
		float yPercent = (coordinate.z + (GetHeight() / 2.0f)) / GetHeight();

		xPercent = clamp(xPercent, 0.0f, 1.0f);
		yPercent = clamp(yPercent, 0.0f, 1.0f);

		return Find(std::round((GetWidth() - 1) * xPercent), std::round((GetHeight() - 1) * yPercent));
	}

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

	friend std::ostream& operator<<(std::ostream& stream, const Grid& other)
	{
		stream << other.m_matrix;
		return stream;
	}
};