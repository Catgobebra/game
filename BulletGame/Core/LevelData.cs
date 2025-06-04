using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace BulletGame.Core
{
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

    public class WaveData
    {
        public float PreWaveDelay { get; set; } = 2.0f;
        public string WaveMessage { get; set; } = "";
        public Color MessageColor { get; set; } = Color.White;

        public List<EnemySpawnData> Enemies { get; set; } = new List<EnemySpawnData>();
    }

    public class EnemySpawnData
    {
        public Vector2? Position { get; set; }
        public Color? Color { get; set; }
        public string PatternType { get; set; } = "Predefined";
        public Dictionary<string, object> PatternParams { get; set; } = new Dictionary<string, object>();
    }
}