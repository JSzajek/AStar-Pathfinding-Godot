#include"pch.h"
#include "vector3.hpp"
#include <math.h>

using namespace std;

/// <summary>
/// Initializes a new default instance of Vector3
/// </summary>
Vector3::Vector3() 
{
	this->x = this->y = this->z = 0;
}

/// <summary>
/// Initializes a new instance of Vector3
/// </summary>
/// <param _x>The x value</param>
/// <param _y>The y value</param>
/// <param _z>The z value</param>
Vector3::Vector3(float _x, float _y, float _z)
{
	this->x = _x;
	this->y = _y;
	this->z = _z;
}

/// <summary>
/// Vector3 destructor
/// </summary>
Vector3::~Vector3() { }

/// <summary>
/// Bracket operator for Vector3 getting the relevent index
/// </summary>
float Vector3::operator[](int i)
{
	if (i == 0) {
		return x;
	}
	if (i == 1) {
		return y;
	}
	else 
	{
		return z;
	}
}

/// <summary>
/// Addition operator
/// </summary>
/// <param vec>the vector to be added</param>
Vector3& Vector3::operator+(Vector3 vec)
{
	return *new Vector3(this->x + vec.x, this->y + vec.y, this->z + vec.z);
}

/// <summary>
/// Subtraction operator
/// </summary>
/// <param vec>the vector to be subtracted</param>
Vector3& Vector3::operator-(Vector3 vec)
{
	return *new Vector3(this->x - vec.x, this->y - vec.y, this->z - vec.z);
}

/// <summary>
/// Scalar Multiplication operator
/// </summary>
/// <param scalar>The scalar to multiply by</param>
Vector3& Vector3::operator*(float scalar)
{
	return *new Vector3(this->x * scalar, this->y * scalar, this->z * scalar);
}

/// <summary>
/// Scalar Division operator
/// </summary>
/// <param name="scalar">The scalar to divide by</param>
/// <returns></returns>
Vector3& Vector3::operator/(float scalar)
{
	if (scalar == 0) {
		return *new Vector3(INFINITY, INFINITY, INFINITY);
	}
	return *new Vector3(this->x / scalar, this->y / scalar, this->z / scalar);
}

/// <summary>
/// Gets the Eucledian Distance to the Vector3
/// </summary>
/// <param vec>The vector to</param>
float Vector3::DistanceTo(Vector3 vec)
{
	return sqrtf(powf(vec.x - this->x, 2) + powf(vec.y - this->y, 2) + powf(vec.z - this->z, 2));
}

/// <summary>
/// Gets the Manhatten Distance to the Vector3
/// </summary>
/// <param vec>the vector to</param>
float Vector3::ManhattenDistanceTo(Vector3 vec)
{
	return abs(this->x - vec.x) + abs(this->y - vec.y) + abs(this->z - vec.z);
}

/// <summary>
/// Gets the Magnitude
/// </summary>
float Vector3::Magnitude()
{
	return sqrtf(pow(this->x, 2) + pow(this->y, 2) + pow(this->z, 2));
}

/// <summary>
/// Gets the Sqr Magnitude with the implication that
/// the vector is confined to 2d space
/// </summary>
float Vector3::SqrMagnitude2D() 
{
	return (this->x * this->x) + (this->z * this->z);
}

/// <summary>
/// Gets Normalized Vector3
/// </summary>
Vector3& Vector3::Normalize()
{
	float norm = Magnitude();
	if (norm < 0) {
		return *this;
	}
	return *new Vector3(this->x / norm, this->y / norm, this->z / norm);
}

/// <summary>
/// Gets the dot product
/// </summary>
/// <param vec>the vector to be multiplied</param>
float Vector3::Dot(Vector3 vec)
{
	return (this->x * vec.x) + (this->y * vec.y) + (this->z * vec.z);
}

/// <summary>
/// Gets direction to the passed Vector3
/// </summary>
/// <param vec>direction to the passed Vector3</param>
float Vector3::DirectionTo(Vector3 vec)
{
	return acosf(Dot(vec) / Magnitude() * vec.Magnitude());
}

/// <summary>
/// Equality of the Vector3
/// </summary>
/// <param vec>the vector to be evaluted</param>
bool Vector3::Equals(Vector3 vec)
{
	return abs(this->x - vec.x) < FLT_EPSILON && abs(this->y - vec.y) < FLT_EPSILON && abs(this->z - vec.z) < FLT_EPSILON;
}

/// <summary>
/// Equality of the Vector3
/// </summary>
/// <param vec>the vector to be evaluted</param>
bool Vector3::operator==(Vector3 vec)
{
	return Equals(vec);
}

/// <summary>
/// Constant equality operator for Vector3
/// </summary>
/// <param name="vec">The vector to be evaluated</param>
bool Vector3::operator==(const Vector3& vec) const
{
	return abs(this->x - vec.x) < FLT_EPSILON && abs(this->y - vec.y) < FLT_EPSILON && abs(this->z - vec.z) < FLT_EPSILON;
}

/// <summary>
/// The compressed summation operator, adding the passed
/// vector3.
/// </summary>
/// <param name="vec">The vector to be added</param>
Vector3& Vector3::operator+=(Vector3 vec)
{
	this->x += vec.x;
	this->y += vec.y;
	this->z += vec.z;
	return *this;
}

/// <summary>
/// Less than operator for Vector3
/// </summary>
/// <param name="vec">The vector to be evaluted</param>
bool Vector3::operator<(const Vector3& vec) const
{
	return sqrtf(powf(this->x, 2) + powf(this->y, 2) + powf(this->z, 2)) < sqrtf(powf(vec.x, 2) + powf(vec.y, 2) + powf(vec.z, 2));
}

/// <summary>
/// Greater than operator for Vector3
/// </summary>
/// <param name="vec">The vector to be evaluted</param>
bool Vector3::operator>(const Vector3& vec) const
{
	return sqrtf(powf(this->x, 2) + powf(this->y, 2) + powf(this->z, 2)) > sqrtf(powf(vec.x, 2) + powf(vec.y, 2) + powf(vec.z, 2));
}

/// <summary>
/// Clones the Vector3
/// </summary>
/// <param vec>Clone Vector3</param>
Vector3* Vector3::Clone()
{
	return new Vector3(x, y, z);
}

/// <summary>
/// Gets the string representation of the Vector3
/// </summary>
string Vector3::ToString()
{
	return "(" + to_string(this->x) + ", " + to_string(this->y) + ", " + to_string(this->z) + ")";
}