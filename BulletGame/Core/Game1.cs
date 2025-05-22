using System;
using System.Collections.Generic;
using System.Linq;
using BulletGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BulletGame
{
    public class Game1 : Game
    {
        private SpawnManager _spawnManager;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont textBlock;
        private GameRenderer _gameRenderer;

        private PlayerController player;
        private EnemyController enemy;
        private MouseState prevMouseState;
        private KeyboardState prevKeyboardState;
        private KeyboardState _prevKeyboardState;

        private SpriteFont miniTextBlock;
        private SpriteFont japanTextBlock;
        private SpriteFont japanSymbol;


        public Random rnd = new();

        private OptimizedBulletPool _bulletPool;
        private List<EnemyController> _enemies = new List<EnemyController>();
        private List<Bonus> _bonuses = new List<Bonus>();
        List<AttackPattern> attacksPatterns = new List<AttackPattern>();

        private int CountEnemyNow = 0;
        private int MaxCountEnemy = 1;
        private int CountBonusNow = 0;
        private int MaxCountBonus = 1;

        private enum GameState { Menu, Playing }
        private GameState _currentState = GameState.Menu;
        private int _selectedMenuItem = 0;
        private readonly string[] _menuItems = { "Новая игра", "Выход" };
        private SpriteFont _menuFont;
        private const float MenuItemSpacing = 60f;


        private Stack<object> _enemyWaveStack = new Stack<object>();

        private const float BonusLifetime = 12f;
        private const float BonusSpawnCooldown = 8f;
        private float _bonusSpawnTimer = 0f;
        private bool _canSpawnBonus = true;


        private int Lvl = 1;
        private string Name = "Пустота";
        private Color NameColor = Color.White;

        private float _hpTimer = 0;
        private float _spawnTimer;
        private const float SpawnInterval = 1f;

        private bool battleStarted = false;
        private float preBattleTimer = 0f;
        private const float PreBattleDelay = 0f;

        private Rectangle _gameArea;
        private Viewport _gameViewport;
        private Viewport _uiViewport;

        private UIManager _uiManager;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            spriteBatch = new SpriteBatch(GraphicsDevice);
            PrimitiveRenderer.Initialize(GraphicsDevice);
            _gameRenderer = new GameRenderer(spriteBatch, GraphicsDevice);

            // Загрузка шрифтов
            var textBlock = Content.Load<SpriteFont>("File");
            var miniTextBlock = Content.Load<SpriteFont>("FileMini");
            var japanTextBlock = Content.Load<SpriteFont>("Japan");
            var japanSymbol = Content.Load<SpriteFont>("JApanS");

            _uiManager = new UIManager(
               textBlock,
               japanTextBlock,
               miniTextBlock,
               japanSymbol,
               spriteBatch,
               GraphicsDevice,
               player,
               _enemies,
               _bonuses,
               _bulletPool,
               _gameArea
           );
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            graphics.HardwareModeSwitch = true;
            graphics.IsFullScreen = true;
            this.IsMouseVisible = false;
            graphics.ApplyChanges();

            _gameArea = new Rectangle(
            (graphics.PreferredBackBufferWidth - 1300) / 2,
            (graphics.PreferredBackBufferHeight - 750) / 2,
            1300,
            750
            );
            _gameViewport = GraphicsDevice.Viewport;
            _gameViewport.Bounds = _gameArea;

            _uiViewport = GraphicsDevice.Viewport;
            _uiViewport.Bounds = new Rectangle(0, 0, 1920, 1080);

           var player_model = new PlayerModel(new Vector2(640, 600), new AttackPattern(
           shootInterval: 0.2f,
           bulletSpeed: 900f,
           bulletsPerShot: 8,
           true,
           strategy: new ZRadiusBulletStrategy(GetDirectionAimPlayer, Color.White)));
            player = new PlayerController(player_model, new PlayerView(player_model));

            _bulletPool = new OptimizedBulletPool();

            attacksPatterns = new List<AttackPattern>
            {
                new AttackPattern(
                shootInterval: 0.1f,
                bulletSpeed: 500f,
                bulletsPerShot: 6,
                false,
                strategy: new SpiralStrategy(
                spiralSpeed: 2.2f,
                radiusStep: 2.0f,
                startColor: Color.Cyan,
                endColor: Color.Purple)),
                new AttackPattern(
                shootInterval: 0.1f,
                bulletSpeed: 500f,
                bulletsPerShot: 6,
                false,
                strategy: new A_StraightLineStrategy(player, Color.Cyan)),
                new AttackPattern(
                shootInterval: 0.5f,
                bulletSpeed: 300f,
                bulletsPerShot: 6,
                false,
                strategy: new RadiusBulletStrategy(player, Color.Cyan)),
                new AttackPattern(
                shootInterval: 0.1f,
                bulletSpeed: 300f,
                bulletsPerShot: 6,
                false,
                strategy: new AstroidStrategy(1.15f, Color.Cyan)),

            };

            _spawnManager = new SpawnManager(
                rnd,
                _gameArea,
                _enemies,
                 _bonuses,
                _enemyWaveStack,
                attacksPatterns,
                player
            );

            _spawnManager.InitializeWaveStack();
            base.Initialize();
            _uiManager._player = player;
        }

        public Vector2 GetDirectionAimPlayer()
        {
            Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

            Vector2 directionToAim = mousePosition - player.Model.Position;

            if (directionToAim != Vector2.Zero)
                directionToAim.Normalize();
            return directionToAim;
        }

        private void HandleMenuInput()
        {
            var keyboardState = Keyboard.GetState();

            // Обработка движения по меню
            if (keyboardState.IsKeyDown(Keys.Down) && !_prevKeyboardState.IsKeyDown(Keys.Down))
            {
                _selectedMenuItem = (_selectedMenuItem + 1) % _menuItems.Length;
            }
            else if (keyboardState.IsKeyDown(Keys.Up) && !_prevKeyboardState.IsKeyDown(Keys.Up))
            {
                _selectedMenuItem = (_selectedMenuItem - 1 + _menuItems.Length) % _menuItems.Length;
            }

            // Обработка выбора
            if (keyboardState.IsKeyDown(Keys.Enter) && !_prevKeyboardState.IsKeyDown(Keys.Enter))
            {
                switch (_selectedMenuItem)
                {
                    case 0: // Новая игра
                        _currentState = GameState.Playing;
                        // Здесь можно добавить сброс состояния игры
                        break;
                    case 1: // Выход
                        Exit();
                        break;
                }
            }

            _prevKeyboardState = keyboardState;
        }

        protected override void Update(GameTime gameTime)
        {
            if (_currentState == GameState.Menu)
            {
                HandleMenuInput();
            }
            else
            {
                base.Update(gameTime);
            }

            player.Model.AimPosition = GetClampedMousePosition();

            preBattleTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            _spawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!battleStarted)
            {
                preBattleTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (preBattleTimer >= PreBattleDelay)
                {
                    battleStarted = true;
                    _spawnTimer = 0f;
                }

                player.Update(gameTime);

                base.Update(gameTime);
                return;
            }

            foreach (var bonus in _bonuses.ToList())
            {
                bonus.Update(deltaTime);
                if (bonus.TimeLeft <= 0)
                {
                    _bonuses.Remove(bonus);
                    CountBonusNow--;
                    _bonusSpawnTimer = BonusSpawnCooldown; // Запускаем КД
                    _canSpawnBonus = false;
                }
            }

            // Обновление таймера спавна бонусов
            if (!_canSpawnBonus)
            {
                _bonusSpawnTimer -= deltaTime;
                if (_bonusSpawnTimer <= 0)
                {
                    _canSpawnBonus = true;
                }
            }

            // Логика спавна бонусов
            if (_canSpawnBonus && MaxCountBonus > CountBonusNow)
            {
                SpawnBonus();
                _canSpawnBonus = false;
                _bonusSpawnTimer = BonusSpawnCooldown;
            }

            if (_spawnTimer >= SpawnInterval && MaxCountBonus > CountBonusNow)
                SpawnBonus();

            /*if (_spawnTimer >= SpawnInterval && MaxCountEnemy > CountEnemyNow)
            {
                SpawnEnemy();
                _spawnTimer = 0f;
            }*/
            if (_enemies.Count == 0 && _enemyWaveStack.Count > 0)
            {
                ProcessNextWave();
            }


            foreach (var enemy in _enemies.ToList())
            {
                enemy.Update(gameTime, _bulletPool);
            }

            foreach (var bonus in _bonuses.ToList())
            {
                if (SATCollision.CheckCollision(player.Model.GetVertices(), bonus.GetVertices()))
                {
                    bonus.ApplyEffect(player.Model);
                    Name = bonus.Name;
                    NameColor = bonus.Color;
                    CountBonusNow--;
                    _bonuses.Remove(bonus);
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _bulletPool.Cleanup();

            UpdateBullets(gameTime);

            player.SetViewport(GraphicsDevice.Viewport);
            player.SetGeameArea(_gameArea);

            player.Update(gameTime);

            if (player.Model.Health <= 0)
            {
                Exit();
            }

            var mouseState = Mouse.GetState();
            float ShootTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                player.Model.ShootTimer += ShootTime;
                if (player.Model.ShootTimer >= 0.2f)
                {
                    Vector2 directionToAim = GetDirectionAimPlayer();
                    new AttackPattern(
                    shootInterval: 0.2f,
                    bulletSpeed: 900f,
                    bulletsPerShot: 1,
                    true,
                    strategy: new StraightLineStrategy(directionToAim, player.Model.Color)
                    ).Shoot(player.Model.Position, _bulletPool);
                    player.Model.ShootTimer = 0f;
                }
            }
            else
            {
                player.Model.ShootTimer = 0f;
            }
            prevMouseState = mouseState;


            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Space) && prevKeyboardState.IsKeyUp(Keys.Space))
            {
                Vector2 directionToAim = GetDirectionAimPlayer();

                player.Model.Health -= player.Model.BonusHealth;
                player.Model.AdditionalAttack.Shoot(player.Model.Position, _bulletPool);
            }

            prevKeyboardState = keyboardState;

            base.Update(gameTime);
        }

        private void ProcessNextWave()
        {
            if (_enemyWaveStack.Count == 0) return;
            var wave = _enemyWaveStack.Pop();
            ProcessWaveItems(wave);
        }

        private void ProcessWaveItems(object waveItem)
        {
            var stack = new Stack<object>();
            stack.Push(waveItem);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                if (current is Action action)
                {
                    action.Invoke(); // Вызываем действие для спавна врага
                }
                else if (current is IEnumerable<object> group)
                {
                    foreach (var item in group.Reverse())
                    {
                        stack.Push(item);
                    }
                }
            }
        }

        private void UpdateBullets(GameTime gameTime)
        {
            var viewport = GraphicsDevice.Viewport;
            var activeBullets = _bulletPool.ActiveBullets.ToList();

            var playerBullets = activeBullets.Where(b => b.Model.IsPlayerBullet).ToList();
            var enemyBullets = activeBullets.Where(b => !b.Model.IsPlayerBullet).ToList();

            foreach (var pBullet in playerBullets)
            {
                foreach (var eBullet in enemyBullets)
                {
                    if (pBullet.CollidesWithBullet(eBullet))
                    {
                        _bulletPool.Return(pBullet);
                        _bulletPool.Return(eBullet);
                        break;
                    }
                }
            }

            foreach (var bullet in activeBullets.Where(b => b.Model.IsPlayerBullet))
            {
                if (!bullet.Model.Active) continue;

                foreach (var enem_ in _enemies.ToList())
                {
                    if (bullet.CollidesWithEnemy(enem_))
                    {
                        enem_.Model.Health -= 1;
                        enem_.Model.TriggerHitAnimation();

                        _bulletPool.Return(bullet);

                        if (enem_.Model.Health <= 0)
                        {
                            _enemies.Remove(enem_);
                            CountEnemyNow--;
                        }
                    }
                }
            }

            foreach (var bullet in activeBullets)
            {
                if (!bullet.Model.Active) continue;

                bullet.Update(gameTime);

                if (!bullet.Model.IsPlayerBullet && bullet.CollidesWithPlayer(player))
                {
                    player.Model.Health--;
                    _bulletPool.Return(bullet);
                }

                if (bullet.IsExpired(_gameArea))
                    _bulletPool.Return(bullet);
            }
        }

        private Vector2 GetClampedMousePosition()
        {
            var mouseState = Mouse.GetState();

            int clampedX = (int)MathHelper.Clamp(
                mouseState.X,
                _gameArea.Left,
                _gameArea.Right
            );

            int clampedY = (int)MathHelper.Clamp(
                mouseState.Y,
                _gameArea.Top,
                _gameArea.Bottom
            );
                
            return new Vector2(clampedX, clampedY);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (_currentState == GameState.Menu)
            {
                _uiManager.DrawMenu(_selectedMenuItem, _menuItems);
            }
            else
            {
                _gameRenderer.Draw(player, _enemies, _bonuses, _bulletPool, _uiManager._japanSymbol);

                // Отрисовка UI
                _uiManager.DrawGameUI(battleStarted, Name, NameColor, Lvl);
            }

            base.Draw(gameTime);
        }

        private void SpawnBonus()
        {
            bool spawned = _spawnManager.SpawnBonus();
            if (spawned)
                CountBonusNow++;
        }

        private void SpawnEnemy()
        {
            bool spawned = _spawnManager.SpawnEnemy();
            if (spawned)
                CountEnemyNow++;
        }
    }
}
/*
    О том, хорош человек или плох, можно судить по испытаниям, которые выпадают на его долю. Удача и неудача определяются нашей судьбой. Хорошие и плохие дейст­вия – это Путь человека. Воздаяние за добро или зло – это всего лишь поучения проповедников.
*/
/*
 
   Беспринципно считать, что ты не можешь достичь всего, чего достигали великие мас­тера. Мастера – это люди, и ты – тоже человек. Если ты знаешь, что можешь стать таким же, как они, ты уже на пути к этому.
   Мастер Иттэй говорил: «Конфуций стал мудрецом потому, что стремился к учению с пятнадцатилетнего возраста, а не потому, что учился на старости лет». Это напоминает буддистское изречение: «Есть намерение, будет и прозрение».
*/


