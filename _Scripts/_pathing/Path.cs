using Godot;

namespace Pathing
{
    /// <summary>
    /// Class representing a basic path of just waypoints
    /// </summary>
    public class Path {
        public Vector3[] points {get;}

        /// <summary>
        /// Constructor initializing a basic path
        /// </summary>
        /// <param name="points">The waypoints of the path</param>
        public Path(Vector3[] points) {
            this.points = points;
        }
    }
}
