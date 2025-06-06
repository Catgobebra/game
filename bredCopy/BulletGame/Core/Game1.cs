using System;
using System.Collections.Generic;
using System.Linq;
using BulletGame.Controllers;
using BulletGame.Core;
using BulletGame.Models;
using BulletGame.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static BulletGame.Game1;

namespace BulletGame
{
    public interface IGameController
    {
        void ResetGameState(int currentLevel = 1);
        GameState CurrentState { get; set; }
        int CurrentLevel { get; set; }
        InputHandler InputHandler { get; }
        bool IsSkipRequested { get; }
        void RequestSkipWave();
        void TogglePause();
    }

    public class Game1 : Game, IGameController
    {
        private SpawnManager _spawnManager;

        private WaveProcessor _waveProcessor;

        private LevelData _currentLevelData;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont textBlock;
        private GameRenderer _gameRenderer;

        private IPlayer player;
        private EnemyController enemy;
        private MouseState prevMouseState;
        private KeyboardState prevKeyboardState;
        private KeyboardState _prevKeyboardState;
        private MenuInputHandler _menuInputHandler;

        private SpriteFont miniTextBlock;
        private SpriteFont japanTextBlock;
        private SpriteFont japanSymbol;
        private SpriteFont miniS_TextBlock;

        private BulletManager _bulletManager;


        public Random rnd = new();

        private IBulletPool _bulletPool;
        private List<IEnemy> _enemies = new List<IEnemy>();
        private List<BonusController> _bonuses = new List<BonusController>();

        private int CountEnemyNow = 0;
        private int CountBonusNow = 0;
        private int MaxCountBonus = 1;

        private PrimitiveRenderer _primitiveRenderer;

        private float _enemySpawnTimer = 0f;
        private const float EnemySpawnInterval = 2f;

        private LevelLoader _levelLoader;


        private bool _isWaveInProgress = false;

        public InputHandler _inputHandler;

        public enum GameState { Menu, Playing, Pause, Victory, GameCompleted}

        public GameState CurrentState { get; set; } = GameState.Menu;

        public int _selectedMenuItem = 0;
        public string[] MenuItems => CurrentState == GameState.Pause
        ? new[] { "Продолжить", "Начать заново", "Выход в меню" }
        : new[] { "Новая игра", "Продолжить", "Выход" };

        private SpriteFont _menuFont;
        private const float MenuItemSpacing = 60f;

        private Stack<IWaveItem> _enemyWaveStack = new Stack<IWaveItem>();

        private const float BonusLifetime = 12f;
        private const float BonusSpawnCooldown = 8f;
        private float _bonusSpawnTimer = 0f;
        private bool _canSpawnBonus = true;

        private Texture2D[] _level1Textures;

        public int CurrentLevel { get; set; } = 1;
        private string Name = "Пустота";
        private Color NameColor = Color.White;

        private float _spawnTimer;
        private const float SpawnInterval = 1f;

        private bool battleStarted = false;
        private float preBattleTimer = 0f;
        private const float PreBattleDelay = 100f;

        private Rectangle _gameArea;
        private Viewport _gameViewport;
        private Viewport _uiViewport;

        private BonusController defaultBonus;

        private UIManager _uiManager;

        public InputHandler InputHandler => _inputHandler;
        public bool IsSkipRequested => _inputHandler.IsSkipRequested;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //PrimitiveRenderer.Initialize(GraphicsDevice);

            _gameRenderer = new GameRenderer(spriteBatch, GraphicsDevice);

            _level1Textures = new Texture2D[] {
                Content.Load<Texture2D>("ascii-art (2)"),
                Content.Load<Texture2D>("ascii-art"),
                Content.Load<Texture2D>("ascii-art (4)"),
                Content.Load<Texture2D>("ascii-art (3)")
            };

 
            textBlock = Content.Load<SpriteFont>("File");
            miniTextBlock = Content.Load<SpriteFont>("FileMini");
            miniS_TextBlock = Content.Load<SpriteFont>("FileMiniS");
            japanTextBlock = Content.Load<SpriteFont>("Japan");
            japanSymbol = Content.Load<SpriteFont>("JApanS");

            _bulletManager = new BulletManager(
                _bulletPool,
                player,
                _enemies, 
                _gameArea
            );

            _uiManager = new UIManager(
                textBlock,
                japanTextBlock,
                miniTextBlock,
                miniS_TextBlock,
                japanSymbol,
                spriteBatch,
                GraphicsDevice,
                player,
                _enemies, 
                _bonuses,
                _bulletPool, 
                _gameArea,
                _level1Textures,
                _primitiveRenderer
            );


