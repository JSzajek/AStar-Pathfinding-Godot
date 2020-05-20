using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Class reprenting a bi directional search graph.
/// Reference: https://www.koderdojo.com/blog/breadth-first-search-and-shortest-path-in-csharp-and-net-core
/// </summary>
/// <typeparam name="T">The type of object in the graph</typeparam>
public class BiDirectionalSearch<T> {
    private Graph<T> graph;

    /// <summary>
    /// Constructor of the bi directional graph
    /// </summary>
    /// <param name="vertices">The vertices of the graph</param>
    /// <param name="edges">The edges of the graph</param>
    public BiDirectionalSearch(IEnumerable<T> vertices, IEnumerable<Tuple<T,T>> edges) {
        graph = new Graph<T>(vertices, edges);
    }

    /// <summary>
    /// Finds the shortest path from the start object to the target object
    /// </summary>
    /// <param name="start">The start object</param>
    /// <param name="target">The target object</param>
    /// <returns>The shortest path between the start and target</returns>
    public IEnumerable<T> ShortestPath(T start, T target) {
        var previous = new Dictionary<T, T>();
        var queue = new Queue<T>();
        queue.Enqueue(start);

        // Currently finding all paths from start - look into stopping at target
        while(queue.Count != 0) {
            var vert = queue.Dequeue();
            foreach(var neighbor in graph.Adjacencies[vert]) {
                if (previous.ContainsKey(neighbor)) {
                    continue;
                }
                previous[neighbor] = vert;
                queue.Enqueue(neighbor);
            }
        }

        var path = new List<T>();
        var current = target;
        while(!current.Equals(start)) {
            path.Add(current);
            current = previous[current];
        }
        path.Add(start);
        path.Reverse(); // Path is currently backwards (end to start)
        return path;
    }

    /// <summary>
    /// Class representing a graph
    /// </summary>
    /// <typeparam name="K">The type of object in the graph</typeparam>
    private class Graph<K> {
        public Dictionary<K, HashSet<K>> Adjacencies { get; }
        /// <summary>
        /// Constructor initializing the adjacencies
        /// </summary>
        public Graph() {
            Adjacencies = new Dictionary<K, HashSet<K>>();
        }

        /// <summary>
        /// Constructor initializing the graph with the passed vertices and edges
        /// </summary>
        /// <param name="vertices">The vertices of the graph</param>
        /// <param name="edges">The edges of the graph</param>
        public Graph(IEnumerable<K> vertices, IEnumerable<Tuple<K,K>> edges) : this() {
            foreach(var vert in vertices) {
                AddVertex(vert);
            }
            foreach(var edge in edges) {
                AddEdge(edge);
            }
        }

        /// <summary>
        /// Adds a vertex to the graph.
        /// </summary>
        /// <param name="vertex">Vertex to add</param>
        public void AddVertex(K vertex) {
            Adjacencies[vertex] = new HashSet<K>();
        }

        /// <summary>
        /// Adds an edge to the adjacencies of the graph
        /// </summary>
        /// <param name="edge">The edge to add</param>
        public void AddEdge(Tuple<K,K> edge) {
            if (Adjacencies.ContainsKey(edge.Item1) && Adjacencies.ContainsKey(edge.Item2)) {
                Adjacencies[edge.Item1].Add(edge.Item2);
                Adjacencies[edge.Item2].Add(edge.Item1);
            }
        }
    }
}