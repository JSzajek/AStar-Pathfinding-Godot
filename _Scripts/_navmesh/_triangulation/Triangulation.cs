using Godot;
using System.Collections.Generic;

/// <summary>
/// Enum representing the winding order of a polygon
/// </summary>
public enum WindingOrder {
	ClockWise, CounterClockwise
}

/// <summary>
/// Class representing the algorithm for polygon ear clipping triangulation.
/// Described in this article: https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
/// </summary>
public class Triangulation {
	private readonly CyclingLinkedList<Vertex> polygonVertices = new CyclingLinkedList<Vertex>();
	private readonly CyclingLinkedList<Vertex> earVertices = new CyclingLinkedList<Vertex>();
	private readonly CyclingList<Vertex> convexVertices = new CyclingList<Vertex>();
	private readonly CyclingList<Vertex> reflexVertices = new CyclingList<Vertex>();

	/// <summary>
	/// Triangulates a polygon from the passed input vertices and
	/// winding order.
	/// </summary>
	/// <param name="inputVerts">The vertices of the polygon</param>
	/// <param name="desired">The desired winding order of the polygon output</param>
	/// <returns>The list of triangles representing the polygon</returns>
	public List<Triangle> Triangulate(Vector2[] inputVerts, WindingOrder desired){
		List<Triangle> triangles = new List<Triangle>();

		//make sure we have our vertices wound properly
		if (DetermineWindingOrder(inputVerts) == WindingOrder.ClockWise)
			inputVerts = ReverseWindingOrder(inputVerts);
		else
			inputVerts = (Vector2[])inputVerts.Clone();

		//clear all of the lists
		polygonVertices.Clear();
		earVertices.Clear();
		convexVertices.Clear();
		reflexVertices.Clear();

		for (int i = 0; i < inputVerts.Length; i++)
			polygonVertices.AddLast(new Vertex(inputVerts[i], i));

		FindConvexAndReflexVertices();
		FindEarVertices();

		while (polygonVertices.Count > 3 && earVertices.Count > 0)
			ClipNextEar(triangles);

		if (polygonVertices.Count == 3)
			triangles.Add(new Triangle(
				polygonVertices[0].Value,
				polygonVertices[1].Value,
				polygonVertices[2].Value));

		List<Triangle> result = new List<Triangle>();

		if (desired == WindingOrder.CounterClockwise)
		{
			for (int i = 0; i < triangles.Count; i++)
			{
				result.Add(new Triangle(triangles[i].A, triangles[i].B, triangles[i].C));
			}
		}
		else
		{
			for (int i = 0; i < triangles.Count; i++)
			{
				result.Add(new Triangle(triangles[i].C, triangles[i].B, triangles[i].A));
			}
		}
		return result;
	}

