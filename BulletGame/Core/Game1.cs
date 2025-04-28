using System;
using System.Collections.Generic;
using System.Linq;
using BulletGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace BulletGame
{
    public interface IAttackStrategy
    {
        void Shoot(Vector2 position, OptimizedBulletPool OptimizedBulletPool, int bulletsPerShot, float bulletSpeed, bool isPlayerBullet);
    }

    public class StraightLineStrategy : IAttackStrategy
    {
        private Vector2 direction;
        private Color color;

        public StraightLineStrategy(Vector2 direction, Color color)
        {
            this.direction = Vector2.Normalize(direction);
            this.color = color;
        }

        public void Shoot(Vector2 position, OptimizedBulletPool OptimizedBulletPool, int bulletsPerShot, float bulletSpeed, bool isPlayerBullet)
        {
            for (int i = 0; i < bulletsPerShot; i++)
            {
                var bullet = OptimizedBulletPool.GetBullet(position, direction, bulletSpeed, color, isPlayerBullet);
                if (bullet == null) break;
            }
        }
    }

    public class A_StraightLineStrategy : IAttackStrategy
    {
        private readonly PlayerController target;
        private Color color;

        public A_StraightLineStrategy(PlayerController direction, Color color)
        {
            this.target = direction;
            this.color = color;
        }

        public void Shoot(Vector2 position, OptimizedBulletPool OptimizedBulletPool, int bulletsPerShot, float bulletSpeed, bool isPlayerBullet)
        {
            Vector2 direction = target.Model.Position - position;
            direction.Normalize();
            for (int i = 0; i < bulletsPerShot; i++)
            {
                if (OptimizedBulletPool.GetBullet(position, direction, bulletSpeed, color, isPlayerBullet) == null) return;
            }
        }
    }

    public class RadiusBulletStrategy : IAttackStrategy
    {
        private readonly PlayerController _target;
        private Color _color;

        public RadiusBulletStrategy(PlayerController target, Color color)
        {
            _target = target;
            _color = color;
        }

        public void Shoot(Vector2 shooterPosition, OptimizedBulletPool OptimizedBulletPool,
                        int bulletsPerShot, float bulletSpeed, bool isPlayerBullet)
        {
            Vector2 baseDirection = _target.Model.Position - shooterPosition;
            baseDirection.Normalize();

            float totalSpreadAngle = 90f;
            float angleStep = totalSpreadAngle / (bulletsPerShot - 1);
            float startAngle = -totalSpreadAngle / 2;

            for (int i = 0; i < bulletsPerShot; i++)
            {
                float currentAngle = startAngle + angleStep * i;
                float radians = MathHelper.ToRadians(currentAngle);

                Matrix rotationMatrix = Matrix.CreateRotationZ(radians);
                Vector2 dir = Vector2.Transform(baseDirection, rotationMatrix);
                dir.Normalize();

                OptimizedBulletPool.GetBullet(shooterPosition, dir, bulletSpeed, _color, isPlayerBullet);
            }
        }
    }

    public class ZRadiusBulletStrategy : IAttackStrategy
    {
        private Vector2 _direction;
        private Color _color;

        public ZRadiusBulletStrategy(Vector2 direction, Color color)
        {
            _direction = direction;
            _color = color;
        }

        public void Shoot(Vector2 shooterPosition, OptimizedBulletPool OptimizedBulletPool,
                        int bulletsPerShot, float bulletSpeed, bool isPlayerBullet)
        {
            Vector2 baseDirection = _direction;
            baseDirection.Normalize();

            float totalSpreadAngle = 90f;
            float angleStep = totalSpreadAngle / (bulletsPerShot - 1);
            float startAngle = -totalSpreadAngle / 2;

            for (int i = 0; i < bulletsPerShot; i++)
            {
                float currentAngle = startAngle + angleStep * i;
                float radians = MathHelper.ToRadians(currentAngle);

                Matrix rotationMatrix = Matrix.CreateRotationZ(radians);
                Vector2 dir = Vector2.Transform(baseDirection, rotationMatrix);
                dir.Normalize();

                OptimizedBulletPool.GetBullet(shooterPosition, dir, bulletSpeed, _color, isPlayerBullet);
            }
        }
    }


    public class SpiralStrategy : IAttackStrategy
    {
        private float spiralSpeed;
        private float radiusStep;
        private float angleOffset;
        private Color startColor;
        private Color endColor;

        public SpiralStrategy(float spiralSpeed, float radiusStep, Color startColor, Color endColor)
        {
            this.spiralSpeed = spiralSpeed;
            this.radiusStep = radiusStep;
            this.startColor = startColor;
            this.endColor = endColor;
            angleOffset = 0f;
        }

        public void Shoot(Vector2 position, OptimizedBulletPool OptimizedBulletPool, int bulletsPerShot, float bulletSpeed, bool isPlayerBullet)
        {
            for (int i = 0; i < bulletsPerShot; i++)
            {
                float angle = angleOffset + MathHelper.TwoPi * i / bulletsPerShot;
                float radius = 1f + radiusStep * i;

                Vector2 direction = new Vector2(
                    (float)Math.Sin(angle) * radius,
                    (float)Math.Cos(angle) * radius
                );
                direction.Normalize();

                Color color = Color.Lerp(startColor, endColor, (float)i / bulletsPerShot);

                if (OptimizedBulletPool.GetBullet(position, direction, bulletSpeed, color, isPlayerBullet) == null) return;
            }

            angleOffset += spiralSpeed;
            if (angleOffset >= MathHelper.TwoPi) angleOffset -= MathHelper.TwoPi;
        }
    }

    public class AstroidStrategy : IAttackStrategy
    {
        private float angleOffset;
        private float speedFactor;
        private Color color;

        public AstroidStrategy(float speedFactor, Color color)
        {
            this.speedFactor = speedFactor;
            this.color = color;
            angleOffset = 0f;
        }

        public void Shoot(Vector2 position, OptimizedBulletPool OptimizedBulletPool, int bulletsPerShot, float bulletSpeed, bool isPlayerBullet)
        {
            for (int i = 0; i < bulletsPerShot; i++)
            {
                float theta = angleOffset + MathHelper.TwoPi * i / bulletsPerShot;
                Vector2 direction = new Vector2(
                    (float)Math.Pow(Math.Cos(theta), 3),
                    (float)Math.Pow(Math.Sin(theta), 3)
                );
                direction.Normalize();

                OptimizedBulletPool.GetBullet(position, direction, bulletSpeed, color, isPlayerBullet);
            }

            angleOffset += speedFactor;
            if (angleOffset >= MathHelper.TwoPi) angleOffset -= MathHelper.TwoPi;
        }
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private PlayerController player;
        private EnemyController enemy;
        private MouseState prevMouseState;

        public Random rnd = new();

        private OptimizedBulletPool _bulletPool;
        private List<EnemyController> _enemies = new List<EnemyController>();
        private int CountEnemyNow = 0;
        private int MaxCountEnemy = 5;

        private float _hpTimer = 0; 
        private float _spawnTimer;
        private const float SpawnInterval = 5f;

        private Rectangle _gameArea; // Область игрового поля
        private Viewport _gameViewport; // Вид для игровых объектов
        private Viewport _uiViewport;   // Вид для интерфейса



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

            /*var enemy_model = new EnemyModel(
            position: new Vector2(640, 360),
            new AttackPattern(
                shootInterval: 0.1f,
                bulletSpeed: 500f,
                bulletsPerShot: 6,
                false,
                strategy: new SpiralStrategy( // Спиральная атака
                    spiralSpeed: 2.2f,
                    radiusStep: 2.0f,
                    startColor: Color.Cyan,
                    endColor: Color.Purple)),
            Color.Crimson
            );*/
            /*var enemy_model = new EnemyModel(
            position: new Vector2(640, 360),
            new AttackPattern(
                shootInterval: 0.1f,
                bulletSpeed: 500f,
                bulletsPerShot: 6,
                false,
                strategy: new A_StraightLineStrategy(player, Color.Cyan)),
            Color.Crimson
            );*/
            /*var enemy_model = new EnemyModel(
            position: new Vector2(640, 360),
            new AttackPattern(
                shootInterval: 0.1f,
                bulletSpeed: 300f,
                bulletsPerShot: 6,
                false,
                strategy: new RadiusBulletStrategy(player, Color.Cyan)),
            Color.Crimson
            );*/
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

            //_enemies.Add(new EnemyController(enemy_model, new EnemyView(enemy_model)));

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            _spawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_spawnTimer >= SpawnInterval && MaxCountEnemy > CountEnemyNow)
            {
                SpawnEnemy();
                _spawnTimer = 0f;
            }

            // Обновляем всех врагов
            foreach (var enemy in _enemies.ToList())
            {
                enemy.Update(gameTime, _bulletPool);

                // Удаляем уничтоженных врагов
                /*if (enemy.Model.IsDestroyed)
                {
                    _enemies.Remove(enemy);
                }*/
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
                Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

                Vector2 directionToAim = mousePosition - player.Model.Position;

                if (directionToAim != Vector2.Zero)
                    directionToAim.Normalize();

                new AttackPattern(
                    shootInterval: 0.2f,
                    bulletSpeed: 900f,
                    bulletsPerShot: 1,
                    true,
                    strategy: new PlayerExplosiveShotStrategy(Color.Beige, Color.Indigo)
                ).Shoot(player.Model.Position, _bulletPool);
            }
            prevMouseState = mouseState;

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
            // Генерация позиции с учетом границ экрана
            int buffer = 100;
            const int maxAttempts = 50; // Лимит попыток генерации
            const float minPlayerDistance = 300f; // Минимум 300px от игрока
            const float minEnemyDistance = 150f;  // Минимум 150px между врагами

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // Генерация позиции
                Vector2 position = new Vector2(
                    rnd.Next(_gameArea.Left + buffer, _gameArea.Right - buffer),
                    rnd.Next(_gameArea.Top + buffer, _gameArea.Bottom - buffer)
                );

                // Проверка расстояния до игрока
                if (Vector2.Distance(position, player.Model.Position) < minPlayerDistance)
                    continue;

                // Проверка расстояния до других врагов
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
                        //Exit();
                        // Наносим урон врагу
                        enem_.Model.Health -= 1;
                        enem_.Model.TriggerHitAnimation();
                        // Уничтожаем пулю
                        _bulletPool.Return(bullet);
                        // Удаляем врага если здоровье закончилось
                        if (enem_.Model.Health <= 0)
                        {
                            _enemies.Remove(enem_);
                            CountEnemyNow--;
                        }
                        //break;
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
            // Толщина линий границы
            int borderThickness = 10;

            // Верхняя граница
            PrimitiveRenderer.DrawLine(
                GraphicsDevice,
                new Vector2(_gameArea.Left, _gameArea.Top),
                new Vector2(_gameArea.Right, _gameArea.Top),
                Color.White,
                borderThickness
            );

            // Нижняя граница
            PrimitiveRenderer.DrawLine(
                GraphicsDevice,
                new Vector2(_gameArea.Left, _gameArea.Bottom),
                new Vector2(_gameArea.Right, _gameArea.Bottom),
                Color.White,
                borderThickness
            );


            // Левая граница
            PrimitiveRenderer.DrawLine(
                GraphicsDevice,
                new Vector2(_gameArea.Left, _gameArea.Top),
                new Vector2(_gameArea.Left, _gameArea.Bottom),
                Color.White,
                borderThickness
            );

            // Правая граница
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

            // Ограничиваем координаты мыши границами экрана
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
        public Vector2 Position { get; private set; }
    }

    public class PlayerExplosiveShotStrategy : IAttackStrategy
    {
        private Color _mainColor;
        private Color _explosionColor;

        public PlayerExplosiveShotStrategy(Color mainColor, Color explosionColor)
        {
            _mainColor = mainColor;
            _explosionColor = explosionColor;
        }

        public void Shoot(Vector2 position, OptimizedBulletPool bulletPool,
                         int bulletsPerShot, float bulletSpeed, bool isPlayerBullet)
        {
            // Основной выстрел
            //Vector2 mainDirection = GetMouseDirection(position);
            //mainDirection.Normalize();
            //bulletPool.GetBullet(position, mainDirection, bulletSpeed, _mainColor, isPlayerBullet);

            // Взрывные частицы
            const int explosionParticles = 12;
            for (int i = 0; i < explosionParticles; i++)
            {
                float angle = MathHelper.TwoPi * i / explosionParticles;
                Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                bulletPool.GetBullet(position, dir, bulletSpeed * 0.7f, _explosionColor, isPlayerBullet);
            }
        }

        private Vector2 GetMouseDirection(Vector2 shooterPosition)
        {
            MouseState mouseState = Mouse.GetState();
            return new Vector2(mouseState.X, mouseState.Y) - shooterPosition;
        }
    }

}