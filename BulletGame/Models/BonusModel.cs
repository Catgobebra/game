using Microsoft.Xna.Framework;
using System;

namespace BulletGame.Models
{
    public class BonusModel
    {
        public AttackPattern Pattern { get; }
        public string Name { get; }
        public Color Color { get; }
        public Vector2 Position { get; }
        public int Health { get; }
        public string Symbol { get; }
        public float TimeLeft { get; set; }
        public float Lifetime { get; }

        public BonusModel(
            AttackPattern pattern,
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
            Lifetime = lifetime;
            TimeLeft = lifetime;
        }
    }
}