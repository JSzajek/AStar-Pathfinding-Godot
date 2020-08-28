using Godot;
using System.Collections.Generic;
using Godot.Collections;
using System;
using System.Diagnostics;
using Pathing;

namespace AStar
{
	/// <summary>
	/// Class representing AStar pathfinding and generating
	/// the corresponding grid map
	/// </summary>
	public class GridAStar : BaseAStar
	{
		#region Constants

		private const float MIN_PATH_UPDATE_TIME = 0.9f;

		#endregion Constants

		#region Fields

		private GridAStarLinker AStarLinker;
		private Vector2 region;
		private int _gridSizeX, _gridSizeY;
		private Curve curve;
		private Vector3 WorldCenter;
		private bool _visualizeGrid;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="GridAStar"/> class.
		/// </summary>
		public override void _Ready()
		{
			sceneMapDataPath = "res://_Scenes/map_data/" + System.IO.Path.GetFileName(Navigator.SceneController.CurrentScene.Filename.BaseName()) + ".data";
			curve = ResourceLoader.Load<Curve>("res://_Assets/Curve.tres");

			// Get link to AStar dll
			AStarLinker = Navigator.DllLoader.GetGridAStarLinker();
			AStarLinker.clear();

			// Create Path Update Timer
			pathTimer = new Timer();
			pathTimer.Name = "Path_Update_Timer";
			AddNode(pathTimer);
			pathTimer.Start(MIN_PATH_UPDATE_TIME);

			// Initialize path request manager
			pathRequestManager = new PathRequestManager(AStarLinker);
            AddChild(pathRequestManager);

			// Translate bounding box
			using (var bounds = GetNode("BoundingBox") as CollisionShape)
			{
				WorldCenter = bounds.GlobalTransform.origin;
				var extents = (bounds.Shape as BoxShape).Extents * 2;
				region = new Vector2(extents.x, extents.z); // double extents to actual scale
                bounds.QueueFree();
			}

			if (!RegenerateGrid && FileSystem.FileExists(sceneMapDataPath))
			{
				Stopwatch timer = Stopwatch.StartNew();
				GD.Print("Importing the grid from " + sceneMapDataPath);

				(_gridSizeX, _gridSizeY, MinMovementPenalty, MaxMovementPenalty) = AStarLinker.ImportGrid(sceneMapDataPath);

				// Visualizing the grid (mainly for testing)
				if (VisualizeGrid)
				{
					DrawGrid(_gridSizeX, _gridSizeY);
				}

				isMapGenerated = true;
				timer.Stop();
				TimeSpan timespan = timer.Elapsed;
				GD.Print(String.Format("{0:00}:{1:00}:{2:00}", timespan.Minutes, timespan.Seconds, timespan.Milliseconds / 10));

				Loading = false;
			}
			base._Ready();
		}

		#endregion Constructors

		#region Properties

		/// <summary>
		/// Gets or sets the node radius of the map. Lower
		/// values resulting in higher definition map with more points.
		/// </summary>
		[Export]
		public float NodeRadius { get; set; } = 0.5f;

		/// <summary>
		/// Gets or sets whether to blur the weights.
		/// </summary>
		[Export]
		public bool BlurWeights { get; set; }

		/// <summary>
		/// Gets or sets the blurring window size when smoothing weights.
		/// </summary>
		[Export]
		public int BlurSize { get; set; } = 3;

		///<inheritdoc/>
		[Export]
		public override bool VisualizeGrid
		{
			get => _visualizeGrid;
			set
			{
				_visualizeGrid = value;
				if (_gridSizeX * _gridSizeY != 0)
				{
					ToggleVisualization();
				}
			}
		}

		#endregion Properties

		#region Public Methods

		///<inheritdoc/>
		public override void GenerateMap()
		{
			GD.Print("Grid Generating map " + sceneMapDataPath);
			var nodeDiameter = NodeRadius * 2;
			_gridSizeX = Mathf.CeilToInt(region.x / nodeDiameter);
			_gridSizeY = Mathf.CeilToInt(region.y / nodeDiameter);

			AStarLinker.setUpGrid(_gridSizeX, _gridSizeY, MinMovementPenalty, MaxMovementPenalty, WorldCenter);

			List<float> points = new List<float>();
			points.Add((_gridSizeX * _gridSizeY * 7) + 1);
			Vector3 bottomLeft = WorldCenter - Vector3.Right * region.x / 2 - Vector3.Forward * region.y / 2;

			// Check each point in the grid and insert
			for (int x = 0; x < _gridSizeX; x++)
			{
				for (int y = 0; y < _gridSizeY; y++)
				{
					var point = bottomLeft + Vector3.Right * (x * nodeDiameter + NodeRadius) + Vector3.Forward * (y * nodeDiameter + NodeRadius);
					(var worldPoint, var isWalkable, var movePenalty) = GetSpot(point);
					worldPoint = worldPoint.Equals(Vector3.Inf) ? point : worldPoint;
					points.AddRange(new float[] { (float)x, (float)y, point.x, point.y, point.z, (float)(isWalkable ? 1 : 0), movePenalty });
				}
			}
			AStarLinker.addPoints(points);

			// Weight blurring
			if (BlurWeights)
			{
				(MinMovementPenalty, MaxMovementPenalty) = AStarLinker.blurMap(BlurSize);
			}

			// Visualizing the grid (mainly for testing)
			if (VisualizeGrid)
			{
				DrawGrid(_gridSizeX, _gridSizeY);
			}

			isMapGenerated = true;
			AStarLinker.ExportGrid(sceneMapDataPath);
		}

