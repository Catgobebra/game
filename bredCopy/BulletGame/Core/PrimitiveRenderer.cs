using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace BulletGame
{
    public interface IPrimitiveRenderer
    {
        void UpdateProjection();
        void DrawPoint(Vector2 position, Color color, float size = 3f);
        void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 2f);
        void DrawCircle(Vector2 position, int radius, int segments, Color color);
        void DrawBullets(IEnumerable<BulletRenderData> bullets);
        void DrawTriangle(Vector2 position, Vector2 direction, float size, Color color);
    }

    public struct BulletRenderData
    {
        public Vector2 Position;
        public Vector2 Direction;
        public Color Color;
        public float Length;
        public float Width;
    }

    public sealed class PrimitiveRenderer : IPrimitiveRenderer
    {
        private readonly GraphicsDevice _device;
        private readonly BasicEffect _effect;
        private readonly Dictionary<Color, List<VertexPositionColor>> _batchedVertices = new();
        private readonly PrimitiveGeometryCalculator _geometry = new();

        public PrimitiveRenderer(GraphicsDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));

            _effect = new BasicEffect(device)
            {
                VertexColorEnabled = true,
                World = Matrix.Identity,
                View = Matrix.Identity
            };

            UpdateProjection();
        }

        public void UpdateProjection()
        {
            _effect.Projection = Matrix.CreateOrthographicOffCenter(
                0, _device.Viewport.Width,
                _device.Viewport.Height, 0, 0, 1);
        }

        public void DrawPoint(Vector2 position, Color color, float size = 3f)
        {
            DrawCircle(position, (int)size, 8, color);
        }

        public void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 2f)
        {
            if (thickness <= 0) return;

            var vertices = _geometry.CalculateLineVertices(start, end, color, thickness);
            DrawPrimitives(vertices, PrimitiveType.TriangleList, vertices.Length / 3);
        }

        public void DrawCircle(Vector2 position, int radius, int segments, Color color)
        {
            if (radius <= 0 || segments < 3) return;

            var vertices = _geometry.CalculateCircleVertices(position, radius, segments, color);
            DrawPrimitives(vertices, PrimitiveType.TriangleList, segments);
        }

        public void DrawBullets(IEnumerable<BulletRenderData> bullets)
        {
            PrepareBatching();
            BatchBulletVertices(bullets);
            RenderBatches();
        }

        public void DrawTriangle(Vector2 position, Vector2 direction, float size, Color color)
        {
            var data = new BulletRenderData
            {
                Position = position,
                Direction = direction,
                Color = color,
                Length = size,
                Width = size
            };

            var vertices = _geometry.CalculateBulletVertices(data);
            DrawPrimitives(vertices, PrimitiveType.TriangleList, 1);
        }

        private void PrepareBatching()
        {
            foreach (var list in _batchedVertices.Values)
            {
                list.Clear();
            }
        }

        private void BatchBulletVertices(IEnumerable<BulletRenderData> bullets)
        {
            foreach (var bullet in bullets)
            {
                if (!_batchedVertices.TryGetValue(bullet.Color, out var vertices))
                {
                    vertices = new List<VertexPositionColor>();
                    _batchedVertices[bullet.Color] = vertices;
                }

                vertices.AddRange(_geometry.CalculateBulletVertices(bullet));
            }
        }

        private void RenderBatches()
        {
            foreach (var batch in _batchedVertices)
            {
                if (batch.Value.Count == 0) continue;

                _effect.DiffuseColor = batch.Key.ToVector3();
                DrawPrimitives(batch.Value.ToArray(), PrimitiveType.TriangleList, batch.Value.Count / 3);
            }
        }

        private void DrawPrimitives(VertexPositionColor[] vertices, PrimitiveType type, int primitiveCount)
        {
            if (vertices.Length == 0) return;

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _device.DrawUserPrimitives(type, vertices, 0, primitiveCount);
            }
        }
    }

    internal sealed class PrimitiveGeometryCalculator
    {
        public VertexPositionColor[] CalculateLineVertices(Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 direction = end - start;
            float length = direction.Length();

            if (length < float.Epsilon)
                return Array.Empty<VertexPositionColor>();

            direction.Normalize();
            Vector2 perpendicular = CalculatePerpendicular(direction) * thickness / 2f;
            Vector2 offset = perpendicular * 0.5f;

            return new VertexPositionColor[]
            {
                new(new Vector3(start + offset + perpendicular, 0), color),
                new(new Vector3(end + offset + perpendicular, 0), color),
                new(new Vector3(start + offset - perpendicular, 0), color),
                new(new Vector3(end + offset + perpendicular, 0), color),
                new(new Vector3(end + offset - perpendicular, 0), color),
                new(new Vector3(start + offset - perpendicular, 0), color)
            };
        }

        public VertexPositionColor[] CalculateCircleVertices(Vector2 position, int radius, int segments, Color color)
        {
            var vertices = new VertexPositionColor[segments * 3];

            for (int i = 0; i < segments; i++)
            {
                float angle1 = MathHelper.TwoPi * i / segments;
                float angle2 = MathHelper.TwoPi * (i + 1) / segments;

                Vector2 p1 = position + new Vector2(MathF.Cos(angle1), MathF.Sin(angle1)) * radius;
                Vector2 p2 = position + new Vector2(MathF.Cos(angle2), MathF.Sin(angle2)) * radius;

                vertices[i * 3] = new VertexPositionColor(new Vector3(position, 0), color);
                vertices[i * 3 + 1] = new VertexPositionColor(new Vector3(p1, 0), color);
                vertices[i * 3 + 2] = new VertexPositionColor(new Vector3(p2, 0), color);
            }

            return vertices;
        }

        public VertexPositionColor[] CalculateBulletVertices(BulletRenderData bullet)
        {
            Vector2 perpendicular = CalculatePerpendicular(bullet.Direction);
            Vector3 pos = new(bullet.Position, 0);

            return new VertexPositionColor[]
            {
                new(pos + new Vector3(bullet.Direction * bullet.Length, 0), bullet.Color),
                new(pos - new Vector3(bullet.Direction * (bullet.Length / 2), 0) +
                    new Vector3(perpendicular * (bullet.Width / 2), 0), bullet.Color),
                new(pos - new Vector3(bullet.Direction * (bullet.Length / 2), 0) -
                    new Vector3(perpendicular * (bullet.Width / 2), 0), bullet.Color)
            };
        }

        private Vector2 CalculatePerpendicular(Vector2 vector)
        {
            return new Vector2(-vector.Y, vector.X);
        }
    }
}