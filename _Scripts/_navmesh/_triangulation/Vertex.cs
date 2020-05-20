using Godot;

/// <summary>
/// Struct representing a vertex
/// </summary>
public struct Vertex {
    public readonly Vector2 Position;
    private Vector3 worldPosition;
    public readonly int Index;

    /// <summary>
    /// Constructor for vertex with 2d vector
    /// </summary>
    /// <param name="position"></param>
    /// <param name="index"></param>
    public Vertex(Vector2 position, int index) {
        this.Position = position;
        this.worldPosition = new Vector3(position.x, 0, position.y);
        this.Index = index;
    }

    /// <summary>
    /// Constructor for vertex with a 3d vector
    /// </summary>
    /// <param name="position">The world position</param>
    /// <param name="index">The index of the vertex</param>
    public Vertex(Vector3 position, int index) {
        this.worldPosition = position;
        this.Position = new Vector2(worldPosition.x, worldPosition.z);
        this.Index  = index;
    }
    
    /// <summary>
    /// Getter method that returns the world point
    /// </summary>
    /// <returns></returns>
    public Vector3 getWorldPosition() => worldPosition;

    /// Equality override
    public override bool Equals(object obj)
    {
        if (obj.GetType() != typeof(Vertex)) 
            return false;
        return Equals((Vertex)obj);
    }

    /// <summary>
    /// Equals method checking vertices equality
    /// </summary>
    /// <param name="vert">The compared to vertex</param>
    /// <returns>Whether the vertices are equivalent</returns>
    public bool Equals(Vertex vert)
    {
        return vert.Position.Equals(Position) && vert.Index == Index;
    }

    /// Hashcode override
    public override int GetHashCode()
    {
        unchecked
        {
            return (Position.GetHashCode() * 397) ^ Index;
        }
    }

    /// ToString override
    public override string ToString()
    {
        return string.Format("{0} ({1})", Position, Index);
    }
}