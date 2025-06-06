using BulletGame;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using BulletGame.Controller;

public class BulletManager
{
    private readonly OptimizedBulletPool _bulletPool;
    private readonly PlayerController _player;
    private readonly List<EnemyController> _enemies;
    private readonly Rectangle _gameArea;

    public BulletManager(
        OptimizedBulletPool bulletPool,
        PlayerController player,
        List<EnemyController> enemies,
        Rectangle gameArea)
    {
        _bulletPool = bulletPool;
        _player = player;
        _enemies = enemies;
        _gameArea = gameArea;
    }

    public void Update(GameTime gameTime)
    {
        var activeBullets = _bulletPool.ActiveBullets.ToList();

        HandleBulletCollisions(activeBullets);
        UpdateBullets(gameTime, activeBullets);
    }

    private void HandleBulletCollisions(List<BulletController> activeBullets)
    {
        var playerBullets = activeBullets.Where(b => b.Model.IsPlayerBullet).ToList();
        var enemyBullets = activeBullets.Where(b => !b.Model.IsPlayerBullet).ToList();

        foreach (var pBullet in playerBullets)
        {
            foreach (var eBullet in enemyBullets)
            {
                if (pBullet.CollidesWithBullet(eBullet))
                {
                    _bulletPool.Return(pBullet);
                    _bulletPool.Return(eBullet);
                    break;
                }
            }
        }

        foreach (var bullet in playerBullets)
        {
            if (!bullet.Model.Active) continue;

            foreach (var enemy in _enemies.ToList())
            {
                if (bullet.CollidesWithEnemy(enemy))
                {
                    enemy.Model.Health -= 1;
                    enemy.Model.TriggerHitAnimation();
                    _bulletPool.Return(bullet);

                    if (enemy.Model.Health <= 0)
                    {
                        _enemies.Remove(enemy);
                    }
                }
            }
        }

        foreach (var bullet in enemyBullets)
        {
            if (!bullet.Model.Active) continue;

            if (bullet.CollidesWithPlayer(_player))
            {
                _player.Model.Health--;
                _bulletPool.Return(bullet);
            }
        }
    }

    private void UpdateBullets(GameTime gameTime, List<BulletController> activeBullets)
    {
        foreach (var bullet in activeBullets)
        {
            if (!bullet.Model.Active) continue;

            bullet.Update(gameTime);

            if (bullet.IsExpired(_gameArea))
                _bulletPool.Return(bullet);
        }
    }
}