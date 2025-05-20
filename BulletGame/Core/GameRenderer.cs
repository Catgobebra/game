using BulletGame.Core;
using BulletGame;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


public class GameRenderer
{
    private readonly SpriteBatch _spriteBatch;
    private readonly GraphicsDevice _graphicsDevice;

    public GameRenderer(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        _spriteBatch = spriteBatch;
        _graphicsDevice = graphicsDevice; // Сохраняем GraphicsDevice
    }

    public void Draw(
        PlayerController player,
        List<EnemyController> enemies,
        List<Bonus> bonuses,
        OptimizedBulletPool bulletPool,
        SpriteFont japanSymbolFont)
    {
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        player.Draw(_graphicsDevice);
        foreach (var enemy in enemies) enemy.Draw(_graphicsDevice); 
        foreach (var bonus in bonuses) bonus.Draw(_spriteBatch, japanSymbolFont);
        foreach (var bullet in bulletPool.ActiveBullets) bullet.Draw(_graphicsDevice);
        PrimitiveRenderer.DrawPoint(_graphicsDevice, player.Model.AimPosition, Color.White, 6f);

        _spriteBatch.End();
    }
}