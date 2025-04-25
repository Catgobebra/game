using Microsoft.Xna.Framework.Graphics;

namespace BulletGame
{
    public class PlayerView
    {
        private readonly PlayerModel _model;

        public PlayerView(PlayerModel model)
        {
            _model = model;
        }

        public void Draw(GraphicsDevice device)
        {
            PrimitiveRenderer.DrawTriangle(
                device,
                _model.Position,
                _model.Direction,
                _model.Size,
                _model.Color
            );
        }
    }
}