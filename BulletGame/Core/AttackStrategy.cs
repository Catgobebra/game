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