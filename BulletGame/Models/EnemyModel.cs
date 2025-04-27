using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

namespace BulletGame
{
    public class EnemyModel
    {
        public Vector2 Position { get; private set; }

        public AttackPattern AttackPattern { get; }
        public Color Color { get; }
        public float ShootTimer { get; private set; }

        public EnemyModel(Vector2 position, AttackPattern pattern, Color color)
        {
            Position = position;
            AttackPattern = pattern;
            Color = color;
            ShootTimer = pattern.ShootInterval;
        }

        public void UpdateShootTimer(float deltaTime)
        {
            ShootTimer -= deltaTime;
        }

        public void ResetShootTimer()
        {
            ShootTimer = AttackPattern.ShootInterval;
        }

        public List<Vector2> GetVertices()
        {
            List<Vector2> vertices = new List<Vector2>();
            float angleStep = MathHelper.TwoPi / 8;

            for (int i = 0; i < 8; i++)
            {
                float angle = angleStep * i;
                Vector2 offset = new Vector2(
                    30 * (float)Math.Cos(angle),
                    30 * (float)Math.Sin(angle)
                );
                vertices.Add(Position + offset);
            }

            return vertices;
        }
    }
}