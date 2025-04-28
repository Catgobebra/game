using BulletGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletGame
{
    public class EnemyController
    {
        public EnemyModel Model { get; private set; }
        private readonly EnemyView _view;

        public EnemyController(EnemyModel model, EnemyView view)
        {
            Model = model;
            _view = view;
        }

        public void Update(GameTime gameTime, OptimizedBulletPool bulletPool)
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
            _view.Draw(device);
        }
    }
}