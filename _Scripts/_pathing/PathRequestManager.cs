using Godot;
using System.Collections.Generic;
using GTimer = Godot.Timer;
using Thread = System.Threading.Thread;
using Task = System.Threading.Tasks.Task;
using AStar;

namespace Pathing
{
	/// <summary>
	/// Path Request manager class representing managment
	/// of queued pathrequest and distributing the results
	/// back after a constant poll rate.
	/// </summary>
	public class PathRequestManager : FreeingNode
	{
		// How often to check if results are done calculating
		private const float POLL_RATE = 0.3f;
		
		private IAStarLinker AStarLinker;
		private Queue<PathResult> results = new Queue<PathResult>();
		private GTimer timer;
		private Dictionary<int, Thread> table;

		/// <summary>
		/// Constructor to initialize the pathrequest manager parameters
		/// </summary>
		/// <param name="linker">AStar algorithm linker that calculates the pathes</param>
		public PathRequestManager(IAStarLinker linker)
		{
			Name = "Path Request Manager";
			table = new Dictionary<int, Thread>();
			AStarLinker = linker;
			timer = new GTimer() { Autostart = true, WaitTime = POLL_RATE };
			AddChild(timer);
			timer.Connect("timeout", this, "poll_time_out");
		}

		/// <summary>
		/// Performs memory managment freeing remaining nodes and closing threads.
		/// </summary>
		public void Destroy() {
			// Wait for any on going threads to join main thread before destroying
			foreach(var s in table.Keys) {
				if (table[s].IsAlive) {
					table[s].Join(1500);
				}
			}
			timer.QueueFree();
			base._ExitTree();
		}

		/// <summary>
		/// Polling method connected to timer timeout signal.
		/// For returning the path results.
		/// </summary>
		public void poll_time_out() 
		{
			if (results.Count > 0) {
				lock(results) {
					while (results.Count > 0) {
						PathResult result = results.Dequeue();
						result.callback(result.path, result.success);
					}
				}
			}
		}

		/// <summary>
		/// Requesting method to request a path request.
		/// </summary>
		/// <param name="request">The path request (basic/smooth)</param>
		public void RequestPath(PathRequest request)
		{
			// Prevent sequential pathrequest tasks from piling up.
			if (currentTasksHash.Contains(request.hash)) {
				GD.Print("Already processing hash " + request.hash);
				return;
			}

			var tesk = Task.Run(() => AStarLinker.GetPath(request, FinishedProcessingPath));
			currentTasksHash.Add(request.hash);
		}

		private HashSet<int> currentTasksHash = new HashSet<int>();

		/// <summary>
		/// Callback method when path finished and will be enqueued in return
		/// queue.
		/// </summary>
		/// <param name="result"></param>
		public void FinishedProcessingPath(PathResult result)
		{
			lock(results) {
				if (currentTasksHash.Contains(result.hash)) {
					results.Enqueue(result);
					currentTasksHash.Remove(result.hash);
				}
				else {
					//GD.Print("Couldn't find in table " + result.hash);
				}
			}
		}
	}
}
