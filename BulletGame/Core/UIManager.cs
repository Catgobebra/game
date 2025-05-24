using BulletGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace BulletGame
{
    public class UIManager
    {
        private readonly SpriteFont _textBlock;
        private readonly SpriteFont _japanTextBlock;
        private readonly SpriteFont _miniTextBlock;
        private readonly SpriteFont _miniS_TextBlock;
        public readonly SpriteFont _japanSymbol;
        private readonly SpriteBatch _spriteBatch;
        private readonly GraphicsDevice _graphicsDevice;

        private int _currentFrame;
        private float _frameTimer;
        private const float FrameDuration = 10f;
        private readonly string[] _level1Texts = new string[4];
        private readonly Texture2D[] _level1Textures;
        private bool _skipRequested;

        public PlayerController _player;
        private readonly List<EnemyController> _enemies;
        private readonly List<Bonus> _bonuses;
        private readonly OptimizedBulletPool _bulletPool;
        private readonly Rectangle _gameArea;

        public UIManager(
            SpriteFont textBlock,
            SpriteFont japanTextBlock,
            SpriteFont miniTextBlock,
            SpriteFont miniS_TextBlock,
            SpriteFont japanSymbol,
            SpriteBatch spriteBatch,
            GraphicsDevice graphicsDevice,
            PlayerController player,
            List<EnemyController> enemies,
            List<Bonus> bonuses,
            OptimizedBulletPool bulletPool,
            Rectangle gameArea,
            Texture2D[] level1Textures)
        {
            _textBlock = textBlock;
            _japanTextBlock = japanTextBlock;
            _miniTextBlock = miniTextBlock;
            _miniS_TextBlock = miniS_TextBlock;
            _japanSymbol = japanSymbol;
            _spriteBatch = spriteBatch;
            _graphicsDevice = graphicsDevice;
            _player = player;
            _enemies = enemies;
            _bonuses = bonuses;
            _bulletPool = bulletPool;
            _gameArea = gameArea;
            _level1Textures = level1Textures;
            SetLvlText();
        }

        private void SetLvlText()
        {
            _level1Texts[0] =  "Сижу в этой прокуренной студии в Намба, пялюсь в монитор, а за спиной  шлем. Да,тот самый." +
            " С\nтреснутым козырьком, из-под которого сочится кровь. Врачи говорят: Панические атаки, стресс. " +
            "Но они\nне видят, как по ночам тени в кимоно шепчут: Тамэк... Не слышат, как сердце колотится, " +
            "словно хочет\nвырваться из грудной клетки и сбежать куда подальше." +
            "А потом пришло письмо. От деда, о котором\nродители молчали всю жизнь." +
            "Хидео Такахара оставил вам наследство. Папа, когда узнал, разбил чашку\nс чаем." +
            "Мама впервые закричала: Не езди! Но как не поехать? Там, в Сикоку, ответ. " +
            "Или... тот самый\nдом, куда зовёт голос." +
            "Купил билет на автобус. Перед уходом заглянул в зеркало." +
            "В отражении за моей\nспиной стоял он в шлеме, с окровавленным мечом. " +
            "На стене медленно проступило: Добро пожаловать\nдомой...";

            _level1Texts[1] =  "Я стою на пороге его квартиры. Дед. Хидео Такахара." +
            " Дверь скрипит, будто её не открывали\nдесятилетия. Внутри, запах плесени, старости и чего-то металлического. " +
            "Как кровь, засохшая в\nтрещинах дерева." +
            "Комната, коробка в три татами. Обои отслаиваются, пол прогнил, на потолке, жёлтые\nпятна от протечек. " +
            "В углу, раскладушка с провалившимся матрасом. Рядом пепельница, переполненная\nокурками, и пустые банки саке." +
            "Но посреди этого хаоса, они. У стены, на самодельной подставке из\nящиков, стоят доспехи. Не музейные, настоящие. " +
            "Ржавые пластины, перетянутые потёртым шёлком.\nШлем с трещиной, как в моих видениях. А рядом, катана. " +
            "Лезвие в ножнах, но рукоять. Она идеальна.\nРезная кость, обмотанная чёрной кожей. " +
            "Будто её только вчера вытерли рисовой бумагой.";

            _level1Texts[2] = "Пока я решал юридические вопросы, мне пришлось остаться в городе. " +
            "Проклятые головные боли не дают\nмне покоя, мучают меня каждый день. Иногда, " +
            "в темноте я вижу искорёженные силуэты, они\nулыбаются и смеются. Я это не вынесу!" +
            "Город медленно превращается в лабиринт." +
            "Сегодня, возвращаясь\nот юристов, забрёл в переулок, которого раньше не видел. " +
            "Старые телефонные будки, облезлые плакаты\n90-х, лужи с радужной плёнкой. " +
            "А в конце, лавка. Витрина забита бутылками с мутной жидкостью\nи высушенными насекомыми.";

            _level1Texts[3] = "Внутри пахло ладаном и полынью. За прилавком, девушка, лет двадцати, в кимоно цвета грозовой тучи.\n" +
            "Волосы, белые, будто её коснулся мороз из старых сказок. На шее, ожерелье из когтей. " +
            "Глаза\nсмотрели сквозь меня." +
            "Ты принёс его с собой, да? её голос звучал как шелест бумажных ширм." +
            " Я\nдостал меч деда. Она кивнула, словно ждала этого." +
            "Рассказал ей всё: о шлеме, о тенях, о голосах. О\nтом, как боль превращает мысли в кашу. " +
            "Она слушала, не перебивая, а потом провела пальцем по\nлезвию." +
            " Она зажгла чёрные свечи с запахом гвоздики, заставила меня сесть на циновку с\nвышитыми демонами. " +
            "Потом поднесла к моим губам чашу с дымящимся чаем. Горький, как пепел.\nВ глазах потемнело...";
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

            if (!battleStarted)
            {
                DrawPreBattleText(lvl);
            }
            else
            {
                _spriteBatch.DrawString(_textBlock, $"{_player.Model.Health} ед. Ки",
                new Vector2(50, 50), Color.White);

                _spriteBatch.DrawString(_textBlock, name,
                    new Vector2(480, 50), nameColor);

                _spriteBatch.DrawString(_textBlock, $"{lvl} Ступень",
                    new Vector2(880, 50), Color.White);
            }
        
            _spriteBatch.DrawString(_japanTextBlock, "せ\nん\nし",
                new Vector2(1750, 400), Color.White);
            _spriteBatch.DrawString(_japanTextBlock, $"だいみょう", new Vector2(800, 940), Color.White);
            _spriteBatch.DrawString(_japanTextBlock, $"ぶ\nし", new Vector2(100, 400), Color.White);

            _spriteBatch.End();
        }

        /*private void DrawPreBattleText()
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
        }*/

        public void DrawMenu(int selectedMenuItem, string[] menuItems)
        {
            _spriteBatch.Begin();

            Vector2 titlePosition = new Vector2(
                _graphicsDevice.Viewport.Width / 2 - _textBlock.MeasureString("Game").X / 2,
                200
            );
            _spriteBatch.DrawString(_textBlock, "Game", titlePosition, Color.White);

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

        public void UpdatePreBattle(GameTime gameTime, int level, bool isSkipRequested)
        {
            if (level != 1) return;

            _skipRequested = isSkipRequested;

            if (_currentFrame >= 4) return;

            if (_skipRequested)
            {
                _currentFrame = 4; // Пропускаем все кадры
                return;
            }

            _frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_frameTimer >= FrameDuration)
            {
                _currentFrame++;
                _frameTimer = 0f;
            }
        }

        public bool IsIntroComplete() => _currentFrame >= 4;

        private void DrawPreBattleText(int level)
        {
            if (level == 1 && _currentFrame < 4)
            {
                DrawLevel1Intro();
            }
            else
            {
                DrawDefaultIntro();
            }

        }

        private void DrawDefaultIntro()
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

            _spriteBatch.DrawString(_miniTextBlock, text, new Vector2(320, 190), Color.White);
        }

        private void DrawLevel1Intro()
        {

            if (_currentFrame >= _level1Texts.Length) return;

            float alpha = MathHelper.Clamp(_frameTimer / FrameDuration * 10, 0f, 1f);

            string text = _level1Texts[_currentFrame];
            Texture2D texture = _level1Textures[_currentFrame];
            Rectangle destinationRect = new Rectangle(650, _gameArea.Y + 12, 599, 447);

            _spriteBatch.DrawString(_miniS_TextBlock, text, new Vector2(320, 625), Color.White * alpha);
            _spriteBatch.Draw(texture, destinationRect, Color.White * alpha);
        }

        public void ResetLevel1Intro()
        {
            _currentFrame = 0;
            _frameTimer = 0f;
            _skipRequested = false;
        }
    }

}
