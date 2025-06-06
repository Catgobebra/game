using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

public interface IDrawable
{
    void Draw(SpriteBatch spriteBatch);
}

public class GameRenderer
{
    private readonly SpriteBatch _spriteBatch;
    private readonly GraphicsDevice _graphicsDevice;

    public GameRenderer(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        _spriteBatch = spriteBatch;
        _graphicsDevice = graphicsDevice;
    }

    public void Draw(IEnumerable<IDrawable> drawables)
    {
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        foreach (var drawable in drawables)
        {
            drawable.Draw(_spriteBatch);
        }

        _spriteBatch.End();
    }
}