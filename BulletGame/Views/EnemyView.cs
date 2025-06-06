using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletGame
{
    public class EnemyView
    {
        private const int CircleRadius = 30;
        private const int Segments = 32;

        private readonly EnemyModel _model;

        public EnemyView(EnemyModel model)
        {
            _model = model;
        }

        public void Draw(GraphicsDevice device)
        {
            int scaledRadius = (int)(CircleRadius * _model.CurrentScale);

            PrimitiveRenderer.DrawCircle(
                device,
                _model.Position,
                scaledRadius,
                Segments,
                Color.Lerp(_model.Color, Color.Red, 1 - _model.CurrentScale / _model.MaxScale)
            );
        }

    }
}