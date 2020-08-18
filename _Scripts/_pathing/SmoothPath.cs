using Godot;
using AStar;

namespace Pathing
{
    /// <summary>
    /// Class representing a path with smoothing
    /// </summary>
    public class SmoothPath : Path {
        
        public Line[] lines;
        public int finishIndex;
        public int slowIndex;

        /// <summary>
        /// Constructor initializing a smoothed path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="lines"></param>
        /// <param name="finishIndex"></param>
        /// <param name="slowIndex"></param>
        /// <returns></returns>
        public SmoothPath(Vector3[] path, Line[] lines, int finishIndex, int slowIndex) : base(path) {
            this.lines = lines;
            this.finishIndex = finishIndex;
            this.slowIndex = slowIndex;
        }
    }
}