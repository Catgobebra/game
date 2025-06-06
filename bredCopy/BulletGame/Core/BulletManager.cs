using BulletGame.Core;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace BulletGame
{
    public class BulletManager
    {
        private readonly IBulletPool _bulletPool;
        private readonly IPlayer _player;
        private readonly IEnumerable<IEnemy> _enemies;
        private readonly Rectangle _gameArea;

        public BulletManager(
            IBulletPool bulletPool,
            IPlayer player,
            IEnumerable<IEnemy> enemies,
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
            UpdateBullets(gameTime, activeBullets);
            HandleCollisions(activeBullets);
        }

        private void UpdateBullets(GameTime gameTime, IEnumerable<IBulletController> bullets)
        {
            foreach (var bullet in bullets.Where(b => b.IsActive))
            {
                bullet.Update(gameTime);
                if (bullet.IsExpired(_gameArea))
                    _bulletPool.Return(bullet);
            }
        }

        private void HandleCollisions(IEnumerable<IBulletController> bullets)
        {
            HandleBulletCollisions(bullets);
            HandlePlayerBulletCollisions(bullets);
            HandleEnemyBulletCollisions(bullets);
        }

        private void HandleBulletCollisions(IEnumerable<IBulletController> bullets)
        {
            var playerBullets = bullets
                .Where(b => b.IsPlayerBullet && b.IsActive)
                .ToList();

            var enemyBullets = bullets
                .Where(b => !b.IsPlayerBullet && b.IsActive)
                .ToList();

            foreach (var pBullet in playerBullets)
            {
                foreach (var eBullet in enemyBullets.Where(b => b.IsActive))
                {
                    if (CollisionChecker.CheckCollision(pBullet.Collider, eBullet.Collider))
                    {
                        _bulletPool.Return(pBullet);
                        _bulletPool.Return(eBullet);
                        break;
                    }
                }
            }
        }

        private void HandlePlayerBulletCollisions(IEnumerable<IBulletController> bullets)
        {
            foreach (var bullet in bullets.Where(b =>
                b.IsPlayerBullet && b.IsActive))
            {
                foreach (var enemy in _enemies.ToList())
                {
                    if (CollisionChecker.CheckCollision(bullet.Collider, enemy.Collider))
                    {
                        enemy.TakeDamage(1);
                        _bulletPool.Return(bullet);
                        break;
                    }
                }
            }
        }

        private void HandleEnemyBulletCollisions(IEnumerable<IBulletController> bullets)
        {
            foreach (var bullet in bullets.Where(b =>
                !b.IsPlayerBullet && b.IsActive))
            {
                if (CollisionChecker.CheckCollision(bullet.Collider, _player.Collider))
                {
                    _player.TakeDamage(1);
                    _bulletPool.Return(bullet);
                }
            }
        }
    }
}