using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Xml.Linq;
using BulletGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace BulletGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;  
        private PlayerController player;
        private EnemyController enemy;
        private MouseState prevMouseState;
        private KeyboardState prevKeyboardState;

        public Random rnd = new();

        private OptimizedBulletPool _bulletPool;
        private List<EnemyController> _enemies = new List<EnemyController>();
        private List<Bonus> _bonuses = new List<Bonus>();
        List<AttackPattern> attacksPatterns = new List<AttackPattern>();

        private int CountEnemyNow = 0;
        private int MaxCountEnemy = 1;
        private int CountBonusNow = 0;
        private int MaxCountBonus = 1;

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


        SpriteFont textBlock;
        SpriteFont japanTextBlock;
        SpriteFont miniTextBlock;
        SpriteFont JapanSymbol;
        SpriteBatch spriteBatch;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            textBlock = Content.Load<SpriteFont>("File");
            miniTextBlock = Content.Load<SpriteFont>("FileMini");
            japanTextBlock = Content.Load<SpriteFont>("Japan");
            JapanSymbol = Content.Load<SpriteFont>("JApanS");
            PrimitiveRenderer.Initialize(GraphicsDevice);
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            graphics.HardwareModeSwitch = true;
            graphics.IsFullScreen = false;
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


            _bulletPool = new OptimizedBulletPool();

            var player_model = new PlayerModel(new Vector2(640, 600), new AttackPattern(
            shootInterval: 0.2f,
            bulletSpeed: 900f,
            bulletsPerShot: 8,
            true,
            strategy: new ZRadiusBulletStrategy(GetDirectionAimPlayer, Color.White)));
            player = new PlayerController(player_model, new PlayerView(player_model));

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
                shootInterval: 0.1f,
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
           
            base.Initialize();
        }

        public Vector2 GetDirectionAimPlayer()
        {
            Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

            Vector2 directionToAim = mousePosition - player.Model.Position;

            if (directionToAim != Vector2.Zero)
                directionToAim.Normalize();
            return directionToAim;
        }

        protected override void Update(GameTime gameTime)
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

            foreach (var bonus in _bonuses.ToList())
            {
                bonus.Update(deltaTime);
                if (bonus.timeLeft <= 0)
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

            if (_spawnTimer >= SpawnInterval && MaxCountEnemy > CountEnemyNow)
            {
                SpawnEnemy();
                _spawnTimer = 0f;
            }

            foreach (var enemy in _enemies.ToList())
            {
                enemy.Update(gameTime, _bulletPool);

                /*if (SATCollision.CheckCollision(player.Model.GetVertices(), enemy.Model.GetVertices()))
                {
                    player.Model.Health += 20;
                }*/
            }

            foreach (var bonus in _bonuses.ToList())
            {
                if (SATCollision.CheckCollision(player.Model.GetVertices(), bonus.GetVertices()))
                {
                    bonus.ApplyEffect(player.Model);
                    Name = bonus.name;
                    NameColor = bonus.color;
                    CountBonusNow--;
                    _bonuses.Remove(bonus);
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _bulletPool.Cleanup();

            //enemy.Update(gameTime, _bulletPool);
            UpdateBullets(gameTime);

            player.SetViewport(GraphicsDevice.Viewport);
            player.SetGeameArea(_gameArea);

            player.Update(gameTime);

            if (player.Model.Health <= 0)
            {
                Exit();
            }

            var mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed &&
               prevMouseState.LeftButton == ButtonState.Released)
            {

                Vector2 directionToAim = GetDirectionAimPlayer();

                new AttackPattern(
                    shootInterval: 0.2f,
                    bulletSpeed: 900f,
                    bulletsPerShot: 1,
                    true,
                    strategy: new StraightLineStrategy(directionToAim, Color.White)
                ).Shoot(player.Model.Position, _bulletPool);
            }
            prevMouseState = mouseState;


            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Space) && prevKeyboardState.IsKeyUp(Keys.Space))
            {
                Vector2 directionToAim = GetDirectionAimPlayer();

                player.Model.Health -= 1;
                /*new CrystalFanStrategy(Color.Blue, directionToAim)*//*new StarPatternStrategy(Color.Blue)*/ //new PlayerExplosiveShotStrategy(Color.Blue,Color.Bisque)
                //if (player.Model.AdditionalAttack.str )
                player.Model.AdditionalAttack.Shoot(player.Model.Position, _bulletPool);
            }

            prevKeyboardState = keyboardState;

            /*timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timer > 0.1f)
            {
                visible = !visible;
                timer = 0;
            }*/

            base.Update(gameTime);
        }

        private void SpawnEnemy()
        {
            int buffer = 100;
            const int maxAttempts = 50; 
            const float minPlayerDistance = 300f; 
            const float minEnemyDistance = 150f;
            const float minBonusDistance = 100f;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector2 position = new Vector2(
                    rnd.Next(_gameArea.Left + buffer, _gameArea.Right - buffer),
                    rnd.Next(_gameArea.Top + buffer, _gameArea.Bottom - buffer)
                );

                if (Vector2.Distance(position, player.Model.Position) < minPlayerDistance)
                    continue;

                bool tooClose = _enemies.Any(e =>
                    Vector2.Distance(position, e.Model.Position) < minEnemyDistance)
                    && _bonuses.Any(e =>
                    Vector2.Distance(position, e.position) < minBonusDistance);

                if (!tooClose)
                {
                    var enemyModel = new EnemyModel(
                    position: position,
                    attacksPatterns[rnd.Next(0, attacksPatterns.Count)],
                    Color.Crimson
                    );
                    _enemies.Add(new EnemyController(enemyModel, new EnemyView(enemyModel)));
                    CountEnemyNow++;
                    return;
                }
            }
        }

        private void SpawnBonus()
        {
            int buffer = 100;
            const int maxAttempts = 50;
            const float minPlayerDistance = 300f;
            const float minEnemyDistance = 50f;
            const float minBonusDistance = 100f;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector2 position = new Vector2(
                    rnd.Next(_gameArea.Left + buffer, _gameArea.Right - buffer),
                    rnd.Next(_gameArea.Top + buffer, _gameArea.Bottom - buffer)
                );

                if (Vector2.Distance(position, player.Model.Position) < minPlayerDistance)
                    continue;

                bool tooClose = _enemies.Any(e =>
                    Vector2.Distance(position, e.Model.Position) < minEnemyDistance)
                    && _bonuses.Any(e =>
                    Vector2.Distance(position, e.position) < minBonusDistance);


                if (!tooClose)
                {
                     _bonuses.Add(CreateRandomBonus(position));
                    CountBonusNow++;
                    return;
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

        private void DrawGameAreaBorders()
        {
            int borderThickness = 10;

            PrimitiveRenderer.DrawLine(
                GraphicsDevice,
                new Vector2(_gameArea.Left, _gameArea.Top),
                new Vector2(_gameArea.Right, _gameArea.Top),
                Color.White,
                borderThickness
            );

            PrimitiveRenderer.DrawLine(
                GraphicsDevice,
                new Vector2(_gameArea.Left, _gameArea.Bottom),
                new Vector2(_gameArea.Right, _gameArea.Bottom),
                Color.White,
                borderThickness
            );


            PrimitiveRenderer.DrawLine(
                GraphicsDevice,
                new Vector2(_gameArea.Left, _gameArea.Top),
                new Vector2(_gameArea.Left, _gameArea.Bottom),
                Color.White,
                borderThickness
            );

            PrimitiveRenderer.DrawLine(
                GraphicsDevice,
                new Vector2(_gameArea.Right, _gameArea.Top),
                new Vector2(_gameArea.Right, _gameArea.Bottom),
                Color.White,
                borderThickness
            );
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

            DrawGameAreaBorders();

            spriteBatch.Begin();

            foreach (var bon in _bonuses)
            {
                bon.Draw(spriteBatch, JapanSymbol);
            }

            if (battleStarted) player.Draw(GraphicsDevice);

            foreach (var enemy in _enemies)
            {
                enemy.Draw(GraphicsDevice);
            }

            DrawBullets();
            Vector2 aimPosition = GetClampedMousePosition();
            PrimitiveRenderer.DrawPoint(GraphicsDevice, aimPosition, Color.Red, 4f);

            spriteBatch.DrawString(textBlock, $"{player.Model.Health} ед. Ки", new Vector2(50, 50), Color.White);
            spriteBatch.DrawString(textBlock, $"{Name}", new Vector2(480, 50), NameColor);
            spriteBatch.DrawString(textBlock, $"1 Ступень", new Vector2(880, 50), Color.White);
            spriteBatch.DrawString(japanTextBlock, $"せ\nん\nし", new Vector2(1750, 400), Color.White);
            spriteBatch.DrawString(japanTextBlock, $"だいみょう", new Vector2(800, 940), Color.White);
            spriteBatch.DrawString(japanTextBlock, $"ぶ\nし", new Vector2(100, 400), Color.White);

            if (!battleStarted)
            {
                string text =
                "Я постиг, что Путь Самурая это смерть." +
                "В ситуации илиили без колебаний выбирай смерть.\nЭто нетрудно. Исполнись решимости и действуй." +
                "Только малодушные оправдывают себя\nрассуждениями о том, что умереть, не достигнув цели, означает" +
                "умереть собачьей смертью.\nСделать правильный выбор в ситуации или или практически невозможно." +
                "Все мы желаем\nжить, и поэтому неудивительно, что каждый пытается найти оправдание, чтобы не умирать\n" +
                "Но если человек не достиг цели и продолжает жить, он проявляет малодушие. Он\nпоступает недостойно." +
                "Если же он не достиг цели и умер, это действительно фанатизм и\nсобачья смерть. Но в этом нет ничего" +
                "постыдного. Такая смерть есть Путь Самурая. Если \nкаждое утро и каждый вечер ты будешь готовить себя" +
                "к смерти и сможешь жить так,\nсловнотвое тело уже умерло, ты станешь Подлинным самураем. Тогда вся" +
                "твоя жизнь будет\nбезупречной, и ты преуспеешь на своем поприще.";
                Vector2 textSize = textBlock.MeasureString(text);
                Vector2 position = new Vector2(320, 190);
                spriteBatch.DrawString(miniTextBlock, text, position, Color.White);
            }


            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawBullets()
        {
            foreach (var bullet in _bulletPool.ActiveBullets)
            {
                bullet.Draw(GraphicsDevice);
            }
        }

        private Bonus CreateRandomBonus(Vector2 position)
        {
            var bonusTemplates = new[]
            {
            new {
            Pattern = new AttackPattern(
                0.2f, 900f, 12, true,
                new ZRadiusBulletStrategy(GetDirectionAimPlayer, Color.White)),
            Letter = "空",
            Name = "Пустота",
            Color = Color.White,
            Health = 1
        },
        new {
            Pattern = new AttackPattern(
                0.2f, 900f, 12, true,
                new QuantumCircleStrategy(Color.Red)),
            Letter = "火",
            Name = "Огонь",
            Color = Color.Red,
            Health = 1
        },
        new {
            Pattern = new AttackPattern(
                0.2f, 900f, 1, true,
                new FractalSquareStrategy(Color.Blue)),
            Letter = "水",
            Name = "Вода",
            Color = Color.Blue,
            Health = 1
        },
        new {
            Pattern = new AttackPattern(
                0.2f, 900f, 1, true,
                new PlayerExplosiveShotStrategy(Color.Brown, Color.Brown)),
            Letter = "土",
            Name = "Земля",
            Color = Color.Brown,
            Health = 1
        },
        new {
            Pattern = new AttackPattern(
                0.2f, 900f, 1, true,
                new CrystalFanStrategy(Color.Yellow, GetDirectionAimPlayer)),
            Letter = "風",
            Name = "Ветер",
            Color = Color.Yellow,
            Health = 1
        }
    };

            var selected = bonusTemplates[rnd.Next(bonusTemplates.Length)];

            return new Bonus(
                selected.Pattern,
                position,
                selected.Letter,
                selected.Name,
                selected.Color,
                selected.Health,
                BonusLifetime
            );
        }

    }

    class Bonus
    {
        public readonly AttackPattern pattern;
        public string name;
        public readonly Color color;
        public readonly Vector2 position;
        public readonly int health;
        public readonly string letter;
        public float timeLeft { get; private set; }

        public Bonus(AttackPattern _pattern, Vector2 _position, string _letter, string _name, Color _color, int _health, float lifetime = 10f)
        {
            pattern = _pattern;
            name = _name;
            color = _color;
            position = _position;
            health = _health;
            letter = _letter;
            timeLeft = lifetime;
        }

        public void Update(float deltaTime)
        {
            timeLeft -= deltaTime;
        }

        public void ApplyEffect(PlayerModel player)
        {
            if (pattern != null)
            {
                player.AdditionalAttack = pattern;
                player.Color = color;
            }

            player.Health += health;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            Vector2 textSize = font.MeasureString(letter);
            Vector2 origin = textSize / 2f;

            spriteBatch.DrawString(
                font,
                letter,
                position,
                color
                );
        }

        public List<Vector2> GetVertices()
        {
            List<Vector2> vertices = new List<Vector2>();
            float angleStep = MathHelper.TwoPi / 8;

            for (int i = 0; i < 8; i++)
            {
                float angle = angleStep * i;
                Vector2 offset = new Vector2(
                    30 * (float)Math.Cos(angle),
                    30 * (float)Math.Sin(angle)
                );
                vertices.Add(position + offset);
            }

            return vertices;
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
