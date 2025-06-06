using BulletGame;
using Microsoft.Xna.Framework.Input;
using static BulletGame.Game1;

public class MenuInputHandler
{
    private readonly Game1 _game;
    private KeyboardState _prevKeyboardState;

    public MenuInputHandler(Game1 game)
    {
        _game = game;
    }

    public void Update()
    {
        var keyboardState = Keyboard.GetState();

        HandleNavigation(keyboardState);
        HandleSelection(keyboardState);

        _prevKeyboardState = keyboardState;
    }

    private void HandleNavigation(KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyDown(Keys.Down) && !_prevKeyboardState.IsKeyDown(Keys.Down))
        {
            _game._selectedMenuItem = (_game._selectedMenuItem + 1) % _game.MenuItems.Length;
        }
        else if (keyboardState.IsKeyDown(Keys.Up) && !_prevKeyboardState.IsKeyDown(Keys.Up))
        {
            _game._selectedMenuItem = (_game._selectedMenuItem - 1 + _game.MenuItems.Length) % _game.MenuItems.Length;
        }
    }

    private void HandleSelection(KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyDown(Keys.Enter) && !_prevKeyboardState.IsKeyDown(Keys.Enter))
        {
            if (_game._currentState == GameState.Menu)
            {
                switch (_game._selectedMenuItem)
                {
                    case 0:
                        _game._gameplayManager.ResetGameState(1);
                        _game._currentState = GameState.Playing;
                        _game.ResetAnimation();
                        break;
                    case 1:
                        _game._gameplayManager.ResetGameState(_game.Lvl);
                        _game._currentState = GameState.Playing;
                        _game.ResetAnimation();
                        break;
                    case 2:
                        _game.Exit();
                        break;
                }
            }
            else if (_game._currentState == GameState.Pause)
            {
                switch (_game._selectedMenuItem)
                {
                    case 0:
                        _game._currentState = GameState.Playing;
                        _game.ResetAnimation();
                        break;
                    case 1:
                        _game._gameplayManager.ResetGameState(_game.Lvl);
                        _game._currentState = GameState.Playing;
                        _game.ResetAnimation();
                        break;
                    case 2:
                        _game._currentState = GameState.Menu;
                        _game.ResetAnimation();
                        break;
                }
            }
        }
    }
}