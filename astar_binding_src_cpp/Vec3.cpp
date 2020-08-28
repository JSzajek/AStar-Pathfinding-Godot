#include "pch.h"

#include "Vec3.h"
#include <string>

Vec3::Vec3()
	: x(0), y(0), z(0)
{
}

Vec3::Vec3(float _x, float _y, float _z)
	: x(_x), y(_y), z(_z)
{
}

Vec3::Vec3(const Vec3& other)
	: x(other.x), y(other.y), z(other.z)
{
}

float& Vec3::operator[](int index)
{
	return index == 0 ? x : index == 1 ? y : z;
}

Vec3 Vec3::operator+(const Vec3& other) const
{
	return Vec3(x + other.x, y + other.y, z + other.z);
}

Vec3 Vec3::operator-(const Vec3& other) const
{
	return Vec3(x - other.x, y - other.y, z - other.z);
}

Vec3 Vec3::operator*(const float& scalar) const
{
	return Vec3(x * scalar, y * scalar, z * scalar);
}

float Vec3::operator*(const Vec3& other) const
{
	return Dot(other);
}

bool Vec3::operator==(const Vec3& other) const
{
	return abs(x - other.x) < FLT_EPSILON && 
			abs(y - other.y) < FLT_EPSILON && 
			abs(z - other.z) < FLT_EPSILON;
}

bool Vec3::operator!=(const Vec3& other) const
{
	return !(*this == other);
}

float Vec3::Dot(const Vec3& other) const
{
	return (x * other.x) + (y * other.y) + (z * other.z);
}

float Vec3::DistanceTo(const Vec3& other) const
{
	return sqrtf(powf(other.x - x, 2) + powf(other.y - y, 2) + powf(other.z - z, 2));
}

float Vec3::ManhattenDistanceTo(const Vec3& other) const
{
	return abs(x - other.x) + abs(y - other.y) + abs(z - other.z);
}

float Vec3::Magnitude() const
{
	return sqrtf((x * x) + (y * y) + (z * z));
}

void Vec3::Normalize()
{
	float norm = Magnitude();
	if (norm <= 0) { return; }
	x /= norm;
	y /= norm;
	z /= norm;
}

float Vec3::DirectionTo(const Vec3& other) const
{
	float norm = Magnitude() * other.Magnitude();
	if (norm <= 0) 
	{ 
		return FLT_EPSILON;
	}
	return acosf(Dot(other) / norm);
}

std::string Vec3::ToString() const
{
	return "(" + std::to_string(x) + ", " + std::to_string(y) + ", " + std::to_string(z) + ")";
}

Vec2 Vec3::ToVec2() const
{
	return Vec2(x, z);
}