using Godot;
using System.Collections.Generic;

/// <summary>
/// Class representing a chunk controlling the rendering visibility 
/// and level of detail for performance purposes.
/// </summary>
public class Chunk : VisibleEntity
{
	#region Fields

    private bool _visible;

	#endregion Fields

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="Chunk"/> class.
	/// </summary>
	public override void _Ready()
	{
		// Ensures the chunk starts unrendered, unless tagged as always render
		Visible = AlwaysVisible ? AlwaysVisible : false;
	}

	#endregion Constructors

	#region Public Properties

	/// <summary>
	/// Gets or sets the Ground Truth MeshInstance of the chunk.
	/// </summary>
	public MeshInstance groundTruth { get; protected set; }

	/// <inheritdoc/>
	public override bool Visible 
	{
		get => _visible; 
		set
		{
			_visible = value;
			BaseVisible = _visible;
			OnVisibilityChanged();
		}
	}

	/// ** Below is related to NavMesh AStar Pathing generation - fix with NavMesh AStar Pathing **

	/// <summary>
	/// Gets the collection of the decoration corners pertaining to the chunk.
	/// </summary>
	/// <typeparam name="MeshInstance"></typeparam>
	public List<MeshInstance> decorationCorners { get; } = new List<MeshInstance>();

	/// <summary>
	/// Gets the collection of obstacles pertaining to the chunk.
	/// </summary>
	/// <typeparam name="MultiMeshInstance"></typeparam>
	public List<MultiMeshInstance> obstacles { get; } = new List<MultiMeshInstance>();

	#endregion Public Properties
}