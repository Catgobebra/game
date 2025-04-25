using Microsoft.Xna.Framework;

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
    }
}