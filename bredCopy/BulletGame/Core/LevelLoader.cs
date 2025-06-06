using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using System.Text.Json.Serialization;

namespace BulletGame.Core
{
    public interface ILevelLoader
    {
        LevelData LoadLevel(int levelNumber);
    }

    public class LevelLoader : ILevelLoader
    {
        private readonly string _contentRootPath;

        public LevelLoader(string contentRootPath)
        {
            _contentRootPath = contentRootPath;
        }

        public LevelData LoadLevel(int levelNumber)
        {
            string filePath = Path.Combine(_contentRootPath, "Levels", $"level{levelNumber}.json");

            try
            {
                if (!File.Exists(filePath))
                    return CreateDefaultLevel(levelNumber);

                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<LevelData>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new ColorJsonConverter() }
                });
            }
            catch (Exception ex) when (ex is IOException or JsonException)
            {
                return CreateDefaultLevel(levelNumber);
            }
        }

        private static LevelData CreateDefaultLevel(int levelNumber) => new()
        {
            LevelNumber = levelNumber,
            Waves = new List<WaveData>
            {
                new WaveData
                {
                    Enemies = new List<EnemySpawnData>
                    {
                        new EnemySpawnData { Position = new Vector2(100, 300) },
                        new EnemySpawnData { Position = new Vector2(200, 300) }
                    }
                }
            },
            PlayerStart = new PlayerStartingData
            {
                Position = new Vector2(640, 600),
                Health = 8
            }
        };
    }

    public class ColorJsonConverter : JsonConverter<Color>
    {
        private static readonly Dictionary<string, Color> _colorMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["red"] = Color.Red,
            ["blue"] = Color.Blue,
            ["green"] = Color.Green,
            ["yellow"] = Color.Yellow,
            ["cyan"] = Color.Cyan,
            ["magenta"] = Color.Magenta,
            ["white"] = Color.White,
            ["black"] = Color.Black,
            ["purple"] = Color.Purple,
            ["orange"] = Color.Orange
        };

        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return GetColorFromString(reader.GetString()!);
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                using JsonDocument doc = JsonDocument.ParseValue(ref reader);
                var root = doc.RootElement;
                return new Color(
                    root.GetProperty("R").GetByte(),
                    root.GetProperty("G").GetByte(),
                    root.GetProperty("B").GetByte(),
                    root.GetProperty("A").GetByte()
                );
            }

            throw new JsonException("Unexpected JSON format for Color");
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(FindColorName(value) ?? SerializeAsObject(value));
        }

        private static string? FindColorName(Color color)
        {
            foreach (var kvp in _colorMap)
            {
                if (kvp.Value == color) return kvp.Key;
            }
            return null;
        }

        private static string SerializeAsObject(Color value)
        {
            return JsonSerializer.Serialize(new
            {
                R = value.R,
                G = value.G,
                B = value.B,
                A = value.A
            });
        }

        private static Color GetColorFromString(string colorName)
        {
            return _colorMap.TryGetValue(colorName, out Color color)
                ? color
                : Color.White;
        }
    }
}