using BulletGame;

public class PlayerView
{
    private readonly PlayerModel _model;
    private readonly IPrimitiveRenderer _renderer;

    public PlayerView(PlayerModel model, IPrimitiveRenderer renderer)
    {
        _model = model;
        _renderer = renderer;
    }

    public void Draw()
    {
        _renderer.DrawTriangle(
            _model.Position,
            _model.Direction,
            _model.Size,
            _model.Color
        );
    }
}