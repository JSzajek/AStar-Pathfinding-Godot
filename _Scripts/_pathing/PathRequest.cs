using Godot;
using System;

namespace Pathing
{
    /// <summary>
    /// Struct representing a path request
    /// </summary>
    public struct PathRequest
    {
        #region Fields

        public Vector3 pathStart, pathEnd;
        public bool smooth;
        public float turnDist, stopDist;
        public int hash;
        public Action<Path, bool> callback;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Constructor initializing a basic path request's parameters
        /// </summary>
        /// <param name="callback">The calllback method to return the results to</param>
        /// <param name="start">The start point of the path</param>
        /// <param name="end">The ending point of the path</param>
        public PathRequest(Action<Path, bool> callback, Vector3 start, Vector3 end, int hash)
        {
            this.hash = hash;
            this.pathStart = start;
            this.pathEnd = end;
            this.callback = callback;
            smooth = false;
            stopDist = turnDist = 0.0f;
        }

        /// <summary>
        /// Overloaded constructor initializing a smoothed path request
        /// </summary>
        /// <param name="callback">The callback method to return the results to</param>
        /// <param name="start">The starting point of the path</param>
        /// <param name="end">The ending point of the path</param>
        /// <param name="turnDist">The turn radius (amount of turning)</param>
        /// <param name="stopDist">The stop distance for slowdown and walking on actual end point</param>
        /// <returns></returns>
        public PathRequest(Action<Path, bool> callback, Vector3 start, Vector3 end, int hash, float turnDist, float stopDist) : this(callback, start, end, hash) {
            this.smooth = true;
            this.turnDist = turnDist;
            this.stopDist = stopDist;
        }

        #endregion Constructors
    }
}
