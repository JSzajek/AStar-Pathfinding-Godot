using System;
using Pathing;

namespace AStar
{
    /// <summary>
    /// Interface representing the core methods of AStarLinker class implementations,
    /// this is mainly for splitting the astar between three different
    /// implementation - otherwise serves no real purpose.
    /// </summary>
    public interface IAStarLinker
    {
        /// <summary>
        /// Retrieves the path from the current AStar library.
        /// </summary>
        /// <param name="request">The path request</param>
        /// <param name="callback">The callback with the resulting path</param>
        void GetPath(PathRequest request, Action<PathResult> callback);
    }
}