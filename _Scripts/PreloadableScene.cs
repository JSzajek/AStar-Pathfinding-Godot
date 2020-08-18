using Godot;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Class representing a scene requiring pre-loading before
/// the player enters the scene.
/// </summary>
public class PreloadableScene : Node
{
    #region Fields

    /// <summary>
    /// Gets the collection of nodes that need to load.
    /// </summary>
    /// <value></value>
    public List<IWaitable> LoadingNodes { get; }

    #endregion Fields

    #region Constructors

    /// <summary>
	/// Initializes a new instance of the <see cref="PreloadableScene"/> class.
	/// </summary>
    public PreloadableScene() 
    {
        LoadingNodes = this.GetChildren<IWaitable>(true);
    }

    #endregion Constructors

    #region Public Methods

    /// <summary>
    /// Polls whether the loading scenes are finished loading.
    /// </summary>
    /// <returns></returns>
    public bool Poll() => GetStageCount() ==  GetStage();

    /// <summary>
    /// Retrieves the total number of nodes that need to be loaded.
    /// </summary>
    /// <returns>The total number of preloading nodes</returns>
    public int GetStageCount() => LoadingNodes.Count;

    /// <summary>
    /// Retrieves the current number of loading nodes.
    /// </summary>
    /// <returns>The current number of loaded nodes</returns>
    public int GetStage() => LoadingNodes.Count(node => !node.Loading);

    #endregion Public Methods
}
