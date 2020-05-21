#include"pch.h"
#include "vector2.hpp"
#include <math.h>
#include <string>

using namespace std;

/// <summary>
/// Initializes a new default instance of Vector2
/// </summary>
Vector2::Vector2()
{
	this->x = this->y = 0;
}

/// <summary>
/// Initializes a new instance of Vector2
/// </summary>
/// <param _x>The x value</param>
/// <param _y>The y value</param>
Vector2::Vector2(float _x, float _y)
{
	this->x = _x;
	this->y = _y;
}

/// <summary>
/// Vector2 destructor
/// </summary>
Vector2::~Vector2() { }

/// <summary>
/// Addition operator
/// </summary>
/// <param vec>the vector to be added</param>
Vector2& Vector2::operator+(Vector2 vec)
{
	/*this->x += vec.x;
	this->y += vec.y;*/
	return *new Vector2(this->x + vec.x, this->y + vec.y);
}

/// <summary>
/// Subtraction operator
/// </summary>
/// <param vec>the vector to be subtracted</param>
Vector2& Vector2::operator-(Vector2 vec)
{
	return *new Vector2(this->x - vec.x, this->y - vec.y);
}

/// <summary>
/// Scalar Multiplication operator
/// </summary>
/// <param scalar>The scalar to multiply by</param>
Vector2& Vector2::operator*(float scalar)
{
	return *new Vector2(this->x * scalar, this->y*scalar);
}

/// <summary>
/// Gets the Distance to Vector2
/// </summary>
/// <param vec>The vector to be evaluated</param>
float Vector2::DistanceTo(Vector2 vec)
{
	return sqrtf(powf(vec.x - this->x, 2) + powf(vec.y - this->y, 2));
}

/// <summary>
/// Gets the Magnitude
/// </summary>
float Vector2::Magnitude()
{
	return sqrtf((this->x*this->x) + (this->y*this->y));
}

/// <summary>
/// Gets Normalized Vector3
/// </summary>
Vector2& Vector2::Normalize()
{
	float norm = Magnitude();
	if (norm < 0) {
		return *this;
	}
	return *new Vector2(this->x / norm, this->y / norm);
}

/// <summary>
/// Gets the dot product
/// </summary>
/// <param vec>the vector to be multiplied</param>
float Vector2::Dot(Vector2 vec)
{
	return (this->x * vec.x) + (this->y * vec.y);
}

/// <summary>
/// Equality of the Vector2
/// </summary>
/// <param vec>the vector to be evaluted</param>
bool Vector2::Equals(Vector2 vec)
{
	return abs(this->x - vec.x) < FLT_EPSILON && abs(this->y - vec.y) < FLT_EPSILON;
}

/// <summary>
/// Gets the string representation of the Vector2
/// </summary>
string Vector2::ToString()
{
	return "(" + to_string(this->x) + ", " + to_string(this->y) + ")";
}