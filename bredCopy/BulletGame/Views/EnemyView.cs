using BulletGame;
using Microsoft.Xna.Framework;
public class EnemyView
{
    private const int CircleRadius = 30;
    private const int Segments = 32;

    private readonly EnemyModel _model;
    private readonly IPrimitiveRenderer _renderer;

    public EnemyView(EnemyModel model, IPrimitiveRenderer renderer)
    {
        _model = model;
        _renderer = renderer;
    }

    public void Draw()
    {
        int scaledRadius = (int)(CircleRadius * _model.CurrentScale);

        _renderer.DrawCircle(
            _model.Position,
            scaledRadius,
            Segments,
            Color.Lerp(_model.Color, Color.Red, 1 - _model.CurrentScale / _model.MaxScale)
        );
    }
}