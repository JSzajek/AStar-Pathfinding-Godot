/// <summary>
/// Struct representing a terrain type
/// </summary>
public struct TerrainType
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainType"/> class.
    /// </summary>
    /// <param name="name">The name of terrain</param>
    /// <param name="mask">The collision mask of the terrain</param>
    /// <param name="penalty">The movement penalty of the terrain</param>
    public TerrainType(string name, int mask, int penalty)
    {
        Name = name;
        Mask = mask;
        Penalty = penalty;
    }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// Gets the name of the terrain type.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the collision mask of the terrain type.
    /// </summary>
    public int Mask { get; }

    /// <summary>
    /// Gets the penalty value of the terrain type.
    /// </summary>
    public int Penalty { get; }

    #endregion Properties
}