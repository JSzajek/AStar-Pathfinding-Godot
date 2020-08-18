using Godot;

namespace Triangulation
{
    /// <summary>
    /// Struct representing a vertex point
    /// </summary>
    public struct Vertex
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Vertex"/> class.
        /// </summary>
        /// <param name="position">The position of the vertex</param>
        /// <param name="index">The index of the vertex</param>
        public Vertex(Vector3 position, int index)
        {
            Position = position;
            Index = index;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the position of the vertex.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// Gets the index of the vertex, -1 if no valid index.
        /// </summary>
        public int Index { get; }

        #endregion Properties

        #region Public Methods

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is Vertex vertex)
            {
                return vertex.Position.Equals(Position) && vertex.Index == Index;
            }
            return false;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Position.GetHashCode() * 397) ^ Index;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("{0} ({1})", Position, Index);
        }

        #endregion Public Methods
    }
}