/*private Vector2 FindValidPosition(Vector2 preferredPosition, int maxAttempts,
                                       float minPlayerDist, float minEnemyDist, float minBonusDist)
       {
           int buffer = 100;
           if (IsPositionValid(preferredPosition, minPlayerDist, minEnemyDist, minBonusDist))
           {
               return preferredPosition;
           }

           for (int i = 0; i < maxAttempts; i++)
           {
               var pos = new Vector2(
                   rnd.Next(_gameArea.Left + buffer, _gameArea.Right - buffer),
                   rnd.Next(_gameArea.Top + buffer, _gameArea.Bottom - buffer)
               );

               if (IsPositionValid(pos, minPlayerDist, minEnemyDist, minBonusDist))
               {
                   return pos;
               }
           }

           return new Vector2(
               rnd.Next(_gameArea.Left + 100, _gameArea.Right - 100),
               rnd.Next(_gameArea.Top + 100, _gameArea.Bottom - 100)
           );
       }

       private bool IsPositionValid(Vector2 position, float minPlayerDist, float minEnemyDist, float minBonusDist)
       {
           if (Vector2.Distance(position, player.Model.Position) < minPlayerDist)
               return false;

           if (_enemies.Any(e => Vector2.Distance(position, e.Model.Position) < minEnemyDist))
               return false;

           if (_bonuses.Any(b => Vector2.Distance(position, b.Position) < minBonusDist))
               return false;

           return true;
       }S
       */


