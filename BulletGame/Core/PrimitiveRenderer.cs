using BulletGame;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

public static class PrimitiveRenderer
{
    public static void DrawPoint(GraphicsDevice device, Vector2 position, Color color, float size = 3f)
    {
        DrawCircle(device, position, (int)size, 8, color);
    }

    private static BasicEffect _effect;
    private static Dictionary<Color, List<VertexPositionColor>> _batchedBullets = new();

    public static void Initialize(GraphicsDevice device)
    {
        _effect = new BasicEffect(device)
        {
            VertexColorEnabled = true,
            World = Matrix.Identity,
            View = Matrix.Identity,
            Projection = Matrix.CreateOrthographicOffCenter(
                0, device.Viewport.Width,
                device.Viewport.Height, 0, 0, 1)
        };
    }

    public static void UpdateProjection(GraphicsDevice device)
    {
        if (_effect != null)
        {
            _effect.Projection = Matrix.CreateOrthographicOffCenter(
                0, device.Viewport.Width,
                device.Viewport.Height, 0, 0, 1);
        }
    }

    public static void DrawAllBullets(GraphicsDevice device, List<BulletController> bullets)
    {
        if (bullets.Count == 0) return;

        _batchedBullets.Clear();

        foreach (var bullet in bullets)
        {
            if (!_batchedBullets.ContainsKey(bullet.Model.Color))
                _batchedBullets[bullet.Model.Color] = new List<VertexPositionColor>();

            AddBulletVertices(bullet, _batchedBullets[bullet.Model.Color]);
        }

        foreach (var batch in _batchedBullets)
        {
            if (batch.Value.Count == 0) continue;

            _effect.DiffuseColor = batch.Key.ToVector3();

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserPrimitives(
                    PrimitiveType.TriangleList,
                    batch.Value.ToArray(),
                    0,
                    batch.Value.Count / 3
                );
            }
        }
        _batchedBullets.Clear();
    }

    private static void AddBulletVertices(BulletController bullet, List<VertexPositionColor> vertices)
    {
        const float length = 20f;
        const float width = 12f;

        Vector2 perpendicular = new Vector2(-bullet.Model.Direction.Y, bullet.Model.Direction.X);
        Vector3 pos = new Vector3(bullet.Model.Position, 0);

        vertices.Add(new VertexPositionColor(
            pos + new Vector3(bullet.Model.Direction * length, 0),
            bullet.Model.Color
        ));

        vertices.Add(new VertexPositionColor(
            pos - new Vector3(bullet.Model.Direction * (length / 2), 0) +
            new Vector3(perpendicular * (width / 2), 0),
            bullet.Model.Color
        ));

        vertices.Add(new VertexPositionColor(
            pos - new Vector3(bullet.Model.Direction * (length / 2), 0) -
            new Vector3(perpendicular * (width / 2), 0),
            bullet.Model.Color
        ));
    }

    public static void DrawTriangle(GraphicsDevice device, Vector2 position,
                          Vector2 direction, float size, Color color)
    {
        VertexPositionColor[] vertices = new VertexPositionColor[3];
        Vector2 perpendicular = new Vector2(-direction.Y, direction.X);

        vertices[0] = new VertexPositionColor(
            new Vector3(position + direction * size, 0), color);

        vertices[1] = new VertexPositionColor(
            new Vector3(position - direction * size / 2 + perpendicular * size / 2, 0), color);

        vertices[2] = new VertexPositionColor(
            new Vector3(position - direction * size / 2 - perpendicular * size / 2, 0), color);

        DrawPrimitives(device, vertices, PrimitiveType.TriangleList, 1);
    }

    public static void DrawCircle(GraphicsDevice device, Vector2 position, int radius, int segments, Color color)
    {
        VertexPositionColor[] vertices = new VertexPositionColor[segments * 3];

        for (int i = 0; i < segments; i++)
        {
            float angle1 = MathHelper.TwoPi * i / segments;
            float angle2 = MathHelper.TwoPi * (i + 1) / segments;

            Vector2 p1 = position + new Vector2((float)Math.Cos(angle1) * radius, (float)Math.Sin(angle1) * radius);
            Vector2 p2 = position + new Vector2((float)Math.Cos(angle2) * radius, (float)Math.Sin(angle2) * radius);

            vertices[i * 3] = new VertexPositionColor(new Vector3(position, 0), color);
            vertices[i * 3 + 1] = new VertexPositionColor(new Vector3(p1, 0), color);
            vertices[i * 3 + 2] = new VertexPositionColor(new Vector3(p2, 0), color);
        }

        DrawPrimitives(device, vertices, PrimitiveType.TriangleList, segments);
    }

    public static void DrawBullet(GraphicsDevice device, Vector2 position, Vector2 direction,
                                float length, float width, Color color)
    {
        VertexPositionColor[] vertices = new VertexPositionColor[3];
        Vector2 perpendicular = new Vector2(-direction.Y, direction.X);

        vertices[0] = new VertexPositionColor(new Vector3(position + direction * length, 0), color);
        vertices[1] = new VertexPositionColor(new Vector3(position - direction * length / 2 + perpendicular * width / 2, 0), color);
        vertices[2] = new VertexPositionColor(new Vector3(position - direction * length / 2 - perpendicular * width / 2, 0), color);

        DrawPrimitives(device, vertices, PrimitiveType.TriangleList, 1);
    }

    private static void DrawPrimitives(GraphicsDevice device, VertexPositionColor[] vertices,
                                     PrimitiveType type, int primitiveCount)
    {
        _effect.World = Matrix.Identity;
        _effect.View = Matrix.Identity;

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            device.DrawUserPrimitives(type, vertices, 0, primitiveCount);
        }
    }
}