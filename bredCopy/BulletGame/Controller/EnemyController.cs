using BulletGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletGame
{
    public class EnemyController : IEnemy
    {
        public EnemyModel Model { get; private set; }
        private readonly EnemyView _view;
        private readonly PolygonCollider _collider;

        public ICollider Collider { get; private set; }
        public Vector2 Position => Model.Position;

        public EnemyController(EnemyModel model, EnemyView view)
        {
            Model = model;
            _view = view;
            _collider = new PolygonCollider(Model.GetVertices());
            Collider = _collider;
        }

        public void TakeDamage(int damage)
        {
            Model.Health -= damage;
            Model.TriggerHitAnimation();
        }

        public void Update(GameTime gameTime, IBulletPool bulletPool)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Model.UpdateShootTimer(deltaTime);
            Model.UpdateAnimation(gameTime);

            if (Model.ShootTimer <= 0)
            {
                Model.AttackPattern.Shoot(Model.Position, bulletPool);
                Model.ResetShootTimer();
            }
        }

        public void Draw(GraphicsDevice device)
        {
            _view.Draw();
        }
    }
}