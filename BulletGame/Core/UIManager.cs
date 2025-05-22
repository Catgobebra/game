using BulletGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace BulletGame
{
    public class UIManager
    {
        private readonly SpriteFont _textBlock;
        private readonly SpriteFont _japanTextBlock;
        private readonly SpriteFont _miniTextBlock;
        public readonly SpriteFont _japanSymbol;
        private readonly SpriteBatch _spriteBatch;
        private readonly GraphicsDevice _graphicsDevice;

        public PlayerController _player;
        private readonly List<EnemyController> _enemies;
        private readonly List<Bonus> _bonuses;
        private readonly OptimizedBulletPool _bulletPool;
        private readonly Rectangle _gameArea;

        public UIManager(
            SpriteFont textBlock,
            SpriteFont japanTextBlock,
            SpriteFont miniTextBlock,
            SpriteFont japanSymbol,
            SpriteBatch spriteBatch,
            GraphicsDevice graphicsDevice,
            PlayerController player,
            List<EnemyController> enemies,
            List<Bonus> bonuses,
            OptimizedBulletPool bulletPool,
            Rectangle gameArea)
        {
            _textBlock = textBlock;
            _japanTextBlock = japanTextBlock;
            _miniTextBlock = miniTextBlock;
            _japanSymbol = japanSymbol;
            _spriteBatch = spriteBatch;
            _graphicsDevice = graphicsDevice;
            _player = player;
            _enemies = enemies;
            _bonuses = bonuses;
            _bulletPool = bulletPool;
            _gameArea = gameArea;
        }

        public void DrawGameElements(bool battleStarted, string name, Color nameColor, int lvl)
        {
            DrawGameAreaBorders();
            DrawUI(battleStarted, name, nameColor, lvl);
        }

        private void DrawGameAreaBorders()
        {
            int borderThickness = 10;

            PrimitiveRenderer.DrawLine(
                _graphicsDevice,
                new Vector2(_gameArea.Left, _gameArea.Top),
                new Vector2(_gameArea.Right, _gameArea.Top),
                Color.White,
                borderThickness
            );

            PrimitiveRenderer.DrawLine(
                 _graphicsDevice,
                 new Vector2(_gameArea.Left, _gameArea.Bottom),
                 new Vector2(_gameArea.Right, _gameArea.Bottom),
                 Color.White,
                 borderThickness
             );


            PrimitiveRenderer.DrawLine(
                _graphicsDevice,
                new Vector2(_gameArea.Left, _gameArea.Top),
                new Vector2(_gameArea.Left, _gameArea.Bottom),
                Color.White,
                borderThickness
            );

            PrimitiveRenderer.DrawLine(
                _graphicsDevice,
                new Vector2(_gameArea.Right, _gameArea.Top),
                new Vector2(_gameArea.Right, _gameArea.Bottom),
                Color.White,
                borderThickness
            );
        }

        private void DrawUI(bool battleStarted, string name, Color nameColor, int lvl)
        {
            _spriteBatch.Begin();

            // Отрисовка здоровья игрока
            _spriteBatch.DrawString(_textBlock, $"{_player.Model.Health} ед. Ки",
                new Vector2(50, 50), Color.White);

            // Отрисовка названия бонуса
            _spriteBatch.DrawString(_textBlock, name,
                new Vector2(480, 50), nameColor);

            // Отрисовка уровня
            _spriteBatch.DrawString(_textBlock, $"{lvl} Ступень",
                new Vector2(880, 50), Color.White);

            // Японские символы
            _spriteBatch.DrawString(_japanTextBlock, "せ\nん\nし",
                new Vector2(1750, 400), Color.White);
            _spriteBatch.DrawString(_japanTextBlock, $"だいみょう", new Vector2(800, 940), Color.White);
            _spriteBatch.DrawString(_japanTextBlock, $"ぶ\nし", new Vector2(100, 400), Color.White);

            if (!battleStarted)
            {
                DrawPreBattleText();
            }

            _spriteBatch.End();
        }

        private void DrawPreBattleText()
        {
            string text = "Я постиг, что Путь Самурая это смерть." +
                "В ситуации или или без колебаний выбирай смерть.\nЭто нетрудно. Исполнись решимости и действуй." +
                "Только малодушные оправдывают себя\nрассуждениями о том, что умереть, не достигнув цели, означает" +
                "умереть собачьей смертью.\nСделать правильный выбор в ситуации или или практически невозможно." +
                "Все мы желаем\nжить, и поэтому неудивительно, что каждый пытается найти оправдание, чтобы не умирать\n" +
                "Но если человек не достиг цели и продолжает жить, он проявляет малодушие. Он\nпоступает недостойно." +
                "Если же он не достиг цели и умер, это действительно фанатизм и\nсобачья смерть. Но в этом нет ничего" +
                "постыдного. Такая смерть есть Путь Самурая. Если \nкаждое утро и каждый вечер ты будешь готовить себя" +
                "к смерти и сможешь жить так,\nсловнотвое тело уже умерло, ты станешь Подлинным самураем. Тогда вся" +
                "твоя жизнь будет\nбезупречной, и ты преуспеешь на своем поприще.";
            Vector2 position = new Vector2(320, 190);
            _spriteBatch.DrawString(_miniTextBlock, text, position, Color.White);
        }

        public void DrawMenu(int selectedMenuItem, string[] menuItems)
        {
            _spriteBatch.Begin();

            // Заголовок меню
            Vector2 titlePosition = new Vector2(
                _graphicsDevice.Viewport.Width / 2 - _textBlock.MeasureString("Game").X / 2,
                200
            );
            _spriteBatch.DrawString(_textBlock, "Game", titlePosition, Color.White);

            // Пункты меню
            for (int i = 0; i < menuItems.Length; i++)
            {
                Color color = (i == selectedMenuItem) ? Color.Yellow : Color.White;
                Vector2 position = new Vector2(
                    _graphicsDevice.Viewport.Width / 2 - _textBlock.MeasureString(menuItems[i]).X / 2,
                    300 + i * 60
                );
                _spriteBatch.DrawString(_textBlock, menuItems[i], position, color);
            }

            _spriteBatch.End();
        }
        public void DrawGameUI(bool battleStarted, string bonusName, Color bonusColor, int level)
        {
            DrawGameAreaBorders();
            DrawUI(battleStarted, bonusName, bonusColor, level);
        }
    }
}