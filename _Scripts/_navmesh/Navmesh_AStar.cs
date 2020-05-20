using Godot;
using System.Collections.Generic;
using System.Linq;
using System;
using Array = System.Array;

/// <summary>
/// Class representing AStar pathfinding and generating
/// the corresponding grid map
/// </summary>
public class Navmesh_AStar : Spatial, IAStar 
{
    private const float marginAdjust = 0.2f;
	private const float largeTriangleThreshold = 20f;
	private const float MIN_PATH_UPDATE_TIME = 0.2f;
	private bool _visualizeGrid = false, _visualizeNetwork = false;

	[Export]
	public int minMovementPenalty {get; set;} = 0;

	[Export]
	public int maxMovementPenalty {get; set;} = 10;

	[Export]
	public bool visualizeGrid {
		get => _visualizeGrid; 
		set {
			_visualizeGrid = value;
			if (triangles != null && triangles.Count > 0) {
				ToggleVisualization();
			}
		}
	}

    [Export]
    public bool visualizeNetwork {
        get => _visualizeNetwork;
        set {
            _visualizeNetwork = value;
            if (triangles != null && triangles.Count > 0) {
				ToggleNetworkVisualization();
			}
        }
    }

	[Export]
	public float visualAlpha {get; set;} = 1.0f;

	#region Private Parameters

	private enum CollisionType {
		WALKABLE, UNWALKABLE, UNDEFINED
	}

	private class ClockWiseVector2Comparer : IComparer<Vector2> {
		public int Compare(Vector2 v1, Vector2 v2) {
			if (v1.x >= 0) {
				return v2.x < 0 ? 1 : -Comparer<float>.Default.Compare(v1.y, v2.y);
			}
			return v2.x >= 0 ? -1 :  Comparer<float>.Default.Compare(v1.y, v2.y);
		}
	};

    private HashSet<Vector3> vertices; // hashset for O(1) look up
    private List<Tuple<Vector3, Vector3>> edges;
    private List<Triangle> triangles;

	private readonly DllLoader loader = new DllLoader();
	private Navmesh_AStarLinker AStarLinker;
	private Timer pathTimer;
	private PathRequestManager pathRequestManager;
	private PhysicsDirectSpaceState direct_space;
	private PhysicsShapeQueryParameters check_shape;
	private Curve curve;

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
		AStarLinker = loader.GetNavmeshAStarLinker();

		// Initialize path request manager
		pathRequestManager = new PathRequestManager(AStarLinker, AddNode);

		// Create Path Update Timer
		pathTimer = new Timer();
		pathTimer.Name = "Path_Update_Timer";
		AddNode(pathTimer);
		pathTimer.Start(MIN_PATH_UPDATE_TIME);
		
