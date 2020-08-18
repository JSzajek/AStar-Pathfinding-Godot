using Godot;
using System;

namespace Pathing
{
    /// <summary>
    /// Struct representing a path request result
    /// </summary>
    public struct PathResult
    {
        public int hash;
        public Path path;
        public bool success;
        public Action<Path, bool> callback;

        /// <summary>
        /// Constructor initializing a path result
        /// </summary>
        /// <param name="path">The path of the path result</param>
        /// <param name="success">Whether it was a success</param>
        /// <param name="callback">The callback method to return the path to</param>
        public PathResult(Path path, bool success, int hash, Action<Path, bool> callback)
        {
            this.hash = hash;
            this.path = path;
            this.success = success;
            this.callback = callback;
        }
    }
}