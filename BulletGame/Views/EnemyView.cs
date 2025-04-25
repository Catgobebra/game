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
            PrimitiveRenderer.DrawCircle(
                device,
                _model.Position,
                CircleRadius,
                Segments,
                _model.Color
            );
        }
    }
}