using System.Collections.Generic;
using BulletGame.Core;
using BulletGame;
using Microsoft.Xna.Framework;

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