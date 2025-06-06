using BulletGame;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using BulletGame.Controllers;
using BulletGame.Controller;


public class GameRenderer
{
    private readonly SpriteBatch _spriteBatch;
    private readonly GraphicsDevice _graphicsDevice;

    public GameRenderer(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        _spriteBatch = spriteBatch;
        _graphicsDevice = graphicsDevice;
    }

    public void Draw(
        PlayerController player,
        List<EnemyController> enemies,
        List<BonusController> bonuses,
        OptimizedBulletPool bulletPool,
        SpriteFont japanSymbolFont)
    {
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        player.Draw(_graphicsDevice);
        foreach (var enemy in enemies) enemy.Draw(_graphicsDevice); 
        foreach (var bonus in bonuses) bonus._view.Draw(_spriteBatch, japanSymbolFont);
        foreach (var bullet in bulletPool.ActiveBullets) bullet.Draw(_graphicsDevice);
        PrimitiveRenderer.DrawPoint(_graphicsDevice, player.Model.AimPosition, Color.White, 6f);

        _spriteBatch.End();
    }
}