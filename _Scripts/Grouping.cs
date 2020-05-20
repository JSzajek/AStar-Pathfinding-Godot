using Godot;
using System.Collections.Generic;

public class Grouping : Spatial
{
	private PhysicsDirectSpaceState direct_space;
	private PhysicsShapeQueryParameters check_shape;
	private List<float[]> points;

	public float nodeRadius = 0.5f;
	private const byte walkableMask = 4, unwalkableMask = 5;
	private uint collisionMaskBit;
	private int obstacleProximityPenalty = 20;

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
	private List<TerrainType> terrains;

	#endregion Terrain Parameters

	private List<ulong> walkables;

	private uint GetCollisionBit(List<byte> values) {
		uint bit = 0;
		foreach(var val in values) {
			bit += (uint)Mathf.Pow(2, val);
		}
		return bit;
	}

	private void Initializing() {
		terrains = new List<TerrainType>()
		{
			new TerrainType("Grass", 6, 8),
			new TerrainType("Path", 7, 3),
			new TerrainType("Sand", 8, 20)
		};

		collisionMaskBit = GetCollisionBit(new List<byte>() {walkableMask});
		direct_space = GetWorld().DirectSpaceState;
		check_shape = new PhysicsShapeQueryParameters() {CollisionMask = (int)GetCollisionBit(new List<byte>() {unwalkableMask})};
		var shape = new SphereShape() {Radius = nodeRadius};
		check_shape.SetShape(shape);
	}

	public List<float[]> GetPoints() {
		
		// No need to recalculate the points
		if (points != null) {
			return points;
		}

		Initializing();

		// Gather the four points of each of the walkable instances
		walkables = new List<ulong>();
		List<GraphicsPoint> graphicPoints = new List<GraphicsPoint>();
		foreach(var child in GetChildren()) {
			if (child is MeshInstance meshInstance && meshInstance.GetFirstChild<PhysicsBody>() is PhysicsBody body 
				&& body.GetCollisionLayerBit(walkableMask)) {

				walkables.Add(body.GetInstanceId());
				foreach (var corner in meshInstance.GetTopCorners()) {
					graphicPoints.Add(new GraphicsPoint(corner));
				}
			}
		}

		// Get the polygon from the a convex hull of the points.
		var polygon = new Polygon(graphicPoints);

		points = new List<float[]>();
		for (float i = polygon.minX; i < polygon.maxX; i += nodeRadius) {
			for (float j = polygon.minY; j < polygon.maxY; j += nodeRadius) {
				if (polygon.IsInside(new Vector3(i, 0, j)))
				{
					(var spot, var walkable, var penalty) = Spot(new Vector3(i, polygon.centroid.y, j));
					if (walkable) {
						points.Add(new float[] { spot.x, spot.y, spot.z, walkable ? 1 : 0, penalty});
					}
				}
			}
		}
		return points;
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

	private (Vector3, bool, int) Spot(Vector3 center) {
		var stackedPoints = new List<(Vector3, bool, int)>();
		if (FireRay(center) is Godot.Collections.Dictionary dict && dict.Contains("position") &&
			dict["position"] is Vector3 position && dict["collider"] is PhysicsBody collider && walkables.Contains(collider.GetInstanceId())) {
			check_shape.Transform = new Transform(check_shape.Transform.basis, center + (Vector3.Up * position.y));
			var temp = direct_space.IntersectShape(check_shape, 1);
			if (temp.Count == 0) {	
				return (position, true, FindMovementPenalty(collider));
			}
			return (position, false, obstacleProximityPenalty);
		}
		return (center, false, obstacleProximityPenalty);
	}

	private Godot.Collections.Dictionary FireRay(Vector3 center) {
		return direct_space.IntersectRay(center + (Vector3.Up * 100), center + (Vector3.Down * 100), null, collisionMaskBit, true);
	}
}