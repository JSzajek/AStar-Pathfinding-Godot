#include "pch.h"

#include "Vec2.h"

Vec2::Vec2()
	: x(0), y(0)
{
}

Vec2::Vec2(float _x, float _y)
	: x(_x), y(_y)
{
}

Vec2::Vec2(const Vec2& other)
	: x(other.x), y(other.y)
{
}

float& Vec2::operator[](int index)
{
	return index == 0 ? x : y;
}

Vec2 Vec2::Vec2::operator+(const Vec2& other) const
{
	return Vec2(x + other.x, y + other.y);
}

Vec2 Vec2::operator-(const Vec2& other) const
{
	return Vec2(x - other.x, y - other.y);
}

Vec2 Vec2::operator*(const float& scalar) const
{
	return Vec2(x * scalar, y * scalar);
}

float Vec2::operator*(const Vec2& other) const
{
	return Dot(other);
}

bool Vec2::operator==(const Vec2& other) const
{
	return abs(x - other.x) < FLT_EPSILON &&
		abs(y - other.y) < FLT_EPSILON;
}

bool Vec2::operator!=(const Vec2& other) const
{
	return !(*this == other);
}

float Vec2::Dot(const Vec2& other) const
{
	return (x * other.x) + (y * other.y);
}

float Vec2::DistanceTo(const Vec2& other) const
{
	return sqrtf(powf(other.x - x, 2) + powf(other.y - y, 2));
}

float Vec2::ManhattenDistanceTo(const Vec2& other) const
{
	return abs(x - other.x) + abs(y - other.y);
}

float Vec2::Magnitude() const
{
	return sqrtf((x * x) + (y * y));
}

void Vec2::Normalize()
{
	float norm = Magnitude();
	if (norm <= 0) { return; }
	x /= norm;
	y /= norm;
}

float Vec2::DirectionTo(const Vec2& other)
{
	float norm = Magnitude() * other.Magnitude();
	if (norm <= 0)
	{
		return FLT_EPSILON;
	}
	return acosf(Dot(other) / norm);
}