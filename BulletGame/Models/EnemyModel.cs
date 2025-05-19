using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

namespace BulletGame
{
    public class EnemyModel
    {
        public Vector2 Position { get; set; }

        public int Health { get; set; } = 5;

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

        public EnemyModel(Vector2? position, AttackPattern attackPattern, Color crimson)
        {
            this.position = position;
            AttackPattern = attackPattern;
            this.crimson = crimson;
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

        private float _hitAnimationTimer;
        private Vector2? position;
        private Color crimson;
        private const float HitAnimationDuration = 1.0f; // 1 секунда длительность
        public float CurrentScale { get; private set; } = 1f;
        public float MaxScale { get; set; } = 1.5f; // Максимальное увеличение на 50%

        public void TriggerHitAnimation()
        {
            _hitAnimationTimer = HitAnimationDuration;
        }

        public void UpdateAnimation(GameTime gameTime)
        {
            if (_hitAnimationTimer > 0)
            {
                _hitAnimationTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Плавная интерполяция с использованием синусоиды
                float progress = _hitAnimationTimer / HitAnimationDuration;
                CurrentScale = 1f + (MaxScale - 1f) * (float)Math.Sin(progress * MathHelper.Pi);
            }
            else
            {
                CurrentScale = 1f;
            }
        }

    }
}