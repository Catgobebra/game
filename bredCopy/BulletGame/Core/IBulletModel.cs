using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace BulletGame.Core
{
    public interface IBulletModel
    {
        Vector2 Position { get; }
        Vector2 Direction { get; }
        public float Speed { get; set; }
        public Color Color { get; set; }
        public bool IsPlayerBullet { get; set; }
        bool Active { get; set; }
        List<Vector2> GetVertices();
        void UpdatePosition(GameTime gameTime);
        void Reset(Vector2 position, Vector2 direction, float speed, Color color, bool isPlayerBullet);
    }
}