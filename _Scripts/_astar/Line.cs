using Godot;

namespace AStar
{
    /// <summary>
    /// Struct representing a line and related methods
    /// </summary>
    public struct Line
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Line"/> class.
        /// </summary>
        /// <param name="gradient">The gradient of the line</param>
        /// <param name="perpGradient">The perpendicular gradient of the line</param>
        /// <param name="point1">The point on the line</param>
        /// <param name="point2">The point perpendicular to the line</param>
        /// <param name="approachSide">The current approach side</param>
        public Line(float gradient, float perpGradient, Vector2 point1, Vector2 point2, int approachSide)
        {
            Gradient = gradient;
            PerpGradient = perpGradient;
            PointOnLine = point1;
            PointPerpToLine = point2;
            ApproachSide = approachSide == 1;
            YIntercept = point1.y - gradient * point1.x;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the gradient of the line.
        /// </summary>
        public float Gradient { get; }

        /// <summary>
        /// Gets the y intercept of the line.
        /// </summary>
        public float YIntercept { get; }

        /// <summary>
        /// Gets the perpendicular gradient of the line.
        /// </summary>
        public float PerpGradient { get; }

        /// <summary>
        /// Gets the point on the line.
        /// </summary>
        public Vector2 PointOnLine { get; }

        /// <summary>
        /// Gets the point perpendicular to the line.
        /// </summary>
        public Vector2 PointPerpToLine { get; }

        /// <summary>
        /// Gets the approach side, where true represents front
        /// and false back.
        /// </summary>
        public bool ApproachSide { get; }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Getter method that gets the side of the passed point.
        /// </summary>
        /// <param name="point">The passed point</param>
        /// <returns>The side the point represented as a bool (infront or behind)</returns>
        public bool GetSide(Vector2 point)
        {
            return (point.x - PointOnLine.x) * (PointPerpToLine.y - PointOnLine.y) > (point.y - PointOnLine.y) * (PointPerpToLine.x - PointOnLine.x);
        }

        /// <summary>
        /// Determines whether the passed point has crossed the line.
        /// </summary>
        /// <param name="point">The passed point</param>
        /// <returns>boolean of whether the point has crossed the line</returns>
        public bool HasCrossedLine(Vector2 point)
        {
            return GetSide(point) != ApproachSide;
        }

        /// <summary>
        /// Gets the distance of the line to the passed point.
        /// </summary>
        /// <param name="point">The passed point</param>
        /// <returns>The distance of the line to the point</returns>
        public float DistanceFromPoint(Vector2 point)
        {
            float yInterceptPerp = point.y - PerpGradient * point.x;
            float xIntersect = (yInterceptPerp - YIntercept) / (Gradient - PerpGradient);
            float yIntersect = Gradient * xIntersect + YIntercept;
            return point.DistanceTo(new Vector2(xIntersect, yIntersect));
        }

        #endregion Public Methods
    }
}