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

        private WaveProcessor _waveProcessor;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont textBlock;
        private GameRenderer _gameRenderer;

        private PlayerController player;
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

        private OptimizedBulletPool _bulletPool;
        private List<EnemyController> _enemies = new List<EnemyController>();
        private List<Bonus> _bonuses = new List<Bonus>();

        private int CountEnemyNow = 0;
        private int MaxCountEnemy = 1;
        private int CountBonusNow = 0;
        private int MaxCountBonus = 1;

        private float _enemySpawnTimer = 0f;
        private const float EnemySpawnInterval = 2f;

        private bool _isWaveInProgress = false;

        public InputHandler _inputHandler;

        public enum GameState { Menu, Playing, Pause }

        public GameState _currentState { get; set; } = GameState.Menu;

        public int _selectedMenuItem = 0;
        public readonly string[] _menuItems = { "Новая игра", "Выход" };
        private SpriteFont _menuFont;
        private const float MenuItemSpacing = 60f;


        private Stack<object> _enemyWaveStack = new Stack<object>();

        private const float BonusLifetime = 12f;
        private const float BonusSpawnCooldown = 8f;
        private float _bonusSpawnTimer = 0f;
        private bool _canSpawnBonus = true;

        private Texture2D[] _level1Textures;

        private int Lvl = 1;
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

        private Bonus defaultBonus;

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
               _level1Textures
           );
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            graphics.HardwareModeSwitch = true;
            graphics.IsFullScreen = false;
            this.IsMouseVisible = false;
            graphics.ApplyChanges();

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

           var player_model = new PlayerModel(new Vector2(640, 600), new AttackPattern(
           shootInterval: 0.2f,
           bulletSpeed: 900f,
           bulletsPerShot: 8,
           true,
           strategy: new ZRadiusBulletStrategy(() => _inputHandler.GetDirectionAimPlayer(), Color.White)));
            player = new PlayerController(player_model, new PlayerView(player_model));

            _bulletPool = new OptimizedBulletPool();

            _spawnManager = new SpawnManager(
                rnd,
                _gameArea,
                _enemies,
                 _bonuses,
                _enemyWaveStack,
                player
            );

            _inputHandler = new InputHandler(
                player,
                this,
                _bulletPool,
                _gameArea
            );

            _spawnManager.InitializeWaveStack();
            _menuInputHandler = new MenuInputHandler(this);

            defaultBonus = new Bonus(
               new AttackPattern(
               0.2f, 900f, 12, true,
               new ZRadiusBulletStrategy(_inputHandler.GetDirectionAimPlayer, Color.White)),
                   Vector2.Zero,
                   "空",
                   "Пустота",
                   Color.White,
                   1
            );

            base.Initialize();
            _uiManager._player = player;
        }

        public void ResetGameState()
        {
            player.Model.UpdatePosition(new Vector2(640,600));

            _enemies.Clear();
            _bonuses.Clear();

            _bulletPool.ForceCleanup();
            _bulletPool.Cleanup();

            _spawnTimer = 0f;
            preBattleTimer = 0f;
            battleStarted = false;
            _inputHandler.IsSkipRequested = false;
            _spawnManager.InitializeWaveStack();
            _uiManager.ResetLevel1Intro();
            _isWaveInProgress = false;

            CountEnemyNow = 0;
            CountBonusNow = 0;
            _canSpawnBonus = true;
            _bonusSpawnTimer = 0f;
            _enemySpawnTimer = 0f;

            defaultBonus.ApplyEffect(player.Model);
            Name = defaultBonus.Name;
            NameColor = defaultBonus.Color;
            player.Model.Health = 8;
        }

        protected override void Update(GameTime gameTime)
        {
            if (_currentState == GameState.Playing && !battleStarted)
            {
                _uiManager.UpdatePreBattle(gameTime, Lvl, _inputHandler.IsSkipRequested);
            }

            if (_currentState == GameState.Menu)
            {
                _menuInputHandler.Update();
            }
            else
            {
                _inputHandler.Update(gameTime);

                if (!battleStarted && _inputHandler.IsSkipRequested)
                {
                    battleStarted = true;
                    preBattleTimer = PreBattleDelay;
                    _spawnTimer = 0f;
                    _inputHandler.IsSkipRequested = false;
                }

                base.Update(gameTime);
            }

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
                if (bonus.TimeLeft <= 0)
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

            if (_spawnTimer >= SpawnInterval && MaxCountBonus > CountBonusNow)
                SpawnBonus();

            if (_enemies.Count == 0 && _enemyWaveStack.Count > 0 && (_isWaveInProgress))
            {
                _isWaveInProgress = false;
            }

            if (_enemies.Count == 0 && _enemyWaveStack.Count > 0 
                && _enemySpawnTimer >= EnemySpawnInterval && (!_isWaveInProgress))
            {
                _isWaveInProgress = true;
                _waveProcessor.ProcessNextWave();
                _enemySpawnTimer = 0f;
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

            _bulletPool.Cleanup();

            _bulletManager.Update(gameTime);

            player.SetViewport(GraphicsDevice.Viewport);
            player.SetGeameArea(_gameArea);

            player.Update(gameTime);

            if (player.Model.Health <= 0)
            {
                ResetGameState();
                _currentState = GameState.Menu;
            }

            base.Update(gameTime);
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
                _uiManager.DrawGameUI(battleStarted, Name, NameColor, Lvl);

                if (battleStarted)
                {
                    _gameRenderer.Draw(player, _enemies, _bonuses, _bulletPool, _uiManager._japanSymbol);
                }
            }

            base.Draw(gameTime);
        }

        private float GetIntroAlpha()
        {
            float fadeStart = PreBattleDelay - 2f;
            float fadeTime = MathHelper.Clamp(preBattleTimer - fadeStart, 0f, 2f);
            return MathHelper.Lerp(1f, 0f, fadeTime / 2f);
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