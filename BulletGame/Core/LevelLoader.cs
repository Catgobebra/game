// LevelLoader.cs
using System.IO;
using Newtonsoft.Json;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;

namespace BulletGame
{
    public static class LevelLoader
    {
        public static LevelData LoadLevel(int levelNumber)
        {
            string path = $"Content/Levels/level_{levelNumber}.json";

            if (!File.Exists(path))
            {
                return CreateDefaultLevel(levelNumber);
            }

            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<LevelData>(json, new JsonSerializerSettings
            {
                Converters = { new Vector2Converter() }
            });
        }
        private static LevelData CreateDefaultLevel(int levelNumber)
        {
            return new LevelData
            {
                LevelNumber = levelNumber,
                LevelName = "Пустота",
                LevelNameColor = Color.White,
                MaxEnemies = 3,
                MaxBonuses = 2,
                EnemySpawnInterval = 2f,
                BonusSpawnCooldown = 10f,
                PreBattleDelay = 100f,
                Waves = new List<Wave>
                {
                    new Wave
                    {
                        Enemies = new List<EnemySpawnInfo>
                        {
                            new EnemySpawnInfo
                            {
                                Type = "Basic",
                                Position = new Vector2(500, 200),
                                Health = 3,
                                Speed = 150f,
                                Pattern = "Single",
                                BulletColor = Color.Red
                            }
                        }
                    }
                }
            };
        }
    }
    public class Vector2Converter : JsonConverter<Vector2>
    {
        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = Newtonsoft.Json.Linq.JObject.Load(reader);
            return new Vector2(obj["X"].Value<float>(), obj["Y"].Value<float>());
        }

        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("X");
            writer.WriteValue(value.X);
            writer.WritePropertyName("Y");
            writer.WriteValue(value.Y);
            writer.WriteEndObject();
        }
    }
}