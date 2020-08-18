using Godot;
using System.Collections.Generic;
using System.Linq;
using System;
using Pathing;

namespace AStar
{
    /// <summary>
    /// Class representing AStar pathfinding and generating
    /// the corresponding grid map
    /// </summary>
    public class NavmeshAStar : BaseAStar
    {
        #region Constants

        private const float marginAdjust = 0.2f;
        private const float largeTriangleThreshold = 20f;
        private const float MIN_PATH_UPDATE_TIME = 0.2f;

        #endregion Constants

        #region Fields

        private enum CollisionType
        {
            WALKABLE, UNWALKABLE, UNDEFINED
        }

        private NavmeshAStarLinker AStarLinker;
        private bool _visualizeGrid, _visualizeNetwork;
        private int numOfMeshes;

        #endregion Fields

        #region Properties

        ///<inheritdoc/>
        [Export]
        public override bool VisualizeGrid
        {
            get => _visualizeGrid;
            set
            {
                _visualizeGrid = value;
                if (numOfMeshes > 0)
                {
                    ToggleVisualization();
                }
            }
        }

        /// <summary>
        /// Gets or set whether to visualize the network of 
        /// connections within the navmeshes. 
        /// </summary>
        [Export]
        public bool VisualizeNetwork
        {
            get => _visualizeNetwork;
            set
            {
                _visualizeNetwork = value;
                if (numOfMeshes > 0)
                {
                    ToggleNetworkVisualization();
                }
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NavmeshAStar"/> class.
        /// </summary>
        public override void _Ready()
        {
            // Get link to AStar dll
            AStarLinker = Navigator.DllLoader.GetNavmeshAStarLinker();

            // Initialize path request manager
            pathRequestManager = new PathRequestManager(AStarLinker, AddNode);

            // Create Path Update Timer
            pathTimer = new Timer();
            pathTimer.Name = "Path_Update_Timer";
            AddNode(pathTimer);
            pathTimer.Start(MIN_PATH_UPDATE_TIME);

            base._Ready();
        }

        #endregion Constructors

        #region Public Methods

        ///<inheritdoc/>
        public override void GenerateMap()
        {
            AStarLinker.setUpGrid(); //Initialization
            var grouped = this.Get<Spatial>("/root/Main/NavmeshScene").GetChildren<MeshInstance>().GroupBy(mesh => GetCollisionCategory(mesh.GetFirstChild<PhysicsBody>()));
            var walkables = grouped.Single(group => group.Key == CollisionType.WALKABLE);
            var unwalkables = grouped.Single(group => group.Key == CollisionType.UNWALKABLE);
            var holes = new List<MeshInstance>();
            foreach(var walkable in walkables) 
            {
                var walkableAabb = walkable.GetTransformedAabb();
                var surfaceHeight = walkable.GetSurfaceHeight();
                var topCorners = walkable.GetTopCorners();
                AStarLinker.startNavMesh(walkable.GetTopCorners().ToList());

                foreach(var unwalkable in unwalkables) 
                {
                    var unwalkableAabb = unwalkable.GetTransformedAabb();
                    if (unwalkableAabb.Intersects(walkableAabb)) {
                        var constrainedPoints = unwalkable.GetTopCornersConstrained(surfaceHeight, 1f);
                        var numOfInstersections = constrainedPoints.Where(point => walkableAabb.HasPoint(point)).Count();
                        if (numOfInstersections != 4) {
                            AStarLinker.clipEdgePolygon(constrainedPoints.ToList());
                        }
                        else {
                            holes.Add(unwalkable);
                        }
                    }
                }

                foreach(var hole in holes) 
                {
                    AStarLinker.clipHolePolygon(hole.GetTopCornersConstrained(surfaceHeight, 1f).ToList());
                }
                holes.Clear();
                AStarLinker.endNavMesh();
                numOfMeshes++;
            }

            // Visualizing the triangles (mainly for testing)
            if (VisualizeGrid) {
                VisualizeTriangles();
            }

            // Visualizing the connections (mainly for testing)
            if (VisualizeNetwork) {
                VisualizeConnections();
            }

            isMapGenerated = true;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Toggles the visibility of the grid visualization (used for testing)
        /// </summary>
        private void ToggleVisualization()
        {
            var vis = this.Get<Spatial>("Debug/visualization");
            if (vis != null)
            {
                vis.Visible = !vis.Visible;
                return;
            }
            VisualizeTriangles();
        }

        /// <summary>
        /// Toggles the visibility of the connections visualization (used for testing)
        /// </summary>
        private void ToggleNetworkVisualization()
        {
            var vis = this.Get<Spatial>("Debug/connections");
            if (vis != null)
            {
                vis.Visible = !vis.Visible;
                return;
            }
            VisualizeConnections();
        }

        /// <summary>
        /// Visualizes the connections of the graph within the navmesh (Used for testing)
        /// </summary>
        private void VisualizeConnections()
        {
            var connections = new Spatial { Name = "connections" };

            for (int i = 0; i < numOfMeshes; i++)
            {
                var triangles = AStarLinker.getDebug(i).ToList();
                var debug = new ImmediateGeometry();
                debug.Clear();
                debug.Begin(Mesh.PrimitiveType.Lines);
                foreach (var trig in triangles)
                {
                    (var v1, var v2, var v3) = trig.GetVertices();
                    (var m1, var m2, var m3) = trig.GetMidPoints();
                    // Regular Triangle
                    debug.AddVertex(v1 + (Vector3.Up));
                    debug.AddVertex(v2 + (Vector3.Up));
                    debug.AddVertex(v2 + (Vector3.Up));
                    debug.AddVertex(v3 + (Vector3.Up));
                    debug.AddVertex(v3 + (Vector3.Up));
                    debug.AddVertex(v1 + (Vector3.Up));
                    if (trig.GetArea() > largeTriangleThreshold)
                    {
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
                    else
                    {
                        debug.AddVertex(m1 + (Vector3.Up));
                        debug.AddVertex(m2 + (Vector3.Up));

                        debug.AddVertex(m2 + (Vector3.Up));
                        debug.AddVertex(m3 + (Vector3.Up));

                        debug.AddVertex(m3 + (Vector3.Up));
                        debug.AddVertex(m1 + (Vector3.Up));
                    }
                }
                debug.End();
                connections.AddChild(debug);
            }
            GetNode("Debug").AddChild(connections);
        }

        /// <summary>
        /// Visualizes the navmesh triangles (used for testing)
        /// </summary>
        private void VisualizeTriangles()
        {
            Spatial visualization = new Spatial() { Name = "visualization" };

            for (int i = 0; i < numOfMeshes; i++)
            {
                var triangles = AStarLinker.getDebug(i).ToList();
                var numLargeTrigs = triangles.Where(tri => tri.GetArea() > largeTriangleThreshold).Count();
                var midPoints = new MultiMeshInstance(); // Utilize a multimesh for efficiency
                var material = new SpatialMaterial()
                {
                    VertexColorUseAsAlbedo = true,
                    FlagsTransparent = true
                };
                var cubeMesh = new CubeMesh()
                {
                    Size = Vector3.One * 0.5f,
                    Material = material
                };
                var multiMesh = new MultiMesh()
                {
                    Mesh = cubeMesh,
                    TransformFormat = MultiMesh.TransformFormatEnum.Transform3d,
                    ColorFormat = MultiMesh.ColorFormatEnum.Float,
                    InstanceCount = (triangles.Count * 6) + numLargeTrigs,
                    VisibleInstanceCount = (triangles.Count * 6) + numLargeTrigs
                };

                var vertexPointColor = new Color(1f, 0, 0, VisualAlpha);
                var midPointColor = new Color(0, 0, 1f, VisualAlpha);
                var centriodPointColor = new Color(0, 1f, 0, VisualAlpha);
                var debug = new ImmediateGeometry();
                visualization.AddChild(debug);
                debug.Clear();
                debug.Begin(Mesh.PrimitiveType.Lines);
                int cnt = 0;
                foreach (var tri in triangles)
                {
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

                    if (tri.GetArea() > largeTriangleThreshold)
                    {
                        multiMesh.SetInstanceTransform(cnt, new Transform(midPoints.Transform.basis, tri.GetCentriod() + (Vector3.Up)));
                        multiMesh.SetInstanceColor(cnt, centriodPointColor);
                        cnt++;
                    }
                }
                midPoints.Multimesh = multiMesh;
                visualization.AddChild(midPoints);
                debug.End();
            }
            GetNode("Debug").AddChild(visualization);
        }

        /// <summary>
        /// Helper method to get the collision category of the passed physics body
        /// </summary>
        /// <param name="body">The passed physics body to get the category of</param>
        /// <returns>The collision type category of the physics body</returns>
        private CollisionType GetCollisionCategory(PhysicsBody body)
        {
            if (body == null)
            {
                return CollisionType.UNDEFINED;
            }
            if ((body.CollisionLayer & 16) != 0)
            {
                return CollisionType.WALKABLE;
            }
            else if ((body.CollisionLayer & 32) != 0)
            {
                return CollisionType.UNWALKABLE;
            }
            return CollisionType.UNDEFINED;
        }

        #endregion Private Methods
    }
}