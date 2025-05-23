using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace BulletGame
{
    public class Bonus
    {
        public readonly AttackPattern Pattern;
        public string Name { get; }
        public readonly Color Color;
        public readonly Vector2 Position;
        public readonly int Health;
        public readonly string Symbol;
        public float TimeLeft { get; private set; }

        public Bonus(AttackPattern pattern,
                    Vector2 position,
                    string symbol,
                    string name,
                    Color color,
                    int health,
                    float lifetime = 10f)
        {
            Pattern = pattern;
            Position = position;
            Symbol = symbol;
            Name = name;
            Color = color;
            Health = health;
            TimeLeft = lifetime;
        }

        public void Update(float deltaTime)
        {
            TimeLeft -= deltaTime;
        }

        public void ApplyEffect(PlayerModel player)
        {
            if (Pattern != null)
            {
                player.AdditionalAttack = Pattern;
                player.Color = Color;
                player.BonusHealth = Health;
            }
            player.Health += Health;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            spriteBatch.DrawString(font, Symbol, Position, Color);
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
                vertices.Add(Position + offset);
            }
            return vertices;
        }
    }
}