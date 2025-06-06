using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace BulletGame.Core;
public interface IBulletPool
{
    IEnumerable<IBulletController> ActiveBullets { get; }
    void Return(IBulletController bullet);
    IBulletController GetBullet(Vector2 position, Vector2 direction, float speed, Color color, bool isPlayerBullet);
    void Cleanup();
    void ForceCleanup();
    int ActiveCount { get; }
    int InactiveCount { get; }
    int TotalCreated { get; }
}
