#ifndef MATRIX_HPP
#define MATRIX_HPP

#include <string>
#include <iostream>

namespace std
{
	template <class T>
	class Matrix
	{
	private:
		T** data;
		int columns;
		int rows;
	public:
		// Default Constructor
		Matrix()
		{
			Matrix(0, 0);
		}

		// Constructor with size
		Matrix(int _rows, int _columns)
		{
			rows = _rows;
			columns = _columns;
			data = new T * [rows];

			for (int i = 0; i < rows; i++)
			{
				data[i] = new T[columns];
			}
			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < columns; j++)
				{
					data[i][j] = T();
				}
			}
		}

		// Destructor
		~Matrix() { }

		// Gets to string representation of Matrix
		void toString()
		{
			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < columns; j++)
				{
					cout  << "row " << i << " column " << j << "\t" << data[i][j] << "\n";
				}
			}
		}

		// The index operator getting value at given indices
		T& operator()(const unsigned& _row, const unsigned& _col)
		{
			return this->data[_row][_col];
		}
	};
}

#endif // !MATRIX_HPP