	/// <summary>
	/// Cuts a hole within the passed polygon representing by vertices constrained
	/// by the passed hole vertices
	/// </summary>
	/// <param name="shapeVerts">The polygon vertices</param>
	/// <param name="holeVerts">The hold vertices</param>
	/// <returns>The cut triangulated polygon vertices</returns>
	public Vector2[] CutHoleInShape(Vector2[] shapeVerts, Vector2[] holeVerts) {
			shapeVerts = EnsureWindingOrder(shapeVerts, WindingOrder.CounterClockwise);
			holeVerts = EnsureWindingOrder(holeVerts, WindingOrder.ClockWise);

			polygonVertices.Clear();
			earVertices.Clear();
			convexVertices.Clear();
			reflexVertices.Clear();

			for (int i = 0; i < shapeVerts.Length; i++)
				polygonVertices.AddLast(new Vertex(shapeVerts[i], i));

			CyclingList<Vertex> holePolygon = new CyclingList<Vertex>();
			for (int i = 0; i < holeVerts.Length; i++)
				holePolygon.Add(new Vertex(holeVerts[i], i + polygonVertices.Count));

			FindConvexAndReflexVertices();
			FindEarVertices();

			Vertex rightMostHoleVertex = holePolygon[0];
			foreach (Vertex v in holePolygon)
				if (v.Position.x > rightMostHoleVertex.Position.x)
					rightMostHoleVertex = v;

			List<Edge> segmentsToTest = new List<Edge>();
			for (int i = 0; i < polygonVertices.Count; i++)
			{
				Vertex a = polygonVertices[i].Value;
				Vertex b = polygonVertices[i + 1].Value;

				if ((a.Position.x > rightMostHoleVertex.Position.x || b.Position.x > rightMostHoleVertex.Position.x) &&
					((a.Position.y >= rightMostHoleVertex.Position.y && b.Position.y <= rightMostHoleVertex.Position.y) ||
					(a.Position.y <= rightMostHoleVertex.Position.y && b.Position.y >= rightMostHoleVertex.Position.y)))
					segmentsToTest.Add(new Edge(a, b));
			}

			float? closestPoint = null;
			Edge closestSegment = new Edge();
			foreach (Edge segment in segmentsToTest)
			{
				float? intersection = segment.IntersectsWithSegment(rightMostHoleVertex.Position, Vector2.Right);
				if (intersection != null)
				{
					if (closestPoint == null || closestPoint.Value > intersection.Value)
					{
						closestPoint = intersection;
						closestSegment = segment;
					}
				}
			}

			if (closestPoint == null)
				return shapeVerts;

			Vector2 I = rightMostHoleVertex.Position + Vector2.Right * closestPoint.Value;
			Vertex P = (closestSegment.A.Position.x > closestSegment.B.Position.x) 
				? closestSegment.A 
				: closestSegment.B;

			Triangle mip = new Triangle(rightMostHoleVertex, new Vertex(I, 1), P);

			List<Vertex> interiorReflexVertices = new List<Vertex>();
			foreach (Vertex v in reflexVertices)
				if (mip.ContainsPoint(v))
					interiorReflexVertices.Add(v);

			if (interiorReflexVertices.Count > 0)
			{
				float closestDot = -1f;
				foreach (Vertex v in interiorReflexVertices)
				{
					Vector2 d = (v.Position - rightMostHoleVertex.Position).Normalized();
					float dot = Vector2.Right.Dot(d);

					if (dot > closestDot)
					{
						closestDot = dot;
						P = v;
					}
				}
			}
			
			int mIndex = holePolygon.IndexOf(rightMostHoleVertex);
			int injectPoint = polygonVertices.IndexOf(P);

			for (int i = mIndex; i <= mIndex + holePolygon.Count; i++)
			{
				polygonVertices.AddAfter(polygonVertices[injectPoint++], holePolygon[i]);
			}
			polygonVertices.AddAfter(polygonVertices[injectPoint], P);

			Vector2[] newShapeVerts = new Vector2[polygonVertices.Count];
			for (int i = 0; i < polygonVertices.Count; i++)
				newShapeVerts[i] = polygonVertices[i].Value.Position;

			return newShapeVerts;
	}

	/// <summary>
	/// Helper method that ensures the polygon vertices are in the passed
	/// winding order.
	/// </summary>
	/// <param name="vertices">The polygon vertices</param>
	/// <param name="windingOrder">The desired winding order</param>
	/// <returns>The polygon vertices in the passed winding order</returns>
	public Vector2[] EnsureWindingOrder(Vector2[] vertices, WindingOrder windingOrder) {
		if (DetermineWindingOrder(vertices) != windingOrder){
			return ReverseWindingOrder(vertices);
		}
		return vertices;
	}

	/// <summary>
	/// Reverses the winding order of the passed polygon. For now just
	/// reversing the list (look into ensuring by sorting?)
	/// </summary>
	/// <param name="vertices">The vertices of the polygon</param>
	/// <returns>Reversed vertices</returns>
	public Vector2[] ReverseWindingOrder(Vector2[] vertices) {
		var newVertices = new Vector2[vertices.Length];

		newVertices[0] = vertices[0];
		for (int i = 1; i < newVertices.Length; i++) {
			newVertices[i] = vertices[vertices.Length - i];
		}
		return newVertices;
	}   

