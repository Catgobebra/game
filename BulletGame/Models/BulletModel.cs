using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace BulletGame
{
    public class BulletModel
    {
        public Vector2 Position { get; private set; }
        public Vector2 Direction { get; private set; }
        public float Speed { get; private set; }
        public Color Color { get; private set; }
        public bool Active { get; set; }
        public bool IsPlayerBullet { get; private set; }

        public void Reset(Vector2 position, Vector2 direction,
                        float speed, Color color, bool isPlayerBullet)
        {
            Position = position;
            Direction = direction;
            Speed = speed;
            Color = color;
            Active = true;
            IsPlayerBullet = isPlayerBullet;
        }

        public void UpdatePosition(GameTime gameTime)
        {
            if (!Active) return;
            Position += Direction * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public List<Vector2> GetVertices()
        {
            const float length = 20f;
            const float width = 12f;
            Vector2 tip = Position + Direction * length;
            Vector2 perpendicular = new Vector2(-Direction.Y, Direction.X);
            Vector2 backLeft = Position - Direction * (length / 2) + perpendicular * (width / 2);
            Vector2 backRight = Position - Direction * (length / 2) - perpendicular * (width / 2);
            return new List<Vector2> { tip, backLeft, backRight };
        }
    }
}