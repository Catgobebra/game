namespace BulletGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public interface IPlayer
{
    void TakeDamage(int damage);
    ICollider Collider { get; }
    Vector2 AimPosition { get; set; }

    void StartMainAttack();
    void StopMainAttack();
    void PerformSpecialAttack();

    Vector2 Position { get; }

    Rectangle GameArea { get; set; }
    PlayerModel Model { get; }

    void Update(GameTime gameTime);
    void SetGameArea(Rectangle gameArea);
    void SetPosition(Vector2 position);

    void SetViewport(Viewport viewport);
    Viewport Viewport { get; set; }
    int Health { get; set; }

    AttackPattern AdditionalAttack { get; set; }
    Color Color { get; set; }
    int BonusHealth { get; set; }
}