	/// <summary>
	/// Determines the winding order of the polygon by counting.
	/// </summary>
	/// <param name="vertices">The vertices of the polygon</param>
	/// <returns>The winding order of the polygon</returns>
	public WindingOrder DetermineWindingOrder(Vector2[] vertices) {
		int clockWiseCount = 0;
		int counterClockWiseCount = 0;
		Vector2 p1 = vertices[0];

		for (int i = 1; i < vertices.Length; i++)
		{
			Vector2 p2 = vertices[i];
			Vector2 p3 = vertices[(i + 1) % vertices.Length];

			Vector2 e1 = p1 - p2;
			Vector2 e2 = p3 - p2;

			if (e1.x * e2.y - e1.y * e2.x >= 0)
				clockWiseCount++;
			else
				counterClockWiseCount++;

			p1 = p2;
		}

		return (clockWiseCount > counterClockWiseCount)
			? WindingOrder.ClockWise
			: WindingOrder.CounterClockwise;
	}

	/// <summary>
	/// Clips the ears from the current polygon.
	/// </summary>
	/// <param name="triangles">The ears to clip</param>
	public void ClipNextEar(ICollection<Triangle> triangles) {
		Vertex ear = earVertices[0].Value;
		Vertex prev = polygonVertices[polygonVertices.IndexOf(ear) - 1].Value;
		Vertex next = polygonVertices[polygonVertices.IndexOf(ear) + 1].Value;
		triangles.Add(new Triangle(ear, next, prev));

		earVertices.RemoveAt(0);
		polygonVertices.RemoveAt(polygonVertices.IndexOf(ear));

		ValidateAdjacentVertex(prev);
		ValidateAdjacentVertex(next);
	}

	/// <summary>
	/// Validates the adjacent vertices of the passed vertex
	/// </summary>
	/// <param name="vertex">The vertex to check by</param>
	public void ValidateAdjacentVertex(Vertex vertex) {
		if (reflexVertices.Contains(vertex)) {
			if (IsConvex(vertex)) {
				reflexVertices.Remove(vertex);
				convexVertices.Add(vertex);
			}
		}

		if (convexVertices.Contains(vertex)) {
			bool wasEar = earVertices.Contains(vertex);
			bool isEar = IsEar(vertex);

			if (wasEar && !isEar) {
				earVertices.Remove(vertex);
			}
			else if (!wasEar && isEar) {
				earVertices.AddFirst(vertex);
			}
		}
	}

	/// <summary>
	/// Helper method that finds the convex and reflexive vertices
	/// </summary>
	 public void FindConvexAndReflexVertices() {
		 for (int i = 0; i < polygonVertices.Count; i++) {
			Vertex v = polygonVertices[i].Value;
			if (IsConvex(v)) {
				convexVertices.Add(v);
			}
			else {
				reflexVertices.Add(v);
			}
		}
	 }

	/// <summary>
	/// Helper method that finds the ear vertices
	/// </summary>
	public void FindEarVertices() {
		for (int i = 0; i < convexVertices.Count; i++) {
			Vertex c = convexVertices[i];
			if (IsEar(c)) {
				earVertices.AddLast(c);
			}
		}
	}

	/// <summary>
	/// Helper method that determines whether the passed vertex is
	/// an ear vertex.
	/// </summary>
	/// <param name="c">The passed vertex to check</param>
	/// <returns>Whether the vertex is an ear vertex</returns>
	public bool IsEar(Vertex c) {
		Vertex p = polygonVertices[polygonVertices.IndexOf(c) - 1].Value;
		Vertex n = polygonVertices[polygonVertices.IndexOf(c) + 1].Value;
		foreach (Vertex t in reflexVertices) {
			if (t.Equals(p) || t.Equals(c) || t.Equals(n)) {
				continue;
			}

			if (Triangle.ContainsPoint(p, c, n, t)) {
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Helper method that determines whether the passed vertex is a convex
	/// vertex.
	/// </summary>
	/// <param name="c">The vertex to check</param>
	/// <returns>Whether the vertex is convex</returns>
	public bool IsConvex(Vertex c) {
		var prev = polygonVertices[polygonVertices.IndexOf(c) - 1].Value;
		var next = polygonVertices[polygonVertices.IndexOf(c) + 1].Value;
		var d1 = (c.Position - prev.Position).Normalized();
		var d2 = (next.Position - c.Position).Normalized();
		Vector2 n2 = new Vector2(-d2.y, d2.x);
		
		return d1.Dot(n2) <= 0f;
	}
}
