using BulletGame.Core;
using BulletGame;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Input;

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
    private readonly Func<Vector2> _getDirection;
    private Color _color;

    public ZRadiusBulletStrategy(Func<Vector2> direction, Color color)
    {
        _getDirection = direction;
        _color = color;
    }

    public void Shoot(Vector2 shooterPosition, OptimizedBulletPool OptimizedBulletPool,
                    int bulletsPerShot, float bulletSpeed, bool isPlayerBullet)
    {
        Vector2 baseDirection = _getDirection();
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

public class StarPatternStrategy : IAttackStrategy
{
    private Color color;
    private float rotation;

    public StarPatternStrategy(Color color, float initialRotation = 0)
    {
        this.color = color;
        this.rotation = initialRotation;
    }

    public void Shoot(Vector2 position, OptimizedBulletPool pool,
                     int bulletsPerShot, float speed, bool isPlayer)
    {
        int points = 5;
        for (int i = 0; i < points; i++)
        {
            float angle = rotation + MathHelper.TwoPi * i / points;
            Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

            pool.GetBullet(position, dir, speed, color, isPlayer);

            float innerAngle = angle + MathHelper.PiOver2 / 2;
            Vector2 innerDir = new Vector2(
                (float)Math.Cos(innerAngle) * 0.5f,
                (float)Math.Sin(innerAngle) * 0.5f
            );
            pool.GetBullet(position, innerDir, speed, color, isPlayer);
        }
        rotation += MathHelper.ToRadians(5);
    }
}


public class PulsingCircleStrategy : IAttackStrategy
{
    private Color _color;
    private float _currentRadius;
    private float _pulseSpeed;

    public PulsingCircleStrategy(Color color, float pulseSpeed = 0.5f)
    {
        _color = color;
        _pulseSpeed = pulseSpeed;
        _currentRadius = 10f;
    }

    public void Shoot(Vector2 position, OptimizedBulletPool pool,
                     int bulletsPerShot, float speed, bool isPlayer)
    {
        float angleStep = MathHelper.TwoPi / bulletsPerShot;

        for (int i = 0; i < bulletsPerShot; i++)
        {
            float angle = angleStep * i;
            Vector2 offset = new Vector2(
                (float)Math.Cos(angle) * _currentRadius,
                (float)Math.Sin(angle) * _currentRadius
            );

            Vector2 dir = offset;
            dir.Normalize();

            pool.GetBullet(position + offset, dir, speed, _color, isPlayer);
        }

        _currentRadius += _pulseSpeed;
        if (_currentRadius > 50f) _currentRadius = 10f;
    }
}

public class CrystalFanStrategy : IAttackStrategy
{
    private readonly Color _color;
    private readonly float _fractalDepth;
    private readonly Func<Vector2> _getBaseDirection;

    public CrystalFanStrategy(Color color, Func<Vector2> getDirection, float fractalDepth = 2f)
    {
        _color = color;
        _fractalDepth = fractalDepth;
        _getBaseDirection = getDirection ?? throw new ArgumentNullException(nameof(getDirection));
    }

    public void Shoot(Vector2 position, OptimizedBulletPool pool,
                     int bulletsPerShot, float speed, bool isPlayerBullet)
    {
        Vector2 baseDirection = _getBaseDirection();
        if (baseDirection == Vector2.Zero) return;

        baseDirection.Normalize();
        CreateFan(position, baseDirection, pool, speed, isPlayerBullet, 0);
    }

    private void CreateFan(Vector2 pos, Vector2 baseDirection, OptimizedBulletPool pool,
                          float speed, bool isPlayerBullet, int generation)
    {
        if (generation > _fractalDepth) return;

        const float fanSpread = MathHelper.PiOver2;
        int branches = 3 + generation * 2;
        float angleStep = fanSpread / (branches - 1);
        float startAngle = -fanSpread * 0.5f;
        float currentSpeed = speed * (0.8f - generation * 0.2f);

        float baseAngle = (float)Math.Atan2(baseDirection.Y, baseDirection.X);

        for (int i = 0; i < branches; i++)
        {
            float angle = baseAngle + startAngle + angleStep * i;
            Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

            if (pool.GetBullet(pos, dir, currentSpeed, _color, isPlayerBullet) == null)
                return;

            if (generation < _fractalDepth)
            {
                CreateFan(pos + dir * 50f, dir, pool, speed, isPlayerBullet, generation + 1);
            }
        }
    }
}

public class FractalSquareStrategy : IAttackStrategy
{
    private Color _color;
    private int _iterations;

    public FractalSquareStrategy(Color color, int iterations = 3)
    {
        _color = color;
        _iterations = iterations;
    }

    public void Shoot(Vector2 position, OptimizedBulletPool pool,
                     int bulletsPerShot, float speed, bool isPlayer)
    {
        CreateSquareLayer(position, pool, speed, isPlayer, 0);
    }

    private void CreateSquareLayer(Vector2 center, OptimizedBulletPool pool,
                                  float speed, bool isPlayer, int iteration)
    {
        if (iteration > _iterations) return;

        int bulletsPerSide = 4 + iteration * 2;
        float size = 50f * (float)Math.Pow(0.5f, iteration);

        for (int side = 0; side < 4; side++)
        {
            for (int i = 0; i < bulletsPerSide; i++)
            {
                Vector2 offset = Vector2.Zero;
                float t = (float)i / (bulletsPerSide - 1);

                switch (side)
                {
                    case 0: offset = new Vector2(-size + 2 * size * t, -size); break;
                    case 1: offset = new Vector2(size, -size + 2 * size * t); break;
                    case 2: offset = new Vector2(size - 2 * size * t, size); break;
                    case 3: offset = new Vector2(-size, size - 2 * size * t); break;
                }

                Vector2 dir = Vector2.Normalize(offset);
                pool.GetBullet(center + offset, dir, speed * (1f - iteration * 0.2f),
                             _color, isPlayer);
            }
        }

        CreateSquareLayer(center, pool, speed, isPlayer, iteration + 1);
    }
}

public class QuantumCircleStrategy : IAttackStrategy
{
    private Color _color;
    private float _phaseShift;

    public QuantumCircleStrategy(Color color, float phaseShift = 0.3f)
    {
        _color = color;
        _phaseShift = phaseShift;
    }

    public void Shoot(Vector2 position, OptimizedBulletPool pool,
                     int bulletsPerShot, float speed, bool isPlayer)
    {
        float angleStep = MathHelper.TwoPi / bulletsPerShot;

        for (int i = 0; i < bulletsPerShot; i++)
        {
            float baseAngle = angleStep * i;
            Vector2 dir = new Vector2(
                (float)Math.Sin(baseAngle * 3 + _phaseShift),
                (float)Math.Cos(baseAngle * 2 - _phaseShift)
            );

            dir.Normalize();
            pool.GetBullet(position, dir, speed, _color, isPlayer);
        }
    }
}

public class LotusPatternStrategy : IAttackStrategy
{
    private Color _color;
    private int _layers;
    private float _spread;

    public LotusPatternStrategy(Color color, int layers = 4, float spread = 0.3f)
    {
        _color = color;
        _layers = layers;
        _spread = spread;
    }

    public void Shoot(Vector2 position, OptimizedBulletPool pool,
                     int bulletsPerShot, float speed, bool isPlayer)
    {
        int bulletsPerLayer = bulletsPerShot / _layers;

        for (int layer = 0; layer < _layers; layer++)
        {
            float angleOffset = layer * MathHelper.PiOver4;
            float speedMultiplier = 1f - (layer * 0.1f);

            for (int i = 0; i < bulletsPerLayer; i++)
            {
                float angle = MathHelper.TwoPi * i / bulletsPerLayer + angleOffset;
                Vector2 direction = new Vector2(
                    (float)Math.Cos(angle + _spread * layer),
                    (float)Math.Sin(angle + _spread * layer)
                );

                pool.GetBullet(position, direction, speed * speedMultiplier, _color, isPlayer);
            }
        }
    }
}

public class WavePatternStrategy : IAttackStrategy
{
    private float _waveFrequency;
    private float _waveAmplitude;
    private Color _color;
    private float _phase;

    public WavePatternStrategy(float frequency, float amplitude, Color color)
    {
        _waveFrequency = frequency;
        _waveAmplitude = amplitude;
        _color = color;
        _phase = 0f;
    }

    public void Shoot(Vector2 position, OptimizedBulletPool pool,
                     int bulletsPerShot, float speed, bool isPlayer)
    {
        float angleStep = MathHelper.TwoPi / bulletsPerShot;

        for (int i = 0; i < bulletsPerShot; i++)
        {
            float angle = angleStep * i + _phase;
            float waveOffset = (float)Math.Sin(angle * _waveFrequency) * _waveAmplitude;

            Vector2 direction = new Vector2(
                (float)Math.Cos(angle + waveOffset),
                (float)Math.Sin(angle + waveOffset)
            );

            pool.GetBullet(position, direction, speed, _color, isPlayer);
        }

        _phase += 0.1f;
    }
}

public class MirrorSpiralStrategy : IAttackStrategy
{
    private float _angle;
    private readonly Color _color;
    private readonly bool _mirror;

    public MirrorSpiralStrategy(Color color, bool mirror)
    {
        _color = color;
        _mirror = mirror;
    }

    public void Shoot(Vector2 position, OptimizedBulletPool pool,
                     int bulletsPerShot, float speed, bool isPlayer)
    {
        float step = MathHelper.TwoPi / bulletsPerShot;
        for (int i = 0; i < bulletsPerShot; i++)
        {
            float dirAngle = _angle + step * i * (_mirror ? -1 : 1);
            Vector2 dir = new Vector2((float)Math.Cos(dirAngle), (float)Math.Sin(dirAngle));
            pool.GetBullet(position, dir, speed, _color, isPlayer);
        }
        _angle += MathHelper.ToRadians(2);
    }
}

public class RotatingLotusStrategy : IAttackStrategy
{
    private float _rotation;
    private readonly Color _color;
    private readonly float _rotationSpeed;

    public RotatingLotusStrategy(Color color, float rotationSpeed)
    {
        _color = color;
        _rotationSpeed = rotationSpeed;
    }

    public void Shoot(Vector2 position, OptimizedBulletPool pool,
                     int bulletsPerShot, float speed, bool isPlayer)
    {
        float angleStep = MathHelper.TwoPi / bulletsPerShot;
        for (int i = 0; i < bulletsPerShot; i++)
        {
            float angle = angleStep * i + _rotation;
            Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            pool.GetBullet(position, dir, speed, _color, isPlayer);
        }
        _rotation += _rotationSpeed;
    }
}

public class PulsingQuantumStrategy : IAttackStrategy
{
    private float _time;
    private readonly Color _baseColor;
    private readonly float _pulseSpeed;
    private readonly float _radiusMultiplier;

    public PulsingQuantumStrategy(Color baseColor, float pulseSpeed = 3f, float radiusMultiplier = 2f)
    {
        _baseColor = baseColor;
        _pulseSpeed = pulseSpeed;
        _radiusMultiplier = radiusMultiplier;
    }

    public void Shoot(Vector2 position, OptimizedBulletPool pool,
                     int bulletsPerShot, float speed, bool isPlayer)
    {
        _time += 0.016f; 

        float pulse = (float)(Math.Sin(_time * _pulseSpeed) + 1) / 2f;
        float currentRadius = 50f + 100f * pulse * _radiusMultiplier;

        CreateQuantumRing(position, pool, speed, isPlayer, currentRadius, pulse);

        CreateCentralImpulse(position, pool, speed, isPlayer, pulse);
    }

    private void CreateQuantumRing(Vector2 center, OptimizedBulletPool pool,
                                  float speed, bool isPlayer, float radius, float pulse)
    {
        int ringBullets = 24;
        float angleStep = MathHelper.TwoPi / ringBullets;

        Color pulseColor = new Color(_baseColor, 0.3f + 0.7f * pulse);

        for (int i = 0; i < ringBullets; i++)
        {
            float angle = angleStep * i + _time * 2f;
            Vector2 offset = new Vector2(
                (float)Math.Cos(angle) * radius,
                (float)Math.Sin(angle) * radius
            );

            Vector2 direction = Vector2.Normalize(offset);
            pool.GetBullet(center + offset, direction, speed * 0.8f, pulseColor, isPlayer);
        }
    }

    private void CreateCentralImpulse(Vector2 center, OptimizedBulletPool pool,
                                     float speed, bool isPlayer, float pulse)
    {
        int impulseBullets = 8;
        float angleStep = MathHelper.TwoPi / impulseBullets;
        Color impulseColor = Color.Lerp(_baseColor, Color.White, pulse);

        for (int i = 0; i < impulseBullets; i++)
        {
            float angle = angleStep * i + _time * 3f;
            Vector2 direction = new Vector2(
                (float)Math.Cos(angle),
                (float)Math.Sin(angle)
            );

            pool.GetBullet(center, direction, speed * 1.2f, impulseColor, isPlayer);
        }
    }
}
public class QuantumThreadStrategy : IAttackStrategy
{
    private float _phase;
    private readonly Color _colorA;
    private readonly Color _colorB;
    private readonly float _waveSpeed;

    public QuantumThreadStrategy(Color colorA, Color colorB, float waveSpeed = 2f)
    {
        _colorA = colorA;
        _colorB = colorB;
        _waveSpeed = waveSpeed;
    }

    public void Shoot(Vector2 position, OptimizedBulletPool pool,
                     int bulletsPerShot, float speed, bool isPlayer)
    {
        _phase += 0.02f * _waveSpeed;

        for (int i = 0; i < bulletsPerShot; i++)
        {
            float t = (float)i / bulletsPerShot;
            float waveOffset = (float)Math.Sin(_phase + t * MathHelper.TwoPi);

            Vector2 dir = new Vector2(t, waveOffset);
            dir.Normalize();

            Color color = Color.Lerp(_colorA, _colorB, (waveOffset + 1) / 2);

            var bullet = pool.GetBullet(position, dir, speed * 1.5f, color, isPlayer);
        }
    }
}