            // Применение параметров уровня
            MaxCountBonus = _currentLevelData.MaxBonusCount;
            Name = _currentLevelData.LevelName;
            NameColor = _currentLevelData.LevelNameColor;

            // Инициализация волн
            _spawnManager.InitializeWaveStack(_currentLevelData);
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            graphics.HardwareModeSwitch = true;
            graphics.IsFullScreen = false;
            this.IsMouseVisible = false;
            graphics.ApplyChanges();
            _levelLoader = new LevelLoader(Content.RootDirectory);
            _waveProcessor = new WaveProcessor(_enemyWaveStack);

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

            _currentLevelData = _levelLoader.LoadLevel(CurrentLevel);

           var player_model = new PlayerModel(new Vector2(640, 600), new AttackPattern(
           shootInterval: 0.2f,
           bulletSpeed: 900f,
           bulletsPerShot: 8,
           true,
           strategy: new ZRadiusBulletStrategy(() => _inputHandler.GetDirectionAimPlayer(), Color.White)));
           player = new PlayerController(player_model, new PlayerView(player_model,_primitiveRenderer));

            _bulletPool = new OptimizedBulletPool(new BulletFactory());

            _spawnManager = new SpawnManager(
                rnd,
                _gameArea,
                _enemies,
                 _bonuses,
                _enemyWaveStack,
                player,
                _primitiveRenderer
            );

            _inputHandler = new InputHandler(
                player,
                this,
                _gameArea
            );

            _spawnManager.InitializeWaveStack(_currentLevelData);

            _menuInputHandler = new MenuInputHandler(this);

            var bonusModel = new BonusModel(
                new AttackPattern(
               0.2f, 900f, 12, true,
               new ZRadiusBulletStrategy(_inputHandler.GetDirectionAimPlayer, Color.White)),
                Vector2.Zero,
                "空",
                "Пустота",
                Color.White,
                1
            );

            defaultBonus = new BonusController(bonusModel, new BonusView(bonusModel));

            base.Initialize();
            _uiManager._player = player;
        }

        public void RequestSkipWave()
        {
            //IsSkipRequested = true;
            //_skipCooldownTimer = SkipCooldown;

        }

        public void TogglePause()
        {
            CurrentState = (CurrentState == GameState.Playing)
                ? GameState.Pause : GameState.Playing;
        }

        public void ResetGameState(int CurrentLevel = 1)
        {
            _enemies.Clear();
            _bonuses.Clear();

            _bulletPool.ForceCleanup();
            _bulletPool.Cleanup();

            CurrentLevel = CurrentLevel;

            _currentLevelData = _levelLoader.LoadLevel(CurrentLevel);

            battleStarted = false;
            preBattleTimer = 0f;
            _spawnTimer = 0f;
            _isWaveInProgress = false;

            if (player.GameArea.Width == 0 || player.GameArea.Height == 0)
            {
                player.GameArea = _gameArea;
            }
            player.Model.UpdatePosition(_currentLevelData.PlayerStart.Position);

            MaxCountBonus = _currentLevelData.MaxBonusCount;
            Name = _currentLevelData.LevelName;
            NameColor = _currentLevelData.LevelNameColor;

            player.Health = _currentLevelData.PlayerStart.Health;

            _spawnManager.InitializeWaveStack(_currentLevelData);

            defaultBonus.ApplyEffect(player);
            Name = defaultBonus._model.Name;
            NameColor = defaultBonus._model.Color;
        }

        protected override void Update(GameTime gameTime)
        {
            if (CurrentState == GameState.Playing && !battleStarted)
            {
                _uiManager.UpdatePreBattle(gameTime, CurrentLevel, _inputHandler.IsSkipRequested);
            }

            if (CurrentState == GameState.Menu)
            {
                _menuInputHandler.Update();
            }
            else if (CurrentState == GameState.Pause)
            {
                _menuInputHandler.Update();
                base.Update(gameTime);
            }
            else if (CurrentState == GameState.Victory)
            {
                if (CurrentLevel < 2)
                {
                    CurrentLevel++;
                    ResetGameState(CurrentLevel);
                    CurrentState = GameState.Playing;
                    _menuInputHandler.Update();
                }
                else
                {
                    CurrentState = GameState.GameCompleted;
                }
            }
            else if (CurrentState == GameState.GameCompleted)
            {
                CurrentLevel = 1;
                CurrentState = GameState.Menu;
            }
            else
            {
                _inputHandler.Update();

                if (!battleStarted && _inputHandler.IsSkipRequested)
                {
                    battleStarted = true;
                    preBattleTimer = PreBattleDelay;
                    _spawnTimer = 0f;
                    _inputHandler.IsSkipRequested = false;
                }
                Zaglusha(gameTime);
                base.Update(gameTime);
            }

            //base.Update(gameTime);
        }

