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

        public bool IsExpired(Viewport viewport, float margin = 100f)
        {
            return Model.Position.X < -margin ||
                   Model.Position.X > viewport.Width + margin ||
                   Model.Position.Y < -margin ||
                   Model.Position.Y > viewport.Height + margin;
        }

        public bool CollidesWithPlayer(PlayerController player)
        {
            List<Vector2> bulletVertices = Model.GetVertices();
            List<Vector2> playerVertices = player.Model.GetVertices();
            return SATCollision.CheckCollision(bulletVertices, playerVertices);
        }

        public bool CollidesWithBullet(BulletController other)
        {
            List<Vector2> thisVertices = Model.GetVertices();
            List<Vector2> otherVertices = other.Model.GetVertices();
            return SATCollision.CheckCollision(thisVertices, otherVertices);
        }
    }
}