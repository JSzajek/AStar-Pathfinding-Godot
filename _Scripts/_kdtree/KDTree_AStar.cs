using Godot;
using System.Collections.Generic;
using Godot.Collections;

/// <summary>
/// Class representing AStar pathfinding and generating
/// the corresponding grid map
/// </summary>
public class KDTree_AStar : Spatial, IAStar 
{
	private const float MIN_PATH_UPDATE_TIME = 0.3f;
	private bool _visualizeGrid = false;

	[Export]
	public float nodeRadius { get; set; } = 0.5f;

	[Export]
	public int minMovementPenalty {get; set;} = 0;

	[Export]
	public int maxMovementPenalty {get; set;} = 10;

	[Export]
	public int obstacleProximityPenalty {get; set;} = 20;

	[Export]
	public float heightClearance {get; set;} = 4.5f;

	[Export]
	public bool blurWeights { get; set; } = false;
	
	[Export]
	public int blurSize { get; set; } = 3;

	[Export]
	public bool visualizeGrid {
		get => _visualizeGrid; 
		set {
			_visualizeGrid = value;
			if (points != null) {
				ToggleVisualization();
			}
		}
	}

	[Export]
	public float visualAlpha {get; set;} = 1.0f;

	#region Private Parameters

	private const byte walkableMask = 4, unwalkableMask = 5;
	private uint collisionMaskBit;

	private readonly DllLoader loader = new DllLoader();
	private KDTree_AStarLinker AStarLinker;
	private Timer pathTimer;
	private PathRequestManager pathRequestManager;
	private PhysicsDirectSpaceState direct_space;
	private PhysicsShapeQueryParameters check_shape;
	private Vector2 region;
	private int _gridSizeX, _gridSizeY;
	private Vector3 center;
	private Curve curve;

	private List<float[]> points;

	#endregion Private Parameters

	#region Terrain Parameters

	/// <summary>
	/// Struct representing a terrain type
	/// </summary>
	private struct TerrainType{
		public string name;
		public int mask, penalty;

		/// <summary>
		/// Constructor initializes the terrain type
		/// </summary>
		/// <param name="name">The name of terrain</param>
		/// <param name="mask">The collision mask of the terrain</param>
		/// <param name="penalty">The movement penalty of the terrain</param>
		public TerrainType(string name, int mask, int penalty) {
			this.name = name;
			this.mask = mask;
			this.penalty = penalty;
		}
	}

	// List of movement penalties
	private List<TerrainType> terrains = new List<TerrainType>()
	{
		new TerrainType("Grass", 6, 8),
		new TerrainType("Path", 7, 3),
		new TerrainType("Sand", 8, 20)
	};

	#endregion Terrain Parameters

	/// <summary>
	/// Initializes the parameters
	/// </summary>
	public override void _Ready()
	{
		curve = ResourceLoader.Load<Curve>("res://_Assets/Curve.tres");

		// Get link to AStar dll
		AStarLinker = loader.GetKDTreeAStarLinker();

		// Initialize path request manager
		pathRequestManager = new PathRequestManager(AStarLinker, AddNode);

		// Create Path Update Timer
		pathTimer = new Timer();
		pathTimer.Name = "Path_Update_Timer";
		AddNode(pathTimer);
		pathTimer.Start(MIN_PATH_UPDATE_TIME);

		// Get the physics params
		collisionMaskBit = GetCollisionBit(new List<byte>() {walkableMask});
		direct_space = GetWorld().DirectSpaceState;
		check_shape = new PhysicsShapeQueryParameters() {CollisionMask = (int)GetCollisionBit(new List<byte>() {unwalkableMask})};
		var shape = new SphereShape() {Radius = nodeRadius};
		check_shape.SetShape(shape);

		// Translate bounding box
		using (var bounds = GetNode("BoundingBox") as CollisionShape) {
			center = bounds.GlobalTransform.origin;
			var extents = (bounds.Shape as BoxShape).Extents + center;
			region = new Vector2(extents.x, extents.z) * 2; // double extents to actual scale
			RemoveChild(bounds); // Cleanup editor bounding box
		}
		
		// Initial grid generation
		GenerateMap();
	}

	/// <summary>
	/// Destructor method
	/// </summary>
	public override void _ExitTree()
	{
		loader.Destroy();
		pathRequestManager.Destroy();
	}

	/// <summary>
	/// Helper method to add nodes to the scene, for classes outside of the scene
	/// </summary>
	/// <param name="node">The node to add</param>
	public void AddNode(Node node)
	{
		AddChild(node);
	}

	/// <summary>
	/// Connects the target to the path timer
	/// </summary>
	/// <param name="target">The target to connect</param>
	/// <param name="method">The string method</param>
	public void ConnectToTimer(Godot.Object target, string method) {
		pathTimer.Connect("timeout", target, method);
	}

	public void GenerateMap() 
	{
		var nodeDiameter = nodeRadius * 2;
		_gridSizeX = Mathf.CeilToInt(region.x / nodeDiameter);
		_gridSizeY = Mathf.CeilToInt(region.y / nodeDiameter);
		
		AStarLinker.setUpGrid(nodeRadius, minMovementPenalty, maxMovementPenalty);

		Vector3 bottomLeft = center - Vector3.Right * region.x / 2 - Vector3.Forward * region.y / 2;
		points = new List<float[]>();

		// Check each point in the grid and insert
		for (int x = 0; x < _gridSizeX; x++)
		{
			for (int y = 0; y < _gridSizeY; y++)
			{
				var point = bottomLeft + Vector3.Right * (x * nodeDiameter + nodeRadius) + Vector3.Forward * (y * nodeDiameter + nodeRadius);
				(var worldPoint, var isWalkable, var movePenalty) = GetSpot(point);
				worldPoint = worldPoint.Equals(Vector3.Inf) ? point : worldPoint;
				points.Add(new float[] { point.x, point.y, point.z, isWalkable ? 1 : 0, movePenalty});
			}
		}

		// No weight blurring currently supported with kdtree

		if (points.Count < 0) {
			GD.PrintErr("AStar did not find any pathing instances");
			return;
		}
		AStarLinker.addPoints(points.ToArray().To2D());

		// Visualizing the grid (mainly for testing)
		if (visualizeGrid) {
			VisualizeGrid();
		}
	}

