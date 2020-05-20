using Godot;
using System.Collections.Generic;

public class Polygon {
    private Stack<GraphicsPoint> stack;
    private GraphicsPoint p0;
    private GraphicsPoint[] points;
    public float minX, maxX, minY, maxY;
    public Vector3 centroid;

    public Polygon(List<GraphicsPoint> points) {

        this.points = ConvexHull(points);

        minX = minY = float.MaxValue;
        maxX = maxY = float.MinValue;
        var sumHeight = 0f;
        centroid = Vector3.Zero;
        float det = 0, tempDet = 0;
        int j = 0;
        for(int i = 0; i < points.Count; i++) {
            if (i + 1 == points.Count) { j = 0; }
            else { j++; }

            
            if (points[i].x < minX) {
                minX = points[i].x;
            }
            if (points[i].y < minY) {
                minY = points[i].y;
            }
            if (points[i].x > maxX) {
                maxX = points[i].x;
            }
            if (points[i].y > maxY) {
                maxY = points[i].y;
            }

            tempDet = points[i].x * points[j].y - points[j].x * points[i].y;
            det += tempDet;
            centroid.x += (points[i].x + points[j].x) * tempDet;
            sumHeight += (points[i].heightVal() ?? 0);
            centroid.z += (points[i].y + points[j].y) * tempDet;
        }
        centroid /= 3 * det;
        centroid.y = sumHeight / (float)points.Count;
    }

    private bool OnSegment(GraphicsPoint p, GraphicsPoint q, GraphicsPoint r) {
        if (q.x <= Mathf.Max(p.x, r.x) &&
            q.x >= Mathf.Min(p.x, r.x) &&
            q.y <= Mathf.Max(p.y, r.y) &&
            q.y >= Mathf.Min(p.y, r.y))
        {
            return true;
        }
        return false;
    }

    private bool Intersect(GraphicsPoint p1, GraphicsPoint q1,
                            GraphicsPoint p2, GraphicsPoint q2) {
        int o1 = Orientation(p1, q1, p2);
        int o2 = Orientation(p1, q1, q2);
        int o3 = Orientation(p2, q2, p1);
        int o4 = Orientation(p2, q2, q1);

        if (o1 != o2 && o3 != o4) {  // General case
            return true;
        }
        if (o1 == 0 && OnSegment(p1, p2, q1)) {
            return true;
        }
        if (o2 == 0 && OnSegment(p1, q2, q1)) {
            return true;
        }
        if (o3 == 0 && OnSegment(p2, p1, q2)) {
            return true;
        }
        if (o4 == 0 && OnSegment(p2, q1, q2)) {
            return true;
        }
        return false;
    }

    public bool IsInside(Vector3 vec) {
        GraphicsPoint point = new GraphicsPoint(vec); 
        if (points.Length < 3) {
            return false;
        }
        GraphicsPoint extreme = new GraphicsPoint(10000, point.y);
        
        int count = 0, i = 0;
        do {
            int next = (i+1) % points.Length;
            if (Intersect(points[i], points[next], point, extreme)) {
                if (Orientation(points[i], point, points[next]) == 0) {
                    return OnSegment(points[i], point, points[next]);
                }
                count++;
            }
            i = next;
        } while (i != 0);
        return count % 2 == 1;
    }

    public GraphicsPoint[] ConvexHull(List<GraphicsPoint> points) {
        if (points.Count < 0) {
            return null;
        }
        float ymin = points[0].y;
        int min = 0;
        for (int i = 1; i < points.Count; i++) {
            float y = points[i].y;
            if ((y < ymin) || (ymin == y && points[i].x < points[min].x)) {
                ymin = points[i].y;
                min = i;
            }
        }

        points[0].swapPoint(points[min]);
        p0 = points[0];

        var cmp = new PointComparer(p0);
        points.Sort(cmp); 

        int m = 1;
        for (int i = 1; i < points.Count; i++) {
            while (i < points.Count-1 && Orientation(p0, points[i], points[i+1]) == 0) {
                i++;
            }
            points[m] = points[i];
            m++;
        }

        if (m < 3) {
            return null;
        }

        stack = new Stack<GraphicsPoint>();
        stack.Push(points[0]);
        stack.Push(points[1]);
        stack.Push(points[2]);

        for (int i = 3; i < m; i++) {
            while (Orientation(NextToTop(), stack.Peek(), points[i]) != 2) {
                stack.Pop();
            }
            stack.Push(points[i]);
        }
        return stack.ToArray();
    }

    public GraphicsPoint NextToTop() {
        GraphicsPoint p = stack.Peek();
        stack.Pop();
        GraphicsPoint res = stack.Peek();
        stack.Push(p);
        return res;
    }

    public static int Orientation(GraphicsPoint p, GraphicsPoint q, GraphicsPoint r) {
        float val = ((q.y - p.y) * (r.x - q.x)) - ((q.x - p.x) * (r.y - q.y));
        if (val == 0) {
            return 0; // colinear
        }
        return val > 0 ? 1 : 2; // clockwise or counter-clockwise
    }

    public class PointComparer : IComparer<GraphicsPoint> {
        
        private GraphicsPoint globalPoint;

        public PointComparer(GraphicsPoint global) {
            this.globalPoint = global;
        }
        
        public int Compare(GraphicsPoint x, GraphicsPoint y) 
        { 
            int o = Orientation(globalPoint, x, y);
            if (o == 0) {
                return globalPoint.distSquared(y) >= globalPoint.distSquared(x) ? -1 : 1;
            }
            return o == 2 ? -1 : 1;
        }
    }
}

public struct GraphicsPoint {
    public float x;
    public float y;
    private Vector3? basis;

    public GraphicsPoint(Vector3 vec) {
        basis = vec;
        x = vec.x;
        y = vec.z;
    }

    public GraphicsPoint(float x, float y) {
        this.x = x;
        this.y = y;
        basis = null;
    }

    public float? heightVal() { return basis?.y; }

    public float distSquared(GraphicsPoint p2) {
        return ((x - p2.x) * (x - p2.x)) + ((y - p2.y) * (y - p2.y));
    }

    public void swapPoint(GraphicsPoint p2) {
        GraphicsPoint temp = this;
        this.x = p2.x;
        this.y = p2.y;
        p2.x = temp.x;
        p2.y = temp.y;
    }
}