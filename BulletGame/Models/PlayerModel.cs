using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace BulletGame
{
    public class PlayerModel
    {
        public Vector2 Position { get; private set; }
        public Vector2 Direction { get; private set; }
        public AttackPattern AdditionalAttack { get; set; } = new AttackPattern(
        shootInterval: 0.2f,
        bulletSpeed: 900f,
        bulletsPerShot: 1,
        true,
        strategy: new PlayerExplosiveShotStrategy(Color.Beige, Color.Indigo)
        );

        public float Speed { get; set; } = 500f;
        public float Size { get; set; } = 20f;
        public Color Color { get; set; } = Color.Red;
        public int Health { get; set; } = 800000;
        public Viewport Viewport { get; set; }
        public Rectangle GameArea { get; set; }

        public PlayerModel(Vector2 startPosition)
        {
            Position = startPosition;
            Direction = Vector2.UnitY;
        }

        public void UpdatePosition(Vector2 newPosition)
        {
            float halfSize = Size / 2;
            Position = new Vector2(
                MathHelper.Clamp(newPosition.X,
                    GameArea.Left + halfSize,
                    GameArea.Right - halfSize),
                MathHelper.Clamp(newPosition.Y,
                    GameArea.Top + halfSize,
                    GameArea.Bottom - halfSize)
            );
        }

        public void UpdateDirection(Vector2 newDirection)
        {
            Direction = Vector2.Normalize(newDirection);
        }

        public List<Vector2> GetVertices()
        {
            Vector2 tip = Position + Direction * Size;
            Vector2 perpendicular = new Vector2(-Direction.Y, Direction.X);
            Vector2 backLeft = Position - Direction * (Size / 2) + perpendicular * (Size / 2);
            Vector2 backRight = Position - Direction * (Size / 2) - perpendicular * (Size / 2);
            return new List<Vector2> { tip, backLeft, backRight };
        }
    }
}