using Godot;

namespace Triangulation
{
    /// <summary>
    /// Struct representing a basic triangle comprised
    /// of three vertices.
    /// </summary>
    public struct Triangle
    {
        #region Constructors

        /// <summary>
	    /// Initializes a new instance of the <see cref="Triangle"/> class.
	    /// </summary>
        /// <param name="a">The first vertex of the triangle</param>
        /// <param name="b">The second vertex of the triangle</param>
        /// <param name="c">The third vertex of the triangle</param>
        public Triangle(Vertex a, Vertex b, Vertex c)
        {
            A = a;
            B = b;
            C = c;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the first vertex of the triangle.
        /// </summary>
        public Vertex A { get; }

        /// <summary>
        /// Gets the second vertex of the triangle.
        /// </summary>
        public Vertex B { get; }

        /// <summary>
        /// Gets the third vertex of the triangle.
        /// </summary>
        public Vertex C { get; }

        #endregion Properties

        #region Static Methods

        /// <summary>
        /// Checks whether the triangle comprised of the passed three points will
        /// contain the passed point
        /// </summary>
        /// <param name="a">The first vertex of the triangle</param>
        /// <param name="b">The second vertex of the triangle</param>
        /// <param name="c">The third vertex of the triangle</param>
        /// <param name="point">The point to check</param>
        /// <returns>Whether the point is within the triangle</returns>
        public static bool ContainsPoint(Vertex a, Vertex b, Vertex c, Vertex point)
        {
            return new Triangle(a, b, c).ContainsPoint(point);
        }

        /// <summary>
        /// Checks whether the point lies on the segment comprised of two vertices
        /// </summary>
        /// <param name="sA">The first vertex of the segment</param>
        /// <param name="sB">The second vertex of the segment</param>
        /// <param name="point">The point to check</param>
        /// <returns>Whether the point is on the segment</returns>
        static bool CheckPointToSegment(Vertex sA, Vertex sB, Vertex point)
        {
            if ((sA.Position.z < point.Position.z && sB.Position.z >= point.Position.z) ||
                (sB.Position.z < point.Position.z && sA.Position.z >= point.Position.z))
            {
                float x =
                    sA.Position.x +
                    (point.Position.z - sA.Position.z) /
                    (sB.Position.z - sA.Position.z) *
                    (sB.Position.x - sA.Position.x);

                if (x < point.Position.x)
                    return true;
            }

            return false;
        }

        #endregion Static Methods

        #region Public Methods

        /// <summary>
        /// Retrieves the vertices of the triangles packed in a tuple.
        /// </summary>
        /// <return>The packed tuple of vertices</return>
        public (Vector3, Vector3, Vector3) GetVertices()
        {
            return (A.Position, B.Position, C.Position);
        }

        /// <summary>
        /// Retrieves the centriod of the triangle
        /// </summary>
        /// <returns>The centriod</returns>
        public Vector3 GetCentriod()
        {
            return (A.Position + B.Position + C.Position) / 3f;
        }

        /// <summary>
        /// Retrieves the midpoints of the triangles packed in a tuple.
        /// </summary>
        /// <return>The packed tuple of midpoints</return>
        public (Vector3, Vector3, Vector3) GetMidPoints()
        {
            return (
                (A.Position + B.Position) / 2f,
                (B.Position + C.Position) / 2f,
                (C.Position + A.Position) / 2f
            );
        }

        /// <summary>
        /// Retrieves the area of the triangle
        /// </summary>
        /// <returns>The area</returns>
        public float GetArea()
        {
            (var p1, var p2, var p3) = GetVertices();
            return Mathf.Abs((p1.x * (p2.z - p3.z) + p2.x * (p3.z - p2.z) + p3.x * (p1.z - p2.z)) / 2f);
        }

        /// <summary>
        /// Checks whether the triangle contains the passed point
        /// </summary>
        /// <param name="point">The point to check</param>
        /// <returns>Whether the point is within the triangle</returns>
        public bool ContainsPoint(Vertex point)
        {
            //return true if the point to test is one of the vertices
            if (point.Equals(A) || point.Equals(B) || point.Equals(C))
                return true;

            bool oddNodes = false;

            if (CheckPointToSegment(C, A, point))
                oddNodes = !oddNodes;
            if (CheckPointToSegment(A, B, point))
                oddNodes = !oddNodes;
            if (CheckPointToSegment(B, C, point))
                oddNodes = !oddNodes;

            return oddNodes;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is Triangle triangle)
            {
                return triangle.A.Equals(A) && triangle.B.Equals(B) && triangle.C.Equals(C);
            }
            return false;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = A.GetHashCode();
                result = (result * 397) ^ B.GetHashCode();
                result = (result * 397) ^ C.GetHashCode();
                return result;
            }
        }

        #endregion Public Methods
    }
}