        public class BulletFactory : IBulletFactory
        {
            public IBulletController CreateBullet()
            {
                // Пример создания пули с моделью и контроллером
                var model = new BulletModel();
                return new BulletController(model);
            }
        }

            void Zaglusha(GameTime gameTime)
        {
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


            if (battleStarted && (!_isWaveInProgress)) _enemySpawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (var bonus in _bonuses.ToList())
            {
                bonus.Update(deltaTime);
                if (bonus._model.TimeLeft <= 0)
                {
                    _bonuses.Remove(bonus);
                    CountBonusNow--;
                    _bonusSpawnTimer = BonusSpawnCooldown;
                    _canSpawnBonus = false;
                }
            }

            if (!_canSpawnBonus)
            {
                _bonusSpawnTimer -= deltaTime;
                if (_bonusSpawnTimer <= 0)
                {
                    _canSpawnBonus = true;
                }
            }

            if (_canSpawnBonus && MaxCountBonus > CountBonusNow)
            {
                SpawnBonus();
                _canSpawnBonus = false;
                _bonusSpawnTimer = BonusSpawnCooldown;
            }

            if (_enemies.Count == 0 && _enemyWaveStack.Count > 0 && (_isWaveInProgress))
            {
                _isWaveInProgress = false;
            }

            if (_enemies.Count == 0 && _enemyWaveStack.Count > 0
               && _enemySpawnTimer >= EnemySpawnInterval && (!_isWaveInProgress))
            {
                _isWaveInProgress = true;

                var nextWave = _enemyWaveStack.Pop();

                if (nextWave is WaveData waveData)
                {
                    _spawnManager.ProcessWaveData(waveData);
                }
                else if (nextWave is List<Action> actionWave)
                {
                    _waveProcessor.ProcessWaveItems(new ActionListWaveItem(actionWave));
                }

                _enemySpawnTimer = 0f;
            }

            if (battleStarted &&
                _enemies.Count == 0 &&
                _enemyWaveStack.Count == 0)
            {
                CurrentState = GameState.Victory;
                battleStarted = false;
            }

            foreach (var enemy in _enemies.ToList())
            {
                enemy.Update(gameTime, _bulletPool);
            }

            /*foreach (var bonus in _bonuses.ToList())
            {
                if (CollisionHelper.CheckCollision(player.Collider, bonus._view.GetVertices()))
                {
                    bonus.ApplyEffect(player.Model);
                    Name = bonus._model.Name;
                    NameColor = bonus._model.Color;
                    CountBonusNow--;
                    _bonuses.Remove(bonus);
                }
            }*/

            _bulletPool.Cleanup();

            _bulletManager.Update(gameTime);

            player.SetViewport(GraphicsDevice.Viewport);
            player.SetGameArea(_gameArea);

            player.Update(gameTime);

            if (player.Health <= 0)
            {
                ResetGameState();
                CurrentState = GameState.Menu;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (CurrentState == GameState.Menu)
            {
                _uiManager.DrawMenu(_selectedMenuItem, MenuItems, gameTime);
            }
            else if (CurrentState == GameState.Pause)
            {
                _uiManager.DrawMenu(_selectedMenuItem, MenuItems, gameTime);
            }
            else
            {
                _uiManager.DrawGameUI(battleStarted, Name, NameColor, CurrentLevel);

                if (battleStarted)
                {
                    var drawables = new List<IDrawable>();
                    drawables.Add((IDrawable)player);                  // Игрок
                    drawables.AddRange((IEnumerable<IDrawable>)_enemies);           // Все враги
                    drawables.AddRange((IEnumerable<IDrawable>)_bonuses);           // Все бонусы
                    drawables.AddRange((IEnumerable<IDrawable>)_bulletPool);        // Все пули
                    //drawables.Add(_uiManager._japanSymbol); // Символ

                    // Передаем объединенную коллекцию
                    _gameRenderer.Draw(drawables);
                }
            }

            base.Draw(gameTime);
        }

        public void ResetAnimation()
        {
            _uiManager.ResetMenuAnimation();
        }

        private bool SpawnBonus()
        {
            bool spawned = _spawnManager.SpawnBonus();
            if (spawned)
            {
                CountBonusNow++;
                return true;
            }
            return false;
        }

        public class ActionListWaveItem : IWaveItem
        {
            private readonly List<Action> _actions;

            public ActionListWaveItem(List<Action> actions)
            {
                _actions = actions;
            }

            public void Process()
            {
                foreach (var action in _actions)
                {
                    action?.Invoke();
                }
            }
        }
    }
}