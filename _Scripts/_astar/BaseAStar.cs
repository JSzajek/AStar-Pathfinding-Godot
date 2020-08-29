using Godot;
using System.Collections.Generic;
using Godot.Collections;
using System;
using System.Diagnostics;
using Pathing;
using System.Threading.Tasks;

namespace AStar
{
	public abstract class BaseAStar : Spatial, IAStar, IWaitable
	{
		#region Constants

		private const byte walkableMask = 4, unwalkableMask = 5;

		#endregion Constants

		#region Fields

		private bool _ready;
		private uint collisionMaskBit;
		private bool _visualizeGrid;

		protected Ground ground;
		protected PathRequestManager pathRequestManager;
		protected Timer pathTimer;
		protected string sceneMapDataPath;
		protected bool isMapGenerated;
		protected PhysicsDirectSpaceState direct_space;
		protected PhysicsShapeQueryParameters check_shape;

		// List of movement penalties
		protected List<TerrainType> terrains = new List<TerrainType>()
	{
		new TerrainType("Grass", 6, 8),
		new TerrainType("Path", 7, 3),
		new TerrainType("Sand", 8, 20)
	};

		#endregion Fields

		#region Properties

		/// <summary>
		/// Gets or sets the minimum movement penalty of the map.
		/// </summary>
		[Export]
		public int MinMovementPenalty { get; set; }

		/// <summary>
		/// Gets or sets the maximum movement penalty of the map.
		/// </summary>
		[Export]
		public int MaxMovementPenalty { get; set; } = 10;

		/// <summary>
		/// Gets or sets the obstacle proximity penalty of the map.
		/// Higher values result in more avoidance of obstacles.
		/// </summary>
		[Export]
		public int ObstacleProximityPenalty { get; set; } = 20;

		/// <summary>
		/// Gets or sets the height clearance of the map.
		/// </summary>
		[Export]
		public float HeightClearance { get; set; } = 4.5f;

		/// <summary>
		/// Gets or sets whether to visualize the grid - for debugging. 
		/// </summary>
		[Export]
		public abstract bool VisualizeGrid { get; set; }

		/// <summary>
		/// Gets or sets the transparency value of the debug visualization of the grid map.
		/// </summary>
		[Export]
		public float VisualAlpha { get; set; } = 1f;

		/// <summary>
		/// Gets or sets whether to trigger a grid map regeneration or just use the saved map.
		/// </summary>
		[Export]
		public bool RegenerateGrid { get; set; }

		///<inheritdoc/>
		public bool Loading { get; set; }

		///<inheritdoc/>
		public bool Ready => _ready;

		#endregion Properties

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseAStar"/> class.
		/// </summary>
		public override void _Ready()
		{
			// Get the physics params
			collisionMaskBit = GetCollisionBit(new List<byte>() { walkableMask });
			direct_space = GetWorld().DirectSpaceState;
			check_shape = new PhysicsShapeQueryParameters()
			{
				CollisionMask = (int)GetCollisionBit(new List<byte>() { unwalkableMask }),
				CollideWithAreas = false
			};

			ground = this.Get<Ground>("/root/Main/Ground");

			if (ground != null)
			{
				ground.CompleteLoadingChunks += OnCompleteLoadingChunks;
			}
			_ready = true;
		}

		#endregion Constructors

		#region Public Methods

		///<inheritdoc/>
		public Task OnCompleteLoadingChunks(object sender, EventArgs eventArgs)
		{
			if (!isMapGenerated)
			{
				Stopwatch timer = Stopwatch.StartNew();

				GenerateMap();
				timer.Stop();
				TimeSpan timespan = timer.Elapsed;
				GD.Print(String.Format("{0:00}:{1:00}:{2:00}", timespan.Minutes, timespan.Seconds, timespan.Milliseconds / 10));

				Loading = false;
			}
			return Task.CompletedTask;
		}

		/// <summary>
		/// Performs memory management when exiting the scene.
		/// </summary>
		public override void _ExitTree()
		{
			GD.Print("AStar Exiting the tree");
			pathRequestManager?.Destroy();
			pathTimer?.Free();
			check_shape?.Dispose();
		}

		///<inheritdoc/>
		public void AddNode(Node node)
		{
			AddChild(node);
		}

		///<inheritdoc/>
		public void ConnectToTimer(Godot.Object target, string method)
		{
			pathTimer.Connect("timeout", target, method);
		}

		///<inheritdoc/>
		public void DisconnectTimer(Godot.Object target, string method)
		{
			pathTimer.Disconnect("timeout", target, method);
		}

		///<inheritdoc/>
		public abstract void GenerateMap();

		///<inheritdoc/>
		public virtual void RequestPath(PathRequest request)
		{
			if (isMapGenerated)
			{
				pathRequestManager.RequestPath(request);
			}
		}

		#endregion Public Methods

		#region Protected Methods

		/// <summary>
		/// Helper method to find the movement penalty of the passed collider
		/// from the predefined penalty list.
		/// </summary>
		/// <param name="collider">The passed collider</param>
		/// <returns>The movement penatly of the collider, or 0 if not found</returns>
		protected int FindMovementPenalty(PhysicsBody collider)
		{
			foreach (var terrain in terrains)
			{
				if (collider.GetCollisionLayerBit(terrain.Mask))
				{
					return terrain.Penalty;
				}
			}
			return 0; // Should change to obstacle penalty?
		}

		/// <summary>
		/// Helper method that fires the ray cast at the specified position.
		/// </summary>
		/// <param name="center">The position to fire the ray cast at</param>
		/// <returns>Godot Dictionary of the resulting collision from the ray cast</returns>
		protected Dictionary FireRayCast(Vector3 center)
		{
			return direct_space.IntersectRay(center + (Vector3.Up * HeightClearance), center + (Vector3.Down * 100), null, collisionMaskBit, true);
		}

		/// <summary>
		/// Calculates the collision bit mask from a list of byte values.
		/// </summary>
		/// <param name="values">The list of byte values</param>
		/// <returns>The collision bit mask</returns>
		protected uint GetCollisionBit(List<byte> values)
		{
			uint bit = 0;
			foreach (var val in values)
			{
				bit += (uint)Mathf.Pow(2, val);
			}
			return bit;
		}

		#endregion Protected Methods
	}
}