		// Initial grid generation
		GenerateMap();
	}

	/// <summary>
	/// Destructor method
	/// </summary>
	public override void _ExitTree()
	{
		loader.Destroy();
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

	/// <summary>
	/// Generates the astar grid map
	/// </summary>
	public void GenerateMap()
	{
        vertices = new HashSet<Vector3>();
        edges = new List<Tuple<Vector3, Vector3>>();

        // Sticking with only one ground for right now - later version will connect multiple ground polygons or walkable surfaces
        // TODO : Connect multiple walkable surfaces
        var ground = this.Get<MeshInstance>("/root/Main/NavmeshScene/Ground");

		var grouped = this.Get<Spatial>("/root/Main/NavmeshScene").GetChildren<MeshInstance>().GroupBy(mesh => GetCollisionCategory(mesh.GetFirstChild<PhysicsBody>()));
		var unwalkables = grouped.Single(group => group.Key == CollisionType.UNWALKABLE);

		var groundPoints = ground.GetTopCorners();

		var points = groundPoints.Select(x => x.To2D()).ToArray();
		Array.Sort(points, new ClockWiseVector2Comparer());

		float maxX = float.MinValue, maxZ = float.MinValue, minX = float.MaxValue, minZ = float.MaxValue;
		foreach(var gPoint in groundPoints) {
			if (gPoint.x > maxX) { maxX = gPoint.x; }
			if (gPoint.z > maxZ) { maxZ = gPoint.z; }
			if (gPoint.x < minX) { minX = gPoint.x; }
			if (gPoint.z < minZ) { minZ = gPoint.z; }
		}

		var triangulation = new Triangulation();
		List<Vector2> sourceVertices = points.ToList();
		var potentialHoles = new List<MeshInstance>();

        foreach(var unwalkable in unwalkables) {
			var unwalkablePoints = unwalkable.GetTopCorners(marginAdjust);
			if (unwalkablePoints.Any(p => p.x <= maxX && p.x >= minX && p.z <= maxZ && p.z >= minZ)) {
				if (unwalkablePoints.Any(p => !(p.x <= maxX && p.x >= minX && p.z <= maxZ && p.z >= minZ))) {
					var excludingPoly = Geometry.IntersectPolygons2d(sourceVertices.ToArray(), unwalkablePoints.Select(x =>x.To2D()).ToArray());
					if (excludingPoly.Count == 1 && excludingPoly[0] is Vector2[] exclusion) {
						
						var conflicts = exclusion.Where(point => (point.x != maxX || point.y != maxZ) 
																  && (point.x != maxX || point.y != minZ)
																  && (point.x != minX || point.y != maxZ)
																  && (point.x != minX || point.y != minZ));
						if (conflicts.Count() > 0) {
							foreach(var conflict in exclusion.Where(point => !conflicts.Contains(point))) {
								sourceVertices.Remove(conflict);
							}
							sourceVertices.AddRange(conflicts);
							sourceVertices.Sort(new ClockWiseVector2Comparer());
							continue;
						}
					}
				}
				else {
					// When they are within the walkable bounding box but not on the edge, then it is most likely a hole/obstacle
					potentialHoles.Add(unwalkable);
				}
			}
		}
		
		// Cut out holes after determining concave polygon boundaries - otherwise will result in weird empty triangles
		var constraints = new List<Vector3>();
		foreach(var unwalkable in potentialHoles) {
			constraints.Clear();
			foreach(var p in unwalkable.GetTopCorners(marginAdjust)) {
				constraints.Add(new Vector3(Mathf.Clamp(p.x, minX + marginAdjust, maxX -marginAdjust), 
											p.y, 
											Mathf.Clamp(p.z, minZ + marginAdjust, maxZ - marginAdjust)));
			}
			var constraintPoints = constraints.Select(x => x.To2D()).ToArray();
			Array.Sort(constraintPoints, new ClockWiseVector2Comparer());
			sourceVertices = new List<Vector2>(triangulation.CutHoleInShape(sourceVertices.ToArray(), constraintPoints));
		}

		triangles = triangulation.Triangulate(sourceVertices.ToArray(), WindingOrder.ClockWise);

		// Clean up small area triangles - May remove b/c potentially losing a path connection
		triangles = triangles.Where(tri => tri.GetArea() > 0.5f).ToList();
		
        foreach(var tri in triangles) {
            (var v1, var v2, var v3) = tri.GetVertices();
            (var m1, var m2, var m3) = tri.GetMidPoints();
            vertices.Add(m1);
            vertices.Add(m2);
            vertices.Add(m3);

            // Regular Triangle
            edges.Add(Tuple.Create(m1, v1));
            edges.Add(Tuple.Create(m1, v2));
            edges.Add(Tuple.Create(m1, v2));
            edges.Add(Tuple.Create(m1, v3));
            edges.Add(Tuple.Create(m3, v1));
            edges.Add(Tuple.Create(m3, v3));

            if (tri.GetArea() > largeTriangleThreshold) {
                var centriod = tri.GetCentriod();
                vertices.Add(centriod);
                edges.Add(Tuple.Create(m1, centriod));
                edges.Add(Tuple.Create(m2, centriod));
                edges.Add(Tuple.Create(m3, centriod));
                edges.Add(Tuple.Create(centriod, v1));
				edges.Add(Tuple.Create(centriod, v2));
                edges.Add(Tuple.Create(centriod, v3));
            }
            else {
				edges.Add(Tuple.Create(m1, m2));
                edges.Add(Tuple.Create(m2, m3));
				edges.Add(Tuple.Create(m3, m1));
            }
        }

		AStarLinker.setUpGrid(vertices, edges);

		// Visualizing the triangles (mainly for testing)
		if (visualizeGrid) {
			VisualizeTriangles();
		}

        // Visualizing the connections (mainly for testing)
        if (visualizeNetwork) {
            VisualizeConnections();
        }
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
		var vis = this.Get<Spatial>("Debug/visualization");
		if (vis != null) {
			vis.Visible = !vis.Visible;
			return;
		}
		VisualizeTriangles();
	}

    /// <summary>
    /// Toggles the visibility of the connections visualization (used for testing)
    /// </summary>
    private void ToggleNetworkVisualization() {
        var vis = this.Get<ImmediateGeometry>("Debug/connections");
        if (vis != null) {
            vis.Visible = !vis.Visible;
            return;
        }
        VisualizeConnections();
    }

    /// <summary>
    /// Visualizes the connections of the graph within the navmesh (Used for testing)
    /// </summary>
    private void VisualizeConnections() {
        var debug = new ImmediateGeometry() { Name = "connections"};
		GetNode("Debug").AddChild(debug);
		debug.Clear();
		debug.Begin(Mesh.PrimitiveType.Lines);
		foreach(var trig in triangles) {
            (var v1, var v2, var v3) = trig.GetVertices();
			(var m1, var m2, var m3) = trig.GetMidPoints();
            // Regular Triangle
            debug.AddVertex(v1 + (Vector3.Up));
            debug.AddVertex(v2 + (Vector3.Up));
            debug.AddVertex(v2 + (Vector3.Up));
            debug.AddVertex(v3 + (Vector3.Up));
            debug.AddVertex(v3 + (Vector3.Up));
            debug.AddVertex(v1 + (Vector3.Up));
			if (trig.GetArea() > largeTriangleThreshold) {
				// Triangle is large enough to warrent a centriod waypoint
				var centriod = trig.GetCentriod();
                debug.AddVertex(v1 + (Vector3.Up));
				debug.AddVertex(centriod + (Vector3.Up));
				debug.AddVertex(m1 + (Vector3.Up));
				debug.AddVertex(centriod + (Vector3.Up));

                debug.AddVertex(v2 + (Vector3.Up));
				debug.AddVertex(centriod + (Vector3.Up));
				debug.AddVertex(m2 + (Vector3.Up));
				debug.AddVertex(centriod + (Vector3.Up));

                debug.AddVertex(v3 + (Vector3.Up));
				debug.AddVertex(centriod + (Vector3.Up));
				debug.AddVertex(m3 + (Vector3.Up));
				debug.AddVertex(centriod + (Vector3.Up));
			}
			else {
                debug.AddVertex(m1 + (Vector3.Up));
				debug.AddVertex(m2 + (Vector3.Up));

				debug.AddVertex(m2 + (Vector3.Up));
				debug.AddVertex(m3 + (Vector3.Up));
                
				debug.AddVertex(m3 + (Vector3.Up));
				debug.AddVertex(m1 + (Vector3.Up));
			}
		}
		debug.End();
    }

    /// <summary>
	/// Visualizes the navmesh triangles (used for testing)
	/// </summary>
    private void VisualizeTriangles() {
		Spatial visualization = new Spatial() { Name = "visualization" };
        var numLargeTrigs = triangles.Where(tri => tri.GetArea() > largeTriangleThreshold).Count();
        var midPoints = new MultiMeshInstance(); // Utilize a multimesh for efficiency
		var material = new SpatialMaterial() {
			VertexColorUseAsAlbedo = true,
			FlagsTransparent = true
		};
		var cubeMesh = new CubeMesh() {
			Size = Vector3.One * 0.5f,
			Material = material
		};
		var multiMesh = new MultiMesh() {
			Mesh = cubeMesh,
			TransformFormat = MultiMesh.TransformFormatEnum.Transform3d,
			ColorFormat = MultiMesh.ColorFormatEnum.Float,
			InstanceCount = (triangles.Count * 6) + numLargeTrigs,
			VisibleInstanceCount = (triangles.Count * 6) + numLargeTrigs
		};

        var vertexPointColor = new Color(1f, 0, 0, visualAlpha);
        var midPointColor = new Color(0, 0, 1f, visualAlpha);
        var centriodPointColor = new Color(0, 1f, 0, visualAlpha);
		var debug = new ImmediateGeometry();
		visualization.AddChild(debug);
		debug.Clear();
		debug.Begin(Mesh.PrimitiveType.Lines);
        int cnt = 0;
		foreach(var tri in triangles) {
			(var v1, var v2, var v3) = tri.GetVertices();
            (var m1, var m2, var m3) = tri.GetMidPoints();
			multiMesh.SetInstanceTransform(cnt, new Transform(midPoints.Transform.basis, m1 + (Vector3.Up)));
            multiMesh.SetInstanceColor(cnt, midPointColor);
            cnt++;
            multiMesh.SetInstanceTransform(cnt, new Transform(midPoints.Transform.basis, v1 + (Vector3.Up)));
            multiMesh.SetInstanceColor(cnt, vertexPointColor);
            cnt++;
			debug.AddVertex(new Vector3(v1.x, 1, v1.z));
			debug.AddVertex(new Vector3(v2.x, 1, v2.z));

            multiMesh.SetInstanceTransform(cnt, new Transform(midPoints.Transform.basis, m2 + (Vector3.Up)));
            multiMesh.SetInstanceColor(cnt, midPointColor);
            cnt++;
            multiMesh.SetInstanceTransform(cnt, new Transform(midPoints.Transform.basis, v2 + (Vector3.Up)));
            multiMesh.SetInstanceColor(cnt, vertexPointColor);
            cnt++;
			debug.AddVertex(new Vector3(v2.x, 1, v2.z));
			debug.AddVertex(new Vector3(v3.x, 1, v3.z));

			multiMesh.SetInstanceTransform(cnt, new Transform(midPoints.Transform.basis, m3 + (Vector3.Up)));
            multiMesh.SetInstanceColor(cnt, midPointColor);
            cnt++;
            multiMesh.SetInstanceTransform(cnt, new Transform(midPoints.Transform.basis, v3 + (Vector3.Up)));
            multiMesh.SetInstanceColor(cnt, vertexPointColor);
            cnt++;
			debug.AddVertex(new Vector3(v3.x, 1, v3.z));
			debug.AddVertex(new Vector3(v1.x, 1, v1.z));

            if (tri.GetArea() > largeTriangleThreshold) {
                multiMesh.SetInstanceTransform(cnt, new Transform(midPoints.Transform.basis, tri.GetCentriod() + (Vector3.Up)));
                multiMesh.SetInstanceColor(cnt, centriodPointColor);
                cnt++;
            }
		}
        midPoints.Multimesh = multiMesh;
        visualization.AddChild(midPoints);
		debug.End();
        GetNode("Debug").AddChild(visualization);
	}

    /// <summary>
    /// Helper method to get the collision category of the passed physics body
    /// </summary>
    /// <param name="body">The passed physics body to get the category of</param>
    /// <returns>The collision type category of the physics body</returns>
    private CollisionType GetCollisionCategory(PhysicsBody body) {
		if (body == null) {
			return CollisionType.UNDEFINED;
		}
		if ((body.CollisionLayer & 16) != 0) {
			return CollisionType.WALKABLE;
		}
		else if ((body.CollisionLayer & 32) != 0) {
			return CollisionType.UNWALKABLE;
		}
		return CollisionType.UNDEFINED;
	}
}
