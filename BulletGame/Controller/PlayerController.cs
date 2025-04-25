using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BulletGame
{
    public class PlayerController
    {
        public PlayerModel Model { get; private set; }
        private readonly PlayerView _view;

        public PlayerController(PlayerModel model, PlayerView view)
        {
            Model = model;
            _view = view;
        }

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

        public void SetViewport(Viewport viewport)
        {
            Model.Viewport = viewport;
        }

        public void Draw(GraphicsDevice device)
        {
            _view.Draw(device);
        }
    }

}