using Microsoft.Xna.Framework;

namespace BulletGame.Core;
public interface IBulletController
{
    bool IsPlayerBullet { get; }

    IBulletModel Model { get; }

    bool IsActive { get; }
    ICollider Collider { get; }
    void Update(GameTime gameTime);
    bool IsExpired(Rectangle gameArea);
}
