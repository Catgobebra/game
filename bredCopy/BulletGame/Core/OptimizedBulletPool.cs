using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics; 

namespace BulletGame.Core
{
    public interface IBulletFactory
    {
        IBulletController CreateBullet();
    }

    public class OptimizedBulletPool : IBulletPool
    {
        private readonly HashSet<IBulletController> _active = new();
        private readonly Stack<IBulletController> _inactive = new();
        private readonly IBulletFactory _factory;
        private const int MaxBullets = 6000;
        private int _totalCreated;

        public OptimizedBulletPool(IBulletFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public IBulletController GetBullet(Vector2 position, Vector2 direction, float speed, Color color, bool isPlayerBullet)
        {
            IBulletController bullet = null;

            if (_inactive.Count > 0)
            {
                bullet = _inactive.Pop();
            }
            else if (_totalCreated < MaxBullets)
            {
                bullet = _factory.CreateBullet();
                _totalCreated++;
            }

            if (bullet != null)
            {
                bullet.Model.Reset(position, direction, speed, color, isPlayerBullet);
                bullet.Model.Active = true;
                _active.Add(bullet);
            }

            return bullet;
        }

        public void Return(IBulletController bullet)
        {
            if (bullet == null || !_active.Contains(bullet)) return;

            bullet.Model.Active = false;
            _active.Remove(bullet);
            _inactive.Push(bullet);
        }

        public void Cleanup()
        {
            var expired = _active.Where(b => !b.Model.Active).ToList();
            foreach (var bullet in expired)
            {
                Return(bullet);
            }
        }

        public void ForceCleanup()
        {
            foreach (var bullet in _active.ToList())
            {
                Return(bullet);
            }
        }

        public IEnumerable<IBulletController> ActiveBullets => _active;
        public int ActiveCount => _active.Count;
        public int InactiveCount => _inactive.Count;
        public int TotalCreated => _totalCreated;
    }
}