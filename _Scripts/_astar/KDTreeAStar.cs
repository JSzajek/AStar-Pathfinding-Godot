using Godot;
using System.Collections.Generic;
using Godot.Collections;
using Pathing;

namespace AStar
{
    /// <summary>
    /// Class representing KDTree AStar pathfinding and generating
    /// the corresponding grid map
    /// </summary>
    public class KDTreeAStar : BaseAStar
    {
        #region Constants

        private const float MIN_PATH_UPDATE_TIME = 0.3f;

        #endregion Constants

        #region Fields

        private KDTreeAStarLinker AStarLinker;
        private Vector2 region;
        private int _gridSizeX, _gridSizeY;
        private Vector3 center;
        private Curve curve;
        private Vector3 WorldCenter;
        private List<float[]> points;
        private bool _visualizeGrid;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="KDTreeAStar"/> class.
        /// </summary>
        public override void _Ready()
        {
            curve = ResourceLoader.Load<Curve>("res://_Assets/Curve.tres");

            // Get link to AStar dll
            AStarLinker = Navigator.DllLoader.GetKDTreeAStarLinker();

            // Initialize path request manager
            pathRequestManager = new PathRequestManager(AStarLinker);
            AddChild(pathRequestManager);

            // Create Path Update Timer
            pathTimer = new Timer();
            pathTimer.Name = "Path_Update_Timer";
            AddNode(pathTimer);
            pathTimer.Start(MIN_PATH_UPDATE_TIME);

            // Translate bounding box
            using (var bounds = GetNode("BoundingBox") as CollisionShape)
            {
                WorldCenter = bounds.GlobalTransform.origin;
                var extents = (bounds.Shape as BoxShape).Extents + center;
                region = new Vector2(extents.x, extents.z) * 2; // double extents to actual scale
                RemoveChild(bounds); // Cleanup editor bounding box
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

        ///<inheritdoc/>
        [Export]
        public override bool VisualizeGrid
        {
            get => _visualizeGrid;
            set
            {
                _visualizeGrid = value;
                if (points != null)
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
            var nodeDiameter = NodeRadius * 2;
            _gridSizeX = Mathf.CeilToInt(region.x / nodeDiameter);
            _gridSizeY = Mathf.CeilToInt(region.y / nodeDiameter);

            AStarLinker.setUpGrid(NodeRadius, MinMovementPenalty, MaxMovementPenalty);

            Vector3 bottomLeft = WorldCenter - Vector3.Right * region.x / 2 - Vector3.Forward * region.y / 2;
            points = new List<float[]>();

            // Check each point in the grid and insert
            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = 0; y < _gridSizeY; y++)
                {
                    var point = bottomLeft + Vector3.Right * (x * nodeDiameter + NodeRadius) + Vector3.Forward * (y * nodeDiameter + NodeRadius);
                    (var worldPoint, var isWalkable, var movePenalty) = GetSpot(point);
                    worldPoint = worldPoint.Equals(Vector3.Inf) ? point : worldPoint;
                    points.Add(new float[] { point.x, point.y, point.z, isWalkable ? 1 : 0, movePenalty });
                }
            }

            // No weight blurring currently supported with kdtree

            if (points.Count < 0)
            {
                GD.PrintErr("AStar did not find any pathing instances");
                return;
            }
            AStarLinker.addPoints(points.ToArray().To2D());

            // Visualizing the grid (mainly for testing)
            if (VisualizeGrid)
            {
                DrawGrid();
            }
            isMapGenerated = true;
        }

        /// <summary>
        /// Testing method to retrieve a point from the kdtree 'grid'
        /// </summary>
        /// <param name="position">The desired position</param>
        /// <returns>The closest position within the kdtree</returns>
        public Vector3 getPoint(Vector3 position)
        {
            return AStarLinker.getPoint(position).Item1;
        }

        /// <summary>
        /// Testing method to retrieve the closest neighbors of the passed point
        /// from the kdtree 'grid'
        /// </summary>
        /// <param name="position">The desired target point</param>
        /// <returns>The nearest neighbors within the kdtree</returns>
        public Vector3[] nearestNeighbors(Vector3 position)
        {
            return AStarLinker?.getNearNeighbors(position);
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Toggles the visibility of the grid visualization (used for testing)
        /// </summary>
        private void ToggleVisualization()
        {
            var vis = this.Get<MultiMeshInstance>("Debug/visualization");
            if (vis != null)
            {
                vis.Visible = !vis.Visible;
                return;
            }
            DrawGrid();
        }

        /// <summary>
        /// Visualizes the grid (used for testing)
        /// </summary>
        /// <param name="_gridSizeX">The width of the grid</param>
        /// <param name="_gridSizeY">The depth of the grid</param>
        private void DrawGrid()
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
            foreach (var p in points)
            {
                float d = ((p[4] - MinMovementPenalty) / (float)(MaxMovementPenalty - MinMovementPenalty));
                var color = new Color(1, 1, 1).LinearInterpolate(new Color(0, 0, 0), curve.Interpolate(d));
                color = (p[3] == 1) ? new Color(color.r, color.g, color.b, VisualAlpha) : new Color(1, 0, 0, VisualAlpha);
                multiMesh.SetInstanceTransform(cnt, new Transform(visualization.Transform.basis, new Vector3(p[0], p[1], p[2]) + (Vector3.Up * NodeRadius)));
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