		public override void _ExitTree()    
		{
			GD.Print("Grid Exiting the tree");
			curve.Dispose();
			base._ExitTree();
		}

		#endregion Public Methods

		#region Private Methods

		/// <summary>
		/// Toggles the visibility of the grid visualization (used for testing).
		/// </summary>
		private void ToggleVisualization()
		{
			var vis = this.Get<MultiMeshInstance>("Debug/visualization");
			if (vis != null)
			{
				vis.Visible = !vis.Visible;
				return;
			}
			DrawGrid(_gridSizeX, _gridSizeY);
		}

		/// <summary>
		/// Visualizes the grid (used for testing).
		/// </summary>
		/// <param name="_gridSizeX">The width of the grid</param>
		/// <param name="_gridSizeY">The depth of the grid</param>
		private void DrawGrid(int _gridSizeX, int _gridSizeY)
		{
			MultiMeshInstance visualization; // Utilize a multimesh for efficiency
			visualization = this.Get<MultiMeshInstance>("Debug/visualization");
			if (visualization == null)
			{
				visualization = new MultiMeshInstance()
				{
					Name = "visualization"
				};
			}
			var material = new SpatialMaterial()
			{
				VertexColorUseAsAlbedo = true,
				FlagsTransparent = true
			};
			var cubeMesh = new CubeMesh()
			{
				Size = Vector3.One * NodeRadius,
				Material = material
			};
			var multiMesh = new MultiMesh()
			{
				Mesh = cubeMesh,
				TransformFormat = MultiMesh.TransformFormatEnum.Transform3d,
				ColorFormat = MultiMesh.ColorFormatEnum.Float,
				InstanceCount = _gridSizeX * _gridSizeY,
				VisibleInstanceCount = _gridSizeX * _gridSizeY
			};

			int cnt = 0;
			for (int x = 0; x < _gridSizeX; x++)
			{
				for (int y = 0; y < _gridSizeY; y++)
				{
					(var worldPoint, var movePen, var isWalkable) = AStarLinker.getPoint(x, y);
					float d = ((movePen - MinMovementPenalty) / (float)(MaxMovementPenalty - MinMovementPenalty));
					var color = new Color(1, 1, 1).LinearInterpolate(new Color(0, 0, 0), curve.Interpolate(d));
					color = isWalkable ? new Color(color.r, color.g, color.b, VisualAlpha) : new Color(1, 0, 0, VisualAlpha);
					multiMesh.SetInstanceTransform(cnt, new Transform(visualization.Transform.basis, worldPoint + (Vector3.Up * NodeRadius)));
					multiMesh.SetInstanceColor(cnt, color);
					cnt++;
				}
			}
			visualization.Multimesh = multiMesh;
			if (visualization.GetParentOrNull<Spatial>() == null)
			{
				GD.Print("Adding Debugging");
				this.Get<Spatial>("Debug").AddChild(visualization);
			}
		}

		/// <summary>
		/// Helper method that will check the passed spot for
		/// its collision world position and walkablility and movement penalty
		/// if applicable.
		/// </summary>
		/// <param name="center">The checked spot position</param>
		/// <returns>Tuple representing the world position, walkablity, and movement penalty of the spot</returns>
		private (Vector3, bool, int) GetSpot(Vector3 center)
		{
			// TODO: Check out why inside unwalkable bodies are being labeled as walkable.
			if (FireRayCast(center) is Dictionary result && result.Contains("position") &&
					result["position"] is Vector3 position && result["collider"] is PhysicsBody collider)
			{

				check_shape.Transform = new Transform(check_shape.Transform.basis, center + (Vector3.Up * position.y) + (Vector3.Up * HeightClearance / 2));
				var shape = new BoxShape() { Extents = new Vector3(NodeRadius, HeightClearance / 2, NodeRadius) };
				check_shape.SetShape(shape);

				if (direct_space.GetRestInfo(check_shape).Count == 0)
				{
					shape.Dispose();
					return (position, true, FindMovementPenalty(collider));
				}
				shape.Dispose();
				return (position, false, ObstacleProximityPenalty);
			}
			return (Vector3.Inf, false, ObstacleProximityPenalty);
		}

		#endregion Private Methods
	}
}
