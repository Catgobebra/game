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
            _game._selectedMenuItem = (_game._selectedMenuItem + 1) % _game._menuItems.Length;
        }
        else if (keyboardState.IsKeyDown(Keys.Up) && !_prevKeyboardState.IsKeyDown(Keys.Up))
        {
            _game._selectedMenuItem = (_game._selectedMenuItem - 1 + _game._menuItems.Length) % _game._menuItems.Length;
        }
    }

    private void HandleSelection(KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyDown(Keys.Enter) && !_prevKeyboardState.IsKeyDown(Keys.Enter))
        {
            switch (_game._selectedMenuItem)
            {
                case 0:
                    _game._currentState = GameState.Playing;
                    _game.ResetGameState();
                    break;
                case 1:
                    if (_game._currentState == GameState.Menu)
                        _game.ResetGameState(_game.Lvl);
                    _game._currentState = GameState.Playing;
                    break;
                case 2:
                    if (_game._currentState == GameState.Menu)
                        _game.Exit();
                    else
                        _game._currentState = GameState.Menu;
                    break;
            }
        }
    }
}