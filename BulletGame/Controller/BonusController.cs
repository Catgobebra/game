using BulletGame.Models;

namespace BulletGame.Controllers
{
    public class BonusController
    {
        private readonly BonusModel _model;

        public BonusController(BonusModel model)
        {
            _model = model;
        }

        public void Update(float deltaTime)
        {
            _model.TimeLeft -= deltaTime;
        }

        public void ApplyEffect(PlayerModel player)
        {
            if (_model.Pattern != null)
            {
                player.AdditionalAttack = _model.Pattern;
                player.Color = _model.Color;
                player.BonusHealth = _model.Health;
            }
            player.Health += _model.Health;
        }
    }
}