using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using BulletGame.Models;
using System;

namespace BulletGame.Views
{
    public class BonusView
    {
        private readonly BonusModel _model;

        public BonusView(BonusModel model)
        {
            _model = model;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            spriteBatch.DrawString(font, _model.Symbol, _model.Position, _model.Color);
        }

        public List<Vector2> GetVertices()
        {
            List<Vector2> vertices = new List<Vector2>();
            float angleStep = MathHelper.TwoPi / 8;

            for (int i = 0; i < 8; i++)
            {
                float angle = angleStep * i;
                Vector2 offset = new Vector2(
                    30 * (float)Math.Cos(angle),
                    30 * (float)Math.Sin(angle)
                );
                vertices.Add(_model.Position + offset);
            }
            return vertices;
        }
    }
}