#ifndef LINE_HPP
#define LINE_HPP

#include <math.h>
#include "vector2.hpp"

using namespace std;

namespace astar
{
	class Line
	{
	private:
		const float VERT_LINE_GRAD = pow(10, 5);
		float gradient;
		float yIntercept;
		std::Vector2 pointOnLine1;
		std::Vector2 pointOnLine2;
		float perpendicularGradient;
		bool approachSide;
	public:
		Line();
		Line(std::Vector2 _pointOnLine, std::Vector2 _pointPerpToLine);
		~Line();
		bool getSide(std::Vector2 point);
		bool hasCrossedLine(std::Vector2 point);
		float distanceFromPoint(std::Vector2 point);

		float getYIntercept() { return yIntercept; }
		bool getApproachSide() { return approachSide; }
		float getGradient() { return gradient; }
		float getPerpGradient() { return perpendicularGradient; }
		std::Vector2 getPointOnLine1() { return pointOnLine1; }
		std::Vector2 getPointOnLine2() { return pointOnLine2; }
	};
}

#endif // !LINE_HPP
