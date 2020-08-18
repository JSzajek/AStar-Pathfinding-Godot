using Godot;
using System;
using System.Threading.Tasks;
using AStar;

/// <summary>
/// Class representing the ground plane engulfing all of the chunks within.
/// Containing evehandling to wait until chunks have been generated/loaded.
/// </summary>
public class Ground : Spatial, IWaitable
{
	#region Fields
	// the delegate
	public delegate Task CompletedEventHandler(object sender, EventArgs args);

	public event CompletedEventHandler CompleteLoadingChunks;

	#endregion Fields

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="Ground"/> class.
	/// </summary>
	public override void _Ready()
	{
		PreloadScenes();
	}

	#endregion Constructors

	#region Properties

	/// <summary>
	/// Gets or sets whether the ground is currently loading.
	/// </summary>
	/// <value></value>
	public bool Loading {get; set;} = true;
	
	#endregion Properties

	#region Private Methods

	/// <summary>
	/// Asynchronously preloads the relevant information and assets.
	/// </summary>
	private async void PreloadScenes() 
	{
		if (this.Get<IAStar>("/root/Main/AStar_Linker") is IAStar astar) 
		{
			// Wait until astar is within scene tree and ready
			while(!astar.Ready) { await Task.Delay(10); }

			// After loading the obstacles now invoke generation of the navigation map
			await astar?.OnCompleteLoadingChunks(this, EventArgs.Empty);
		}
		Loading = false;
	}

	#endregion Private Methods
}
