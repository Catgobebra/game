using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace BulletGame.Core
{
    public struct Projection
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

    public class PolygonCollider : ICollider
    {
        private List<Vector2> vertices;

        public PolygonCollider(List<Vector2> vertices)
        {
            this.vertices = vertices;
        }

        public void UpdateVertices(List<Vector2> newVertices)
        {
            vertices = newVertices;
        }

        public IEnumerable<Vector2> GetAxes()
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector2 edge = vertices[(i + 1) % vertices.Count] - vertices[i];
                Vector2 normal = new Vector2(-edge.Y, edge.X);
                if (normal != Vector2.Zero) normal.Normalize();
                yield return normal;
            }
        }

        public Projection Project(Vector2 axis)
        {
            float min = Vector2.Dot(vertices[0], axis);
            float max = min;
            for (int i = 1; i < vertices.Count; i++)
            {
                float dot = Vector2.Dot(vertices[i], axis);
                if (dot < min) min = dot;
                if (dot > max) max = dot;
            }
            return new Projection(min, max);
        }
    }

    public static class CollisionChecker
    {
        public static bool CheckCollision(ICollider colliderA, ICollider colliderB)
        {
            foreach (var axis in colliderA.GetAxes())
            {
                if (!OverlapOnAxis(colliderA, colliderB, axis))
                    return false;
            }

            foreach (var axis in colliderB.GetAxes())
            {
                if (!OverlapOnAxis(colliderA, colliderB, axis))
                    return false;
            }

            return true;
        }

        private static bool OverlapOnAxis(ICollider a, ICollider b, Vector2 axis)
        {
            var projA = a.Project(axis);
            var projB = b.Project(axis);
            return projA.Overlaps(projB);
        }
    }
}