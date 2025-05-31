// LevelData.cs
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace BulletGame
{
    public class LevelData
    {
        public int LevelNumber { get; set; }
        public string LevelName { get; set; }
        public Color LevelNameColor { get; set; }
        public int MaxEnemies { get; set; }
        public int MaxBonuses { get; set; }
        public float EnemySpawnInterval { get; set; }
        public float BonusSpawnCooldown { get; set; }
        public float PreBattleDelay { get; set; }
        public List<Wave> Waves { get; set; }
    }

    public class Wave
    {
        public List<EnemySpawnInfo> Enemies { get; set; }
    }

    public class EnemySpawnInfo
    {
        public string Type { get; set; }
        public Vector2 Position { get; set; }
        public int Health { get; set; }
        public float Speed { get; set; }
        public string Pattern { get; set; }
        public Color BulletColor { get; set; }
    }
}