using System;
using System.Linq;

/// <summary>
/// Extension class for Multidimensional Arrays.
/// </summary>
public static class MultidimensionalArrayExtension
{
    // Reference: https://stackoverflow.com/questions/26291609/converting-jagged-array-to-2d-array-c-sharp/51450057#51450057
	/// <summary>
	/// Converts the potential jagged 2 dimensional into a rectangular array 
	/// </summary>
	/// <param name="source">The jagged 2 dimensional array</param>
	/// <typeparam name="T">The type of the array</typeparam>
	/// <returns>Rectangular array of same type</returns>
	public static T[,] To2D<T>(this T[][] source) {
		try {
			var FirstDim = source.Length;
			var SecondDim = source.GroupBy(row => row.Length).Single().Key;

			var result = new T[FirstDim, SecondDim];
			for (var i = 0; i < FirstDim; ++i)
			for (var j = 0; j < SecondDim; ++j)
				result[i, j] = source[i][j];
			return result;
		}
		catch (InvalidOperationException) {
			throw new InvalidOperationException("The given jagged array is not rectangular.");
		}
	}
}