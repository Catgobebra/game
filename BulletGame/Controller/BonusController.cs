using BulletGame.Models;
using BulletGame.Views;
using System.Windows.Forms;

namespace BulletGame.Controllers
{
    public class BonusController
    {
        public readonly BonusModel _model;
        public readonly BonusView _view;

        public BonusController(BonusModel model, BonusView view)
        {
            _model = model;
            _view = view;
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