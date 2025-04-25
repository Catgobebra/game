using System;
using System.Collections.Generic;
using System.Linq;
using BulletGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.MediaFoundation;

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
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius
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

        private OptimizedBulletPool _bulletPool;

        SpriteFont textBlock;
        Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch;

        private float timer;
        private bool visible = true;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void LoadContent()
        {
            spriteBatch = new Microsoft.Xna.Framework.Graphics.SpriteBatch(GraphicsDevice);
            textBlock = Content.Load<SpriteFont>("File");
            PrimitiveRenderer.Initialize(GraphicsDevice);
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.ApplyChanges();

            _bulletPool = new OptimizedBulletPool();

            var player_model = new PlayerModel(new Vector2(640, 600));
            player = new PlayerController(player_model, new PlayerView(player_model));

            var enemy_model = new EnemyModel(
            position: new Vector2(640, 360),
            new AttackPattern(
                shootInterval: 0.1f,
                bulletSpeed: 300f,
                bulletsPerShot: 6,
                false,
                strategy: new SpiralStrategy(spiralSpeed: 2.2f,
                    radiusStep: 12.0f,
                    startColor: Color.Cyan,
                    endColor: Color.Purple)),
            Color.Crimson
            );

            enemy = new EnemyController(enemy_model, new EnemyView(enemy_model));

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _bulletPool.Cleanup();

            enemy.Update(gameTime, _bulletPool);
            UpdateBullets(gameTime);

            player.SetViewport(GraphicsDevice.Viewport);
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
                bulletSpeed: 340f,
                bulletsPerShot: 1,
                true,
                strategy: new StraightLineStrategy(directionToAim, Color.Indigo)
                ).Shoot(player.Model.Position + directionToAim * 30f, _bulletPool);
            }
            prevMouseState = mouseState;

            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timer > 0.1f)
            {
                visible = !visible;
                timer = 0;
            }

            base.Update(gameTime);
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

            foreach (var bullet in activeBullets)
            {
                if (!bullet.Model.Active) continue;

                bullet.Update(gameTime);

                if (!bullet.Model.IsPlayerBullet && bullet.CollidesWithPlayer(player))
                {
                    player.Model.Health--;
                    _bulletPool.Return(bullet);
                }

                if (bullet.IsExpired(viewport))
                    _bulletPool.Return(bullet);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            player.Draw(GraphicsDevice);
            enemy.Draw(GraphicsDevice);
     
            DrawBullets();
            var mouseState = Mouse.GetState();
            Vector2 aimPosition = new Vector2(mouseState.X, mouseState.Y);
            PrimitiveRenderer.DrawPoint(GraphicsDevice, aimPosition, Color.Red, 4f);

            spriteBatch.DrawString(textBlock, $"{player.Model.Health} Осталось душ", new Vector2(50, 50), Color.White);
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

    public class AttackPattern
    {
        public float ShootInterval { get; }
        public float BulletSpeed { get; }
        public int BulletsPerShot { get; }
        public bool isPlayerBullet { get; }
        private IAttackStrategy attackStrategy;


        public AttackPattern(float shootInterval, float bulletSpeed, int bulletsPerShot, bool playerBullet, IAttackStrategy strategy)
        {
            ShootInterval = shootInterval;
            BulletSpeed = bulletSpeed;
            BulletsPerShot = bulletsPerShot;
            attackStrategy = strategy;
            isPlayerBullet = playerBullet;
        }

        public void Shoot(Vector2 position, OptimizedBulletPool OptimizedBulletPool)
        {
            attackStrategy.Shoot(position, OptimizedBulletPool, BulletsPerShot, BulletSpeed, isPlayerBullet);
        }

    }
}