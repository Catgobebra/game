using BulletGame.Core;
using Microsoft.Xna.Framework;

namespace BulletGame
{
    public class BulletController : IBulletController
    {
        public IBulletModel Model { get; private set; }
        private readonly PolygonCollider _collider;

        public bool IsPlayerBullet => Model.IsPlayerBullet;
        public bool IsActive => Model.Active;
        public ICollider Collider => _collider;

        public BulletController(BulletModel model)
        {
            Model = model;
            _collider = new PolygonCollider(Model.GetVertices());
        }

        public void Update(GameTime gameTime)
        {
            Model.UpdatePosition(gameTime);
            _collider.UpdateVertices(Model.GetVertices());
        }

        public bool IsExpired(Rectangle gameArea)
        {
            float globalX = Model.Position.X;
            float globalY = Model.Position.Y;
            float margin = -20f;

            return globalX < gameArea.Left - margin ||
                   globalX > gameArea.Right + margin ||
                   globalY < gameArea.Top - margin ||
                   globalY > gameArea.Bottom + margin;
        }
    }
}