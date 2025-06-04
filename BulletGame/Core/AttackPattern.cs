using BulletGame.Core;
using Microsoft.Xna.Framework;

public class AttackPattern
{
    public float ShootInterval { get; }
    public float BulletSpeed { get; }
    public int BulletsPerShot { get; }
    public bool IsPlayerBullet { get; }
    private IAttackStrategy attackStrategy;


    public AttackPattern(float shootInterval, float bulletSpeed, int bulletsPerShot, bool playerBullet, IAttackStrategy strategy)
    {
        ShootInterval = shootInterval;
        BulletSpeed = bulletSpeed;
        BulletsPerShot = bulletsPerShot;
        attackStrategy = strategy;
        IsPlayerBullet = playerBullet;
    }

    public void Shoot(Vector2 position, OptimizedBulletPool OptimizedBulletPool)
    {
        attackStrategy.Shoot(position, OptimizedBulletPool, BulletsPerShot, BulletSpeed, IsPlayerBullet);
    }

}