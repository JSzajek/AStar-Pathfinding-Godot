using Godot;

/// <summary>
/// Struct representing a triangle
/// </summary>
public struct Triangle {
    public readonly Vertex A;
    public readonly Vertex B;
    public readonly Vertex C;

    /// <summary>
    /// Constructor initializing a triangle
    /// </summary>
    /// <param name="a">The first vertex</param>
    /// <param name="b">The second vertex</param>
    /// <param name="c">The third vertex</param>
    public Triangle(Vertex a, Vertex b, Vertex c) {
        this.A = a;
        this.B = b;
        this.C = c;
    }

    /// <summary>
    /// Getter method to get all three vertices
    /// </summary>
    /// <returns>Tuple of all three vertices</returns>
    public (Vector3, Vector3, Vector3) GetVertices() {
        return (A.getWorldPosition(), B.getWorldPosition(), C.getWorldPosition());
    }

    /// <summary>
    /// Getter method to the centriod of the triangle
    /// </summary>
    /// <returns>The centriod of the triangle</returns>
    public Vector3 GetCentriod() {
        return (A.getWorldPosition() + B.getWorldPosition() + C.getWorldPosition()) / 3f;
    }

    /// <summary>
    /// Getter method to get all three midpoints of each edge of the triangle
    /// </summary>
    /// <returns>Tuple of all three midpoints</returns>
    public (Vector3, Vector3, Vector3) GetMidPoints() {
        return (
            (A.getWorldPosition() + B.getWorldPosition()) / 2f,
            (B.getWorldPosition() + C.getWorldPosition()) / 2f,
            (C.getWorldPosition() + A.getWorldPosition()) / 2f
        );
    }

    /// <summary>
    /// Getter method that returns the area of the triangle.
    /// </summary>
    /// <returns>The area of the triangle</returns>
    public float GetArea() {
        (var p1, var p2, var p3) = GetVertices();
        return Mathf.Abs((p1.x * (p2.z - p3.z) + p2.x * (p3.z - p2.z) + p3.x * (p1.z - p2.z)) / 2f);
    }

    /// <summary>
    /// Determines if the point lies within the triangle
    /// </summary>
    /// <param name="point">The point to check</param>
    /// <returns>Whether it lies in the triangle</returns>
    public bool ContainsPoint(Vertex point)
    {
        //return true if the point to test is one of the vertices
        if (point.Equals(A) || point.Equals(B) || point.Equals(C))
            return true;

        bool oddNodes = false;

        if (checkPointToSegment(C, A, point))
            oddNodes = !oddNodes;
        if (checkPointToSegment(A, B, point))
            oddNodes = !oddNodes;
        if (checkPointToSegment(B, C, point))
            oddNodes = !oddNodes;

        return oddNodes;
    }

    /// <summary>
    /// Static helper method that checks whether the last point lies within the
    /// theoretical triangle made by first three points.
    /// </summary>
    /// <param name="a">The first vertex of the triangle</param>
    /// <param name="b">The second vertex of the triangle</param>
    /// <param name="c">The third vertex of the triangle</param>
    /// <param name="point">The point to check</param>
    /// <returns>Whether the point lies within the triangle</returns>
    public static bool ContainsPoint(Vertex a, Vertex b, Vertex c, Vertex point)
    {
        return new Triangle(a, b, c).ContainsPoint(point);
    }

    /// <summary>
    /// Static helper method that checks if the point is on the segment formed
    /// by first two vertices
    /// </summary>
    /// <param name="sA">The first vertex of the segment</param>
    /// <param name="sB">The second vertex of the segment</param>
    /// <param name="point">The point to check</param>
    /// <returns>Whether the point is on the segment</returns>
    static bool checkPointToSegment(Vertex sA, Vertex sB, Vertex point)
    {
        if ((sA.Position.y < point.Position.y && sB.Position.y >= point.Position.y) ||
            (sB.Position.y < point.Position.y && sA.Position.y >= point.Position.y))
        {
            float x = 
                sA.Position.x + 
                (point.Position.y - sA.Position.y) / 
                (sB.Position.y - sA.Position.y) * 
                (sB.Position.x - sA.Position.x);
            
            if (x < point.Position.x)
                return true;
        }
        return false;
    }

    /// Equals override
    public override bool Equals(object obj)
    {
        if (obj.GetType() != typeof (Triangle)) 
            return false;
        return Equals((Triangle) obj);
    }

    /// <summary>
    /// Equals method checking triangles equality
    /// </summary>
    /// <param name="tri">The compared to triangle</param>
    /// <returns>Whether the triangles are equivalent</returns>
    public bool Equals(Triangle tri)
    {
        return tri.A.Equals(A) && tri.B.Equals(B) && tri.C.Equals(C);
    }

    /// Hashcode override
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
}