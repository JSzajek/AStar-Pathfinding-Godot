#pragma once

#include "pch.h"

template<typename T>
class Matrix
{
private:
	size_t m_width, m_height;
	std::vector<T> m_data2;
	//T* m_data;
public:

	Matrix(size_t width, size_t height)
		: m_width(width), m_height(height), m_data2(std::vector<T>())
		//m_data(static_cast<T*>(operator new[](width* height * sizeof(T))))
	{
		m_data2.reserve(width * height);
		for (int i = 0; i < width * height; i++)
		{
			m_data2.emplace_back(T());
		}
	}

	~Matrix()
	{
		//delete[] m_data;
	}

	const size_t const GetWidth() { return m_width; }

	const size_t const GetHeight() { return m_height; }

	T& operator()(const size_t row, const size_t col)
	{
		return m_data2[row + (col * m_width)];
	}

	friend std::ostream& operator<<(std::ostream& stream, const Matrix& other)
	{
		stream << "Matrix of Row: " << other.m_width << " Column: " << other.m_height << std::endl;
		return stream;
	}
};