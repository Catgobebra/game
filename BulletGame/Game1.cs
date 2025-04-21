using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BulletGame
{
    public interface IAttackStrategy
    {
        void Shoot(Vector2 position, List<Bullet> bullets, int bulletsPerShot, float bulletSpeed);
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

        public void Shoot(Vector2 position, List<Bullet> bullets, int bulletsPerShot, float bulletSpeed)
        {
            for (int i = 0; i < bulletsPerShot; i++)
            {
                bullets.Add(new Bullet(position, direction, bulletSpeed, color));
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

        public void Shoot(Vector2 position, List<Bullet> bullets, int bulletsPerShot, float bulletSpeed)
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
                bullets.Add(new Bullet(position, direction, bulletSpeed, color));
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

        public void Shoot(Vector2 position, List<Bullet> bullets, int bulletsPerShot, float bulletSpeed)
        {
            for (int i = 0; i < bulletsPerShot; i++)
            {
                float theta = angleOffset + MathHelper.TwoPi * i / bulletsPerShot;
                Vector2 direction = new Vector2(
                    (float)Math.Pow(Math.Cos(theta), 3),
                    (float)Math.Pow(Math.Sin(theta), 3)
                );
                direction.Normalize();

                bullets.Add(new Bullet(position, direction, bulletSpeed, color));
            }

            angleOffset += speedFactor;
            if (angleOffset >= MathHelper.TwoPi) angleOffset -= MathHelper.TwoPi;
        }
    }

    public class Player
    {
        public Vector2 Position { get; private set; }
        public Vector2 Direction { get; private set; }
        public float Speed { get; } = 500f;
        public float Size { get; } = 20f;
        public Color Color { get; } = Color.LimeGreen;

        public Player(Vector2 startPosition)
        {
            Position = startPosition;
            Direction = Vector2.UnitY;
        }

        public void Update(GameTime gameTime, GraphicsDevice graphicsDevice)
        {
            // Управление с клавиатуры
            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            // Получаем направление к курсору мыши
            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);
            Direction = Vector2.Normalize(mousePosition - Position);

            // Движение
            Vector2 moveDirection = Vector2.Zero;

            if (keyboardState.IsKeyDown(Keys.W)) moveDirection.Y -= 1;
            if (keyboardState.IsKeyDown(Keys.S)) moveDirection.Y += 1;
            if (keyboardState.IsKeyDown(Keys.A)) moveDirection.X -= 1;
            if (keyboardState.IsKeyDown(Keys.D)) moveDirection.X += 1;

            if (moveDirection != Vector2.Zero)
            {
                moveDirection.Normalize();
                Position += moveDirection * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            // Ограничение в пределах экрана
            Position = Vector2.Clamp(Position,
                new Vector2(Size, Size),
                new Vector2(graphicsDevice.Viewport.Width - Size,
                          graphicsDevice.Viewport.Height - Size));
        }

        public void Draw(GraphicsDevice device)
        {
            PrimitiveRenderer.DrawTriangle(
                device,
                Position,
                Direction,
                Size,
                Color
            );
        }
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private List<Bullet> bullets;
        private Enemy enemy;
        private Player player;
        private MouseState prevMouseState;
        private AttackPattern attackPattern;


        private float timer;
        private bool visible = true;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            bullets = new List<Bullet>();
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.ApplyChanges();

            enemy = new Enemy(
            position: new Vector2(640, 360),
            /* new AttackPattern(
                 shootInterval: 0.1f,
                 bulletSpeed: 500f,
                 bulletsPerShot: 6,
                 strategy: new SpiralStrategy( // Спиральная атака
                     spiralSpeed: 2.2f,
                     radiusStep: 2.0f,
                     startColor: Color.Cyan,
                     endColor: Color.Purple
                 )
             ),*/
            new AttackPattern(
            shootInterval: 0.2f,
            bulletSpeed: 400f,
            bulletsPerShot: 5,
            strategy: new StraightLineStrategy(new Vector2(0, 1), Color.Aqua)
            ),

            /*new AttackPattern(
            shootInterval: 0.15f,
            bulletSpeed: 350f,
            bulletsPerShot: 20,
            strategy: new AstroidStrategy(0.5f, Color.Yellow)
            ), */
            Color.Crimson
            );

            player = new Player(new Vector2(640, 600));

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            enemy.Update(gameTime, bullets);
            UpdateBullets(gameTime);
            player.Update(gameTime, GraphicsDevice);

            var mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed &&
               prevMouseState.LeftButton == ButtonState.Released)
            {
                Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

                // Вычисляем вектор направления
                Vector2 directionToAim = mousePosition - player.Position;

                // Нормализуем вектор (делаем длину = 1)
                if (directionToAim != Vector2.Zero)
                    directionToAim.Normalize();

                new AttackPattern(
                shootInterval: 0.2f,
                bulletSpeed: 400f,
                bulletsPerShot: 5,
                strategy: new StraightLineStrategy(directionToAim, Color.Aqua)
                ).Shoot(player.Position,bullets);
            }
            prevMouseState = mouseState;

            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timer > 0.1f) // Частота мигания
            {
                visible = !visible;
                timer = 0;
            }


            base.Update(gameTime);
        }

        private void UpdateBullets(GameTime gameTime)
        {
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                bullets[i].Update(gameTime);
                if (bullets[i].IsExpired(graphics))
                    bullets.RemoveAt(i);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            player.Draw(GraphicsDevice);
            enemy.Draw(GraphicsDevice);
            ///sdsdsd
            DrawBullets();
            var mouseState = Mouse.GetState();
            Vector2 aimPosition = new Vector2(mouseState.X, mouseState.Y);
            PrimitiveRenderer.DrawPoint(GraphicsDevice, aimPosition, Color.Red, 4f);

            base.Draw(gameTime);
        }

        private void DrawBullets()
        {
            foreach (var bullet in bullets)
                bullet.Draw(GraphicsDevice);
        }
    }

    public class Enemy
    {
        private Vector2 position;
        private AttackPattern attackPattern;
        private Color color;
        private float shootTimer;

        public Enemy(Vector2 position, AttackPattern pattern, Color color)
        {
            this.position = position;
            this.attackPattern = pattern;
            this.color = color;
            shootTimer = pattern.ShootInterval;
        }

        public void Update(GameTime gameTime, List<Bullet> bullets)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            shootTimer -= deltaTime;

            if (shootTimer <= 0)
            {
                attackPattern.Shoot(position, bullets);
                shootTimer = attackPattern.ShootInterval;
            }
        }

        public void Draw(GraphicsDevice device)
        {
            PrimitiveRenderer.DrawCircle(device, position, 30, 32, color);
        }
    }

    public class AttackPattern
    {
        public float ShootInterval { get; }
        public float BulletSpeed { get; }
        public int BulletsPerShot { get; }
        private IAttackStrategy attackStrategy;

        public AttackPattern(float shootInterval, float bulletSpeed, int bulletsPerShot, IAttackStrategy strategy)
        {
            ShootInterval = shootInterval;
            BulletSpeed = bulletSpeed;
            BulletsPerShot = bulletsPerShot;
            attackStrategy = strategy;
        }

        public void Shoot(Vector2 position, List<Bullet> bullets)
        {
            attackStrategy.Shoot(position, bullets, BulletsPerShot, BulletSpeed);
        }
    }

    public class Bullet
    {
        private Vector2 position;
        private Vector2 direction;
        private float speed;
        private Color color;

        public Bullet(Vector2 startPosition, Vector2 direction, float speed, Color color)
        {
            this.position = startPosition;
            this.direction = direction;
            this.speed = speed;
            this.color = color;
        }

        public void Update(GameTime gameTime)
        {
            position += direction * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public bool IsExpired(GraphicsDeviceManager graphics)
        {
            return position.X < 0 || position.X > graphics.PreferredBackBufferWidth ||
                   position.Y < 0 || position.Y > graphics.PreferredBackBufferHeight;
        }

        public void Draw(GraphicsDevice device)
        {
            PrimitiveRenderer.DrawBullet(device, position, direction, 20f, 12f, color);
        }
    }

    public static class PrimitiveRenderer
    {
        public static void DrawPoint(GraphicsDevice device, Vector2 position, Color color, float size = 3f)
        {
            // Рисуем маленький круг как точку
            DrawCircle(device, position, (int)size, 8, color);
        }

        public static void DrawTriangle(GraphicsDevice device, Vector2 position,
                              Vector2 direction, float size, Color color)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[3];
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);

            // Передняя вершина
            vertices[0] = new VertexPositionColor(
                new Vector3(position + direction * size, 0), color);

            // Задние вершины
            vertices[1] = new VertexPositionColor(
                new Vector3(position - direction * size / 2 + perpendicular * size / 2, 0), color);

            vertices[2] = new VertexPositionColor(
                new Vector3(position - direction * size / 2 - perpendicular * size / 2, 0), color);

            DrawPrimitives(device, vertices, PrimitiveType.TriangleList, 1);
        }

        public static void DrawCircle(GraphicsDevice device, Vector2 position, int radius, int segments, Color color)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[segments * 3];

            for (int i = 0; i < segments; i++)
            {
                float angle1 = MathHelper.TwoPi * i / segments;
                float angle2 = MathHelper.TwoPi * (i + 1) / segments;

                Vector2 p1 = position + new Vector2((float)Math.Cos(angle1) * radius, (float)Math.Sin(angle1) * radius);
                Vector2 p2 = position + new Vector2((float)Math.Cos(angle2) * radius, (float)Math.Sin(angle2) * radius);

                vertices[i * 3] = new VertexPositionColor(new Vector3(position, 0), color);
                vertices[i * 3 + 1] = new VertexPositionColor(new Vector3(p1, 0), color);
                vertices[i * 3 + 2] = new VertexPositionColor(new Vector3(p2, 0), color);
            }

            DrawPrimitives(device, vertices, PrimitiveType.TriangleList, segments);
        }

        public static void DrawBullet(GraphicsDevice device, Vector2 position, Vector2 direction,
                                    float length, float width, Color color)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[3];
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);

            vertices[0] = new VertexPositionColor(new Vector3(position + direction * length, 0), color);
            vertices[1] = new VertexPositionColor(new Vector3(position - direction * length / 2 + perpendicular * width / 2, 0), color);
            vertices[2] = new VertexPositionColor(new Vector3(position - direction * length / 2 - perpendicular * width / 2, 0), color);

            DrawPrimitives(device, vertices, PrimitiveType.TriangleList, 1);
        }

        private static void DrawPrimitives(GraphicsDevice device, VertexPositionColor[] vertices,
                                         PrimitiveType type, int primitiveCount)
        {
            BasicEffect effect = new BasicEffect(device)
            {
                World = Matrix.Identity,
                View = Matrix.Identity,
                Projection = Matrix.CreateOrthographicOffCenter(0, device.Viewport.Width, device.Viewport.Height, 0, 0, 1),
                VertexColorEnabled = true
            };

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserPrimitives(type, vertices, 0, primitiveCount);
            }
        }
    }
}