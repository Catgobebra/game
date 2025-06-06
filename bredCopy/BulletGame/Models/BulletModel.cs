using BulletGame.Core;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace BulletGame
{
    public class BulletModel : IBulletModel
    {
        public Vector2 Position { get; private set; }
        public Vector2 Direction { get; private set; }
        public float Speed { get; set; }
        public Color Color { get; set; }
        public bool IsPlayerBullet { get; set; }
        public bool Active { get; set; }

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