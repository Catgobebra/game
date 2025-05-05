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
        private GraphicsDeviceManager graphics;  
        private PlayerController player;
        private EnemyController enemy;
        private MouseState prevMouseState;
        private KeyboardState prevKeyboardState;

        public Random rnd = new();

        private OptimizedBulletPool _bulletPool;
        private List<EnemyController> _enemies = new List<EnemyController>();
        private List<Bonus> _bonuses = new List<Bonus>();
        private int CountEnemyNow = 0;
        private int MaxCountEnemy = 5;
        private int CountBonusNow = 0;
        private int MaxCountBonus = 1;


        private float _hpTimer = 0; 
        private float _spawnTimer;
        private const float SpawnInterval = 10f;

        private Rectangle _gameArea;
        private Viewport _gameViewport;
        private Viewport _uiViewport; 


        SpriteFont textBlock;
        SpriteFont japanTextBlock;
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
            japanTextBlock = Content.Load<SpriteFont>("Japan");
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

            var player_model = new PlayerModel(new Vector2(640, 600));
            player = new PlayerController(player_model, new PlayerView(player_model));

            var attacksPatterns = new List<AttackPattern>
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
                strategy: new RadiusBulletStrategy(player, Color.Cyan))
            };

            var enemy_model = new EnemyModel(
            position: new Vector2(640, 360),
            new AttackPattern(
                shootInterval: 0.1f,
                bulletSpeed: 300f,
                bulletsPerShot: 6,
                false,
                strategy: new RadiusBulletStrategy(player, Color.Cyan)),
            Color.Crimson
            );

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            _spawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

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
                    player.Model.Health += 20;
                    CountBonusNow--;
                    _bonuses.Remove(bonus);
                }
               

                /*if (SATCollision.CheckCollision(player.Model.GetVertices(), enemy.Model.GetVertices()))
                {
                    player.Model.Health += 20;
                }*/
            }


            //if (SATCollision.CheckCollision(player.Model.GetVertices(), bonus.GetVertices()))
            //{
                //player.Model.Health += 10;
                /*bonus.Position = new Vector2(rnd.Next(_gameArea.Left, _gameArea.Right), 
                    rnd.Next(_gameArea.Top, _gameArea.Bottom));*/
            //}

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
                Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

                Vector2 directionToAim = mousePosition - player.Model.Position;

                if (directionToAim != Vector2.Zero)
                    directionToAim.Normalize();

                new AttackPattern(
                    shootInterval: 0.2f,
                    bulletSpeed: 900f,
                    bulletsPerShot: 1,
                    true,
                    strategy: new StraightLineStrategy(directionToAim, Color.Indigo)
                ).Shoot(player.Model.Position, _bulletPool);
            }
            prevMouseState = mouseState;


            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Space) && prevKeyboardState.IsKeyUp(Keys.Space))
            {
                player.Model.Health -= 1;
                new AttackPattern(
                    shootInterval: 0.2f,
                    bulletSpeed: 900f,
                    bulletsPerShot: 1,
                    true,
                    strategy: new PlayerExplosiveShotStrategy(Color.Indigo, Color.Indigo)
                ).Shoot(player.Model.Position, _bulletPool);
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

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector2 position = new Vector2(
                    rnd.Next(_gameArea.Left + buffer, _gameArea.Right - buffer),
                    rnd.Next(_gameArea.Top + buffer, _gameArea.Bottom - buffer)
                );

                if (Vector2.Distance(position, player.Model.Position) < minPlayerDistance)
                    continue;

                bool tooClose = _enemies.Any(e =>
                    Vector2.Distance(position, e.Model.Position) < minEnemyDistance);

                if (!tooClose)
                {
                    var enemyModel = new EnemyModel(
                    position: position,
                    new AttackPattern(
                        shootInterval: 0.1f,
                        bulletSpeed: 500f,
                        bulletsPerShot: 1,
                        playerBullet: false,
                        strategy: new A_StraightLineStrategy(player, Color.Cyan)),
                    Color.Crimson
                    );
                    _enemies.Add(new EnemyController(enemyModel, new EnemyView(enemyModel)));
                    CountEnemyNow++;
                    return;
                }
            }


            /*Vector2 position = new Vector2(
                rnd.Next(_gameArea.Left + buffer, _gameArea.Right - buffer),
                rnd.Next(_gameArea.Top + buffer, _gameArea.Bottom - buffer)
            );

            var enemyModel = new EnemyModel(
                position: position,
                new AttackPattern(
                    shootInterval: 0.1f,
                    bulletSpeed: 500f,
                    bulletsPerShot: 1,
                    playerBullet: false,
                    strategy: new A_StraightLineStrategy(player, Color.Cyan)),
                Color.Crimson
            );*/
        }

        private void SpawnBonus()
        {
            int buffer = 100;
            const int maxAttempts = 50;
            const float minPlayerDistance = 300f;
            const float minEnemyDistance = 50f;
            const float minBonusDistance = 50f;

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
                    Vector2.Distance(position, e.Position) < minBonusDistance);


                if (!tooClose)
                {
                    var bonus = new Bonus();
                    bonus.Position = position;
                    _bonuses.Add(bonus);
                    CountBonusNow++;
                    return;
                }
            }


            /*Vector2 position = new Vector2(
                rnd.Next(_gameArea.Left + buffer, _gameArea.Right - buffer),
                rnd.Next(_gameArea.Top + buffer, _gameArea.Bottom - buffer)
            );

            var enemyModel = new EnemyModel(
                position: position,
                new AttackPattern(
                    shootInterval: 0.1f,
                    bulletSpeed: 500f,
                    bulletsPerShot: 1,
                    playerBullet: false,
                    strategy: new A_StraightLineStrategy(player, Color.Cyan)),
                Color.Crimson
            );*/
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
                bon.Draw(GraphicsDevice);
            }

            player.Draw(GraphicsDevice);

            foreach (var enemy in _enemies)
            {
                enemy.Draw(GraphicsDevice);
            }

            DrawBullets();
            Vector2 aimPosition = GetClampedMousePosition();
            PrimitiveRenderer.DrawPoint(GraphicsDevice, aimPosition, Color.Red, 4f);

            spriteBatch.DrawString(textBlock, $"{player.Model.Health} ед. Ки", new Vector2(50, 50), Color.White);
            spriteBatch.DrawString(japanTextBlock, $"せ\nん\nし", new Vector2(1750, 400), Color.White);
            spriteBatch.DrawString(japanTextBlock, $"だいみょう", new Vector2(800, 940), Color.White);
            spriteBatch.DrawString(japanTextBlock, $"ぶ\nし", new Vector2(100, 400), Color.White);
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
    }

    class Bonus
    {
        public AttackPattern pattern;
        public string Sprite;
        public Color color;
        public Vector2 Position = new Vector2(400, 400);

        public void Draw(GraphicsDevice device)
        {
            int scaledRadius = (int)(30 * 1f);

            PrimitiveRenderer.DrawCircle(
                device,
                Position,
                scaledRadius,
                30,
                Color.Lerp(color, Color.Red, 1 - 1f / 1.5f)
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
                vertices.Add(Position + offset);
            }

            return vertices;
        }
    }
}