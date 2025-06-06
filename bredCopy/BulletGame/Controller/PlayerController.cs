using BulletGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace BulletGame
{
    public class PlayerController : IPlayer
    {
        public PlayerModel Model { get; private set; }
        private readonly PlayerView _view;

        public PlayerController(PlayerModel model, PlayerView view)
        {
            Model = model;
            _view = view;
        }

        public void TakeDamage(int damage)
        {
            Model.Health -= damage;
        }
        public AttackPattern AdditionalAttack
        {
            get => Model.AdditionalAttack;
            set => Model.AdditionalAttack = value;
        }

        public Color Color
        {
            get => Model.Color;
            set => Model.Color = value;
        }

        public int BonusHealth
        {
            get => Model.BonusHealth;
            set => Model.BonusHealth = value;
        }
        public Vector2 AimPosition
        {
            get => Model.AimPosition;
            set => Model.AimPosition = value;
        }

        public void StartMainAttack()
        {
            // TODO: Добавить логику начала основной атаки
        }

        public void StopMainAttack()
        {
            // TODO: Добавить логику остановки основной атаки
        }

        public int Health
        {
            get => Model.Health;
            set => Model.Health = value;
        }

        public Rectangle GameArea
        {
            get => Model.GameArea;
            set => Model.GameArea = value;
        }

        public Viewport Viewport
        {
            get => Model.Viewport;
            set => Model.Viewport = value;
        }

         
        public void PerformSpecialAttack()
        {
        }

        public Vector2 Position => Model.Position;

        public ICollider Collider => new PolygonCollider(Model.GetVertices());

        public void Update(GameTime gameTime)
        {
            if (Model.Viewport.Width == 0 || Model.Viewport.Height == 0)
                return;

            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);

            Model.UpdateDirection(mousePosition - Model.Position);


            Vector2 moveDirection = Vector2.Zero;
            if (keyboardState.IsKeyDown(Keys.W)) moveDirection.Y -= 1;
            if (keyboardState.IsKeyDown(Keys.S)) moveDirection.Y += 1;
            if (keyboardState.IsKeyDown(Keys.A)) moveDirection.X -= 1;
            if (keyboardState.IsKeyDown(Keys.D)) moveDirection.X += 1;

            if (moveDirection != Vector2.Zero)
            {
                moveDirection.Normalize();
                Vector2 newPosition = Model.Position + moveDirection * Model.Speed *
                                      (float)gameTime.ElapsedGameTime.TotalSeconds;
                Model.UpdatePosition(newPosition);
            }
        }

        public void SetViewport(Viewport viewport) => Model.Viewport = viewport;
        public void SetGameArea(Rectangle gameArea) => Model.GameArea = gameArea;
        public void SetPosition(Vector2 position) => Model.UpdatePosition(position);

        public void Draw(GraphicsDevice device) => _view.Draw();
    }
}