	public Vector3 getPoint(Vector3 position) {
		return AStarLinker.getPoint(position).Item1;
	}

	public Vector3[] nearestNeighbors(Vector3 position) {
		return AStarLinker?.getNearNeighbors(position);
	}

	/// <summary>
	/// Requests a path from the path request manager.
	/// </summary>
	/// <param name="request">The path request</param>
	public void RequestPath(PathRequest request) {
		pathRequestManager.RequestPath(request);
	}

	/// <summary>
	/// Toggles the visibility of the grid visualization (used for testing)
	/// </summary>
	private void ToggleVisualization() {
		var vis = this.Get<MultiMeshInstance>("Debug/visualization");
		if (vis != null) {
			vis.Visible = !vis.Visible;
			return;
		}
		VisualizeGrid();
	}

	/// <summary>
	/// Visualizes the grid (used for testing)
	/// </summary>
	/// <param name="_gridSizeX">The width of the grid</param>
	/// <param name="_gridSizeY">The depth of the grid</param>
	private void VisualizeGrid() {
		MultiMeshInstance visualization; // Utilize a multimesh for efficiency
		visualization = this.Get<MultiMeshInstance>("Debug/visualization");
		if (visualization == null) {
			visualization = new MultiMeshInstance() {
				Name = "visualization"
			};
		}
		var material = new SpatialMaterial() {
			VertexColorUseAsAlbedo = true,
			FlagsTransparent = true
		};
		var cubeMesh = new CubeMesh() {
			Size = Vector3.One * nodeRadius,
			Material = material
		};
		var multiMesh = new MultiMesh() {
			Mesh = cubeMesh,
			TransformFormat = MultiMesh.TransformFormatEnum.Transform3d,
			ColorFormat = MultiMesh.ColorFormatEnum.Float,
			InstanceCount = _gridSizeX * _gridSizeY,
			VisibleInstanceCount = _gridSizeX * _gridSizeY
		};

		int cnt = 0;
		foreach(var p in points) {
			float d = ((p[4] - minMovementPenalty) / (float)(maxMovementPenalty - minMovementPenalty));
			var color = new Color(1, 1, 1).LinearInterpolate(new Color(0,0,0), curve.Interpolate(d));
			color = (p[3] == 1) ? new Color(color.r, color.g, color.b, visualAlpha) : new Color(1, 0, 0, visualAlpha);
			multiMesh.SetInstanceTransform(cnt, new Transform(visualization.Transform.basis, new Vector3(p[0], p[1], p[2]) + (Vector3.Up * nodeRadius)));
			multiMesh.SetInstanceColor(cnt, color);
			cnt++;
		}
		visualization.Multimesh = multiMesh;
		this.Get<Spatial>("Debug").AddChild(visualization);
	}

	/// <summary>
	/// Helper method that will check the passed spot for
	/// its collision world position and walkablility and movement penalty
	/// if applicable.
	/// </summary>
	/// <param name="center">The checked spot position</param>
	/// <returns>Tuple representing the world position, walkablity, and movement penalty of the spot</returns>
	private (Vector3, bool, int) GetSpot(Vector3 center) {
		// TODO: Check out why inside unwalkable bodies are being labeled as walkable.
		if (FireRayCast(center) is Dictionary result && result.Contains("position") &&
				result["position"] is Vector3 position && result["collider"] is PhysicsBody collider) {
			check_shape.Transform = new Transform(check_shape.Transform.basis, center + (Vector3.Up * position.y));
			var temp = direct_space.IntersectShape(check_shape, 1);
			if (temp.Count == 0) {	
				return (position, true, FindMovementPenalty(collider));
			}
			return (position, false, obstacleProximityPenalty);
		}
		return (Vector3.Inf, false, obstacleProximityPenalty);
	}

	/// <summary>
	/// Helper method to find the movement penalty of the passed collider
	/// from the predefined penalty list.
	/// </summary>
	/// <param name="collider">The passed collider</param>
	/// <returns>The movement penatly of the collider, or 0 if not found</returns>
	private int FindMovementPenalty(PhysicsBody collider) {
		foreach(var terrain in terrains) {
			if(collider.GetCollisionLayerBit(terrain.mask)) {
				return terrain.penalty;
			}
		}
		return 0; // Should change to obstacle penalty?
	}

	/// <summary>
	/// Helper method that fires the ray cast at the specified position
	/// </summary>
	/// <param name="center">The position to fire the ray cast at</param>
	/// <returns>Godot Dictionary of the resulting collision from the ray cast</returns>
	private Dictionary FireRayCast(Vector3 center) {
		return direct_space.IntersectRay(center + (Vector3.Up * heightClearance), center + (Vector3.Down * 100), null, collisionMaskBit, true);
	}

	/// <summary>
	/// Calculates the collision bit mask from a list of byte values.
	/// </summary>
	/// <param name="values">The list of byte values</param>
	/// <returns>The collision bit mask</returns>
	private uint GetCollisionBit(List<byte> values) {
		uint bit = 0;
		foreach(var val in values) {
			bit += (uint)Mathf.Pow(2, val);
		}
		return bit;
	}
}
