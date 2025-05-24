using BulletGame.Core;
using BulletGame;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using static BulletGame.Game1;

public class InputHandler
{
    private readonly PlayerController _player;
    private readonly Game1 _game;
    private readonly OptimizedBulletPool _bulletPool;
    private readonly Rectangle _gameArea;
    public bool IsSkipRequested { get; set; }

    private float _skipCooldownTimer;
    private const float SkipCooldown = 0.5f;

    private MouseState _prevMouseState;
    private KeyboardState _prevKeyboardState;

    public InputHandler(PlayerController player, Game1 game,
                      OptimizedBulletPool bulletPool, Rectangle gameArea)
    {
        _player = player;
        _game = game;
        _bulletPool = bulletPool;
        _gameArea = gameArea;
    }

    public void Update(GameTime gameTime)
    {
        HandleGameInput(gameTime);
        HandleSystemInput();
        HandleMouseInput(gameTime);
    }

    private void HandleGameInput(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.Space) && _prevKeyboardState.IsKeyUp(Keys.Space))
        {
            PerformSpecialAttack();
        }

        IsSkipRequested = keyboardState.IsKeyDown(Keys.Space);

        _prevKeyboardState = keyboardState;
    }

    private void HandleSystemInput()
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            _game.ResetGameState();
            _game._currentState = GameState.Menu;
        }
    }

    private void HandleMouseInput(GameTime gameTime)
    {
        var mouseState = Mouse.GetState();
        _player.Model.AimPosition = GetClampedMousePosition(mouseState);

        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            HandleMainAttack(gameTime);
        }
        else
        {
            _player.Model.ShootTimer = 0f;
        }

        _prevMouseState = mouseState;
    }

    private Vector2 GetClampedMousePosition(MouseState mouseState)
    {
        return new Vector2(
            MathHelper.Clamp(mouseState.X, _gameArea.Left, _gameArea.Right),
            MathHelper.Clamp(mouseState.Y, _gameArea.Top, _gameArea.Bottom)
        );
    }

    private void HandleMainAttack(GameTime gameTime)
    {
        float shootTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _player.Model.ShootTimer += shootTime;

        if (_player.Model.ShootTimer >= 0.2f)
        {
            Vector2 direction = GetAttackDirection();
            new AttackPattern(
                shootInterval: 0.2f,
                bulletSpeed: 900f,
                bulletsPerShot: 1,
                true,
                strategy: new StraightLineStrategy(direction, _player.Model.Color)
            ).Shoot(_player.Model.Position, _bulletPool);

            _player.Model.ShootTimer = 0f;
        }
    }

    public Vector2 GetDirectionAimPlayer()
    {
        Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        Vector2 directionToAim = mousePosition - _player.Model.Position;

        if (directionToAim != Vector2.Zero)
            directionToAim.Normalize();

        return directionToAim;
    }

    private void PerformSpecialAttack()
    {
        Vector2 direction = GetAttackDirection();
        _player.Model.Health -= _player.Model.BonusHealth;
        _player.Model.AdditionalAttack.Shoot(_player.Model.Position, _bulletPool);
    }

    public Vector2 GetAttackDirection()
    {
        Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        Vector2 direction = mousePosition - _player.Model.Position;

        if (direction != Vector2.Zero)
            direction.Normalize();

        return direction;
    }
}