/*
 Сижу в этой прокуренной студии в Намба, пялюсь в монитор, а за спиной — шлем. Да, тот самый. С треснутым козырьком, из-под которого сочится кровь. Врачи говорят: «Панические атаки, стресс». Но они не видят, как по ночам тени в кимоно шепчут: «Тамэки…». Не слышат, как сердце колотится, словно хочет вырваться из грудной клетки и сбежать куда подальше.

А потом пришло письмо. От деда, о котором родители молчали всю жизнь. «Хидео Такахара оставил вам наследство». Папа, когда узнал, разбил чашку с чаем. Мама впервые закричала: «Не езди!». Но как не поехать? Там, в Сикоку, ответ. Или… тот самый «дом», куда зовёт голос.
Купил билет на автобус. Перед уходом заглянул в зеркало. В отражении за моей спиной стоял он — в шлеме, с окровавленным мечом. На стене медленно проступило: «Добро пожаловать домой…».

Я стою на пороге его квартиры. Дед. Хидео Такахара.
Дверь скрипит, будто её не открывали десятилетия. Внутри — запах плесени, старости и чего-то… металлического. Как кровь, засохшая в трещинах дерева.
Комната — коробка в три татами. Обои отслаиваются, пол прогнил, на потолке — жёлтые пятна от протечек. В углу — раскладушка с провалившимся матрасом. Рядом пепельница, переполненная окурками, и пустые банки саке. 
Но посреди этого хаоса — они.
У стены, на самодельной подставке из ящиков, стоят доспехи. Не музейные — настоящие. Ржавые пластины, перетянутые потёртым шёлком. Шлем с трещиной, как в моих видениях. А рядом — катана. Лезвие в ножнах, но рукоять… Она идеальна. Резная кость, обмотанная чёрной кожей. Будто её только вчера вытерли рисовой бумагой.

Пока я решал юридические вопросы, мне пришлось остаться в городе. Проклятые головные боли не дают мне покоя, мучают меня каждый день. Иногда, в темноте я вижу искорёженные силуэты, они улыбаются и смеются. Я это не вынесу!

Город медленно превращается в лабиринт.
Сегодня, возвращаясь от юристов, забрёл в переулок, которого раньше не видел. Старые телефонные будки, облезлые плакаты 90-х, лужи с радужной плёнкой. А в конце — лавка. Витрина забита бутылками с мутной жидкостью и высушенными насекомыми.

Внутри пахло ладаном и полынью. За прилавком — девушка, лет двадцати, в кимоно цвета грозовой тучи. Волосы — белые, будто её коснулся мороз из старых сказок. На шее — ожерелье из когтей. Глаза смотрели сквозь меня.

— Ты принёс его с собой, да? — её голос звучал как шелест бумажных ширм.
Я достал меч деда. Она кивнула, словно ждала этого.

Рассказал ей всё: о шлеме, о тенях, о голосах. О том, как боль превращает мысли в кашу. Она слушала, не перебивая, а потом провела пальцем по лезвию. 
Она зажгла чёрные свечи с запахом гвоздики, заставила меня сесть на циновку с вышитыми демонами. Потом поднесла к моим губам чашу с дымящимся чаем. Горький, как пепел. В глазах потемнело.
*/