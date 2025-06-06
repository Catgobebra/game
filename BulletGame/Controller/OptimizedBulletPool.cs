using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace BulletGame.Controller
{
    public class OptimizedBulletPool
    {
        private readonly HashSet<BulletController> _active = new HashSet<BulletController>();
        private readonly Stack<BulletController> _inactive = new Stack<BulletController>();
        private const int MaxBullets = 6000;
        private int _totalCreated;

        public BulletController GetBullet(Vector2 position, Vector2 direction, float speed, Color color, bool isPlayerBullet)
        {
            BulletController bullet = null;

            if (_inactive.Count > 0)
            {
                bullet = _inactive.Pop();
            }
            else if (_totalCreated < MaxBullets)
            {
                var model = new BulletModel();
                bullet = new BulletController(model, new BulletView(model));
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

        public void Return(BulletController bullet)
        {
            if (bullet == null || !_active.Contains(bullet)) return;

            bullet.Model.Active = false;
            _active.Remove(bullet);
            _inactive.Push(bullet); ;
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
            foreach (var bullet in _active)
            {
                bullet.Model.Active = false;
            }
            Cleanup();
        }

        public IEnumerable<BulletController> ActiveBullets => _active;

        public int ActiveCount => _active.Count;
        public int InactiveCount => _inactive.Count;
        public int TotalCreated => _totalCreated;
    }
}