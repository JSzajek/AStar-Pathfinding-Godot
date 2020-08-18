using Godot;
using System;
using Pathing;
using System.Threading.Tasks;

namespace AStar
{
    /// <summary>
    /// Interface representing the core methods of AStar class implementations,
    /// this is mainly for splitting the astar between three different
    /// implementation - otherwise serves no real purpose.
    /// </summary>
    public interface IAStar
    {
        #region Properties

        /// <summary>
        /// Gets whether the IAstar instance is ready within the scene.
        /// </summary>
        bool Ready { get; }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Adds nodes to the scene, for classes outside of the scene
        /// </summary>
        /// <param name="node">The node to add</param>
        void AddNode(Node node);

        /// <summary>
        /// Connects the target to the path timer.
        /// </summary>
        /// <param name="target">The target to connect</param>
        /// <param name="method">The string method</param>
        void ConnectToTimer(Godot.Object target, string method);

        /// <summary>
        /// Disconnects the target from the path timer.
        /// </summary>
        /// <param name="target">The target to disconnect</param>
        /// <param name="method">The string method</param>
        void DisconnectTimer(Godot.Object target, string method);

        /// <summary>
        /// Generates the astar map.
        /// </summary>
        void GenerateMap();

        /// <summary>
        /// Requests a path from the path request manager connected to the astar map.
        /// </summary>
        /// <param name="request">The path request</param>
        void RequestPath(PathRequest request);

        /// <summary>
        /// Complete Loading Chunk event handler, triggered when chunk has finished loading
        /// decorations and beginning the process of loading/generating the map.
        /// </summary>
        /// <param name="sender">The sender (parent Ground class)</param>
        /// <param name="eventArgs">Additional information</param>
        Task OnCompleteLoadingChunks(object sender, EventArgs eventArgs);

        #endregion Public Methods    
    }
}