using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace BulletGame.Core
{
    // Базовый класс для общих свойств сообщений
    public abstract class MessageContainer
    {
        public string Message { get; set; } = "";
        public Color MessageColor { get; set; } = Color.White;
    }

    public class LevelData
    {
        public int LevelNumber { get; set; } = 1;
        public string LevelName { get; set; } = "Пустота";
        public Color LevelNameColor { get; set; } = Color.White;

        public int MaxBonusCount { get; set; } = 1;
        public float BonusLifetime { get; set; } = 12f;
        public float BonusSpawnCooldown { get; set; } = 8f;

        public List<WaveData> Waves { get; set; } = new List<WaveData>();
        public PlayerStartingData PlayerStart { get; set; } = new PlayerStartingData();
    }

    public class PlayerStartingData
    {
        public Vector2 Position { get; set; } = new Vector2(640, 600);
        public int Health { get; set; } = 8;
    }

    public class WaveData : MessageContainer
    {
        public float PreWaveDelay { get; set; } = 2.0f;
        public List<EnemySpawnData> Enemies { get; set; } = new List<EnemySpawnData>();
    }

    // Интерфейс для паттернов поведения
    public interface IPattern
    {
        string PatternType { get; }
    }

    // Реализации паттернов
    public class PredefinedPattern : IPattern
    {
        public string PatternType => "Predefined";
        public string MovementPath { get; set; } = "Linear";
        public float Speed { get; set; } = 1.0f;
    }

    public class RandomPattern : IPattern
    {
        public string PatternType => "Random";
        public float MinSpeed { get; set; } = 0.5f;
        public float MaxSpeed { get; set; } = 2.0f;
        public float DirectionVariance { get; set; } = 0.3f;
    }

    public class EnemySpawnData
    {
        public Vector2? Position { get; set; }
        public Color? Color { get; set; }
        public IPattern Pattern { get; set; } = new PredefinedPattern();
        public string PatternType => Pattern.PatternType;
        public IPattern PatternParams => Pattern;

    }
}