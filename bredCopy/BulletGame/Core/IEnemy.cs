using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletGame.Core;
public interface IEnemy
{
    ICollider Collider { get; }
    Vector2 Position { get; }
    void TakeDamage(int damage);
    void Update(GameTime gameTime, IBulletPool bulletPool);
    void Draw(GraphicsDevice device);
}
