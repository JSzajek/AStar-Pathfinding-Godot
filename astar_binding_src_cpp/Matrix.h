#pragma once

#include "pch.h"

/// <summary>
/// Class representing a dynamic matrix containing
/// templated objects.
/// </summary>
/// <typeparam name="T">The type of objects</typeparam>
template<typename T>
class Matrix
{
private:
	size_t m_width, m_height;
	std::vector<T> m_data;
public:

	/// <summary>
	/// Initializes a new instance of the <see cref="Matrix"/> class.
	/// </summary>
	/// <param name="width">The width of the matrix</param>
	/// <param name="height">The height of the matrix</param>
	Matrix(size_t width, size_t height)
		: m_width(width), m_height(height), m_data(std::vector<T>())
	{
		m_data.reserve(width * height);
		for (int i = 0; i < width * height; i++)
		{
			m_data.emplace_back(T());
		}
	}

	/// <summary>
	/// Retrieves the width of the matrix.
	/// </summary>
	/// <returns>The width of the matrix</returns>
	const size_t const GetWidth() { return m_width; }

	/// <summary>
	/// Retrieves the height of the matrix.
	/// </summary>
	/// <returns>The height of the matrix</returns>
	const size_t const GetHeight() { return m_height; }

	/// <summary>
	/// Retrieves the element within the matrix at the passed
	/// row and column index.
	/// </summary>
	/// <param name="row">The row index</param>
	/// <param name="col">The column index</param>
	/// <returns>The reference to the element at the index</returns>
	T& operator()(const size_t row, const size_t col)
	{
		return m_data[row + (col * m_width)];
	}

	/// <summary>
	/// Overloads the stream operator retrieving the matrix's string.
	/// </summary>
	/// <param name="stream">The stream to append to</param>
	/// <param name="other">The matrix to convert</param>
	/// <returns>The modified stream</returns>
	friend std::ostream& operator<<(std::ostream& stream, const Matrix& other)
	{
		stream << "Matrix of Row: " << other.m_width << " Column: " << other.m_height << std::endl;
		return stream;
	}
};