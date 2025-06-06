using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using static BulletGame.Game1;

namespace BulletGame
{
    public class MenuItem
    {
        public string Text { get; }

        public System.Action Action { get; }

        public MenuItem(string text, System.Action action)
        {
            Text = text;
            Action = action;
        }
    }

    public class Menu
    {
        public List<MenuItem> Items { get; } = new List<MenuItem>();
        public int SelectedIndex { get; set; } = 0;
    }

    public class MenuInputHandler
    {
        private KeyboardState _prevKeyboardState;
        private readonly Dictionary<GameState, Menu> _menus;
        private readonly Game1 _game;

        public MenuInputHandler(Game1 game)
        {
            _game = game;

            _menus = new Dictionary<GameState, Menu>
            {
                { GameState.Menu, CreateMainMenu() },
                { GameState.Pause, CreatePauseMenu() }
            };
        }

        private Menu CreateMainMenu()
        {
            var menu = new Menu();

            menu.Items.Add(new MenuItem("New Game", () => {
                _game.ResetGameState(1);
                _game.CurrentState = GameState.Playing;
                _game.ResetAnimation();
            }));

            menu.Items.Add(new MenuItem("Restart Level", () => {
                _game.ResetGameState(_game.CurrentLevel);
                _game.CurrentState = GameState.Playing;
                _game.ResetAnimation();
            }));

            menu.Items.Add(new MenuItem("Exit", () => _game.Exit()));

            return menu;
        }

        private Menu CreatePauseMenu()
        {
            var menu = new Menu();

            menu.Items.Add(new MenuItem("Resume", () => {
                _game.CurrentState = GameState.Playing;
                _game.ResetAnimation();
            }));

            menu.Items.Add(new MenuItem("Restart Level", () => {
                _game.ResetGameState(_game.CurrentLevel);
                _game.CurrentState = GameState.Playing;
                _game.ResetAnimation();
            }));

            menu.Items.Add(new MenuItem("Main Menu", () => {
                _game.CurrentState = GameState.Menu;
                _game.ResetAnimation();
            }));

            return menu;
        }

        public void Update()
        {
            if (!_menus.TryGetValue(_game.CurrentState, out var menu)) return;

            var keyboardState = Keyboard.GetState();

            HandleNavigation(menu, keyboardState);
            HandleSelection(menu, keyboardState);

            _prevKeyboardState = keyboardState;
        }

        private void HandleNavigation(Menu menu, KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.Down) && !_prevKeyboardState.IsKeyDown(Keys.Down))
            {
                menu.SelectedIndex = (menu.SelectedIndex + 1) % menu.Items.Count;
            }
            else if (keyboardState.IsKeyDown(Keys.Up) && !_prevKeyboardState.IsKeyDown(Keys.Up))
            {
                menu.SelectedIndex = (menu.SelectedIndex - 1 + menu.Items.Count) % menu.Items.Count;
            }
        }

        private void HandleSelection(Menu menu, KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.Enter) && !_prevKeyboardState.IsKeyDown(Keys.Enter))
            {
                menu.Items[menu.SelectedIndex]?.Action.Invoke();
            }
        }
    }
}