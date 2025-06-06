using System.Collections.Generic;
using Microsoft.Xna.Framework;

public static class SATCollision
{
    public static bool CheckCollision(List<Vector2> polygonA, List<Vector2> polygonB)
    {
        foreach (var axis in GetAxes(polygonA))
        {
            if (!OverlapOnAxis(polygonA, polygonB, axis))
                return false;
        }

        foreach (var axis in GetAxes(polygonB))
        {
            if (!OverlapOnAxis(polygonA, polygonB, axis))
                return false;
        }

        return true;
    }

    private static List<Vector2> GetAxes(List<Vector2> polygon)
    {
        List<Vector2> axes = new List<Vector2>();
        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 edge = polygon[(i + 1) % polygon.Count] - polygon[i];
            Vector2 normal = new Vector2(-edge.Y, edge.X);
            normal.Normalize();
            axes.Add(normal);
        }
        return axes;
    }

    private static bool OverlapOnAxis(List<Vector2> polygonA, List<Vector2> polygonB, Vector2 axis)
    {
        Projection projA = Project(polygonA, axis);
        Projection projB = Project(polygonB, axis);
        return projA.Overlaps(projB);
    }

    private static Projection Project(List<Vector2> polygon, Vector2 axis)
    {
        float min = Vector2.Dot(polygon[0], axis);
        float max = min;
        for (int i = 1; i < polygon.Count; i++)
        {
            float dot = Vector2.Dot(polygon[i], axis);
            if (dot < min) min = dot;
            if (dot > max) max = dot;
        }
        return new Projection(min, max);
    }

    private struct Projection
    {
        public float Min;
        public float Max;

        public Projection(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public bool Overlaps(Projection other)
        {
            return !(Max < other.Min || other.Max < Min);
        }
    }
}