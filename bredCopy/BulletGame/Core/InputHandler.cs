using BulletGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BulletGame
{
    /*public interface IPlayerController
    {
        Vector2 AimPosition { get; set; }

        void StartMainAttack();
        void StopMainAttack();
        void PerformSpecialAttack();

        Vector2 Position { get; }
    }*/

    /*public interface IGameController
    {
        void TogglePause();
        void RequestSkipWave();
    }*/

    public class InputHandler
    {
        private readonly IPlayer _playerController;
        private readonly IGameController _gameController;
        private readonly Rectangle _gameArea;

        public bool IsSkipRequested { get; set; } = false;

        private MouseState _prevMouseState;
        private KeyboardState _prevKeyboardState;

        public InputHandler(
            IPlayer playerController,
            IGameController gameController,
            Rectangle gameArea)
        {
            _playerController = playerController;
            _gameController = gameController;
            _gameArea = gameArea;

            _prevMouseState = Mouse.GetState();
            _prevKeyboardState = Keyboard.GetState();
        }

        public void Update()
        {
            var currentKeyboardState = Keyboard.GetState();
            var currentMouseState = Mouse.GetState();

            UpdateAimPosition(currentMouseState);
            HandleGameInput(currentKeyboardState);
            HandleSystemInput(currentKeyboardState);
            HandleMouseInput(currentMouseState);

            _prevKeyboardState = currentKeyboardState;
            _prevMouseState = currentMouseState;
        }

        private void UpdateAimPosition(MouseState mouseState)
        {
            _playerController.AimPosition = new Vector2(
                MathHelper.Clamp(mouseState.X, _gameArea.Left, _gameArea.Right),
                MathHelper.Clamp(mouseState.Y, _gameArea.Top, _gameArea.Bottom)
            );
        }

        public Vector2 GetDirectionAimPlayer()
        {
            Vector2 direction = new Vector2(
                Mouse.GetState().X - _playerController.Position.X,
                Mouse.GetState().Y - _playerController.Position.Y
            );
            direction.Normalize();
            return direction;
        }

        private void HandleGameInput(KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.Space) &&
                _prevKeyboardState.IsKeyUp(Keys.Space))
            {
                _playerController.PerformSpecialAttack();
            }

            if (keyboardState.IsKeyDown(Keys.Enter) &&
                _prevKeyboardState.IsKeyUp(Keys.Enter))
            {
                _gameController.RequestSkipWave();
            }
        }

        private void HandleSystemInput(KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.Escape) &&
                _prevKeyboardState.IsKeyUp(Keys.Escape))
            {
                _gameController.TogglePause();
            }
        }

        /*public Vector2 GetDirectionAimPlayer()
        {
            MouseState mouseState = Mouse.GetState();
            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);
            return Vector2.Normalize(mousePosition - _playerController.Model.Position);
        }*/


        private void HandleMouseInput(MouseState mouseState)
        {
            if (mouseState.LeftButton == ButtonState.Pressed &&
                _prevMouseState.LeftButton == ButtonState.Released)
            {
                _playerController.StartMainAttack();
            }
            else if (mouseState.LeftButton == ButtonState.Released &&
                     _prevMouseState.LeftButton == ButtonState.Pressed)
            {
                _playerController.StopMainAttack();
            }
        }
    }
}