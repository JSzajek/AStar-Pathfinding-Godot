using Godot;

/// <summary>
/// Struct reprenting a edge
/// </summary>
public struct Edge {
    public Vertex A;
    public Vertex B;

    /// <summary>
    /// Constructor initializing an edge
    /// </summary>
    /// <param name="a">The first Vertex of the edge</param>
    /// <param name="b">The second Vertex of the edge</param>
    public Edge(Vertex a, Vertex b) {
        A = a;
        B = b;
    }

    /// <summary>
    /// Finds the intersection with the passed segment represented by an origin
    /// point and a direction.
    /// </summary>
    /// <param name="origin">The origin point of the segment</param>
    /// <param name="direction">The direction of the segment</param>
    /// <returns>The distance to intersection or if doens't intersect then null</returns>
    public float? IntersectsWithSegment(Vector2 origin, Vector2 direction) {
        float largestDistance = Mathf.Max(A.Position.x - origin.x, B.Position.x - origin.x) * 2f;
        Edge raySegment = new Edge(new Vertex(origin, 0), new Vertex(origin + (direction * largestDistance), 0));

        Vector2? intersection = FindIntersection(this, raySegment);
        float? value = null;

        if (intersection != null)
            value = origin.DistanceTo(intersection.Value);

        return value;
    }

    /// <summary>
    /// Helper method to find the intersection point of the passed edges.
    /// </summary>
    /// <param name="a">The first edge</param>
    /// <param name="b">The second edge</param>
    /// <returns>The intersection point of the edges or null if no intersection</returns>
    private Vector2? FindIntersection(Edge a, Edge b) {
        float x1 = a.A.Position.x;
        float y1 = a.A.Position.y;
        float x2 = a.B.Position.x;
        float y2 = a.B.Position.y;
        float x3 = b.A.Position.x;
        float y3 = b.A.Position.y;
        float x4 = b.B.Position.x;
        float y4 = b.B.Position.y;

        float denom = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);

        float uaNum = (x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3);
        float ubNum = (x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3);

        float ua = uaNum / denom;
        float ub = ubNum / denom;

        if (Mathf.Clamp(ua, 0f, 1f) != ua || Mathf.Clamp(ub, 0f, 1f) != ub)
            return null;

        return a.A.Position + (a.B.Position - a.A.Position) * ua;
    }
}