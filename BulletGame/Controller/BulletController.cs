using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace BulletGame
{
    public class BulletController
    {
        public BulletModel Model { get; private set; }
        private readonly BulletView _view;

        public BulletController(BulletModel model, BulletView view)
        {
            Model = model;
            _view = view;
        }

        public void Update(GameTime gameTime)
        {
            Model.UpdatePosition(gameTime);
        }

        public void Draw(GraphicsDevice device)
        {
            if (Model.Active)
            {
                _view.Draw(device);
            }
        }

        public bool IsExpired(Rectangle gameArea, float margin = -20f)
        {
            float globalX = Model.Position.X;
            float globalY = Model.Position.Y;

            return globalX < gameArea.Left - margin ||
           globalX > gameArea.Right + margin ||
           globalY < gameArea.Top - margin ||
           globalY > gameArea.Bottom + margin;
        }

        public bool CollidesWithPlayer(PlayerController player)
        {
            List<Vector2> bulletVertices = Model.GetVertices();
            List<Vector2> playerVertices = player.Model.GetVertices();
            return SATCollision.CheckCollision(bulletVertices, playerVertices);
        }

        public bool CollidesWithEnemy(EnemyController enemy)
        {
            List<Vector2> bulletVertices = Model.GetVertices();
            List<Vector2> enemyVertices = enemy.Model.GetVertices();
            return SATCollision.CheckCollision(bulletVertices, enemyVertices);
        }

        public bool CollidesWithBullet(BulletController other)
        {
            List<Vector2> thisVertices = Model.GetVertices();
            List<Vector2> otherVertices = other.Model.GetVertices();
            return SATCollision.CheckCollision(thisVertices, otherVertices);
        }
    }
}