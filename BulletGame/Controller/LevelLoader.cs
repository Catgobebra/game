using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using System.Text.Json.Serialization;
using BulletGame.Models;

namespace BulletGame.Controller
{
    public static class LevelLoader
    {
        public static LevelData LoadLevel(int levelNumber)
        {
            string filePath = $"../../../Content/Levels/level{levelNumber}.json";

            var b = Directory.GetCurrentDirectory();

            if (!File.Exists(filePath))
                return CreateDefaultLevel(levelNumber);

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<LevelData>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new ColorJsonConverter(), new DictionaryConverter() }
                });

            }
            catch
            {
                return CreateDefaultLevel(levelNumber);
            }
        }

        public class ColorJsonConverter : JsonConverter<Color>
        {
            public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    return GetColorFromString(reader.GetString());
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
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
                writer.WriteStartObject();
                writer.WriteNumber("R", value.R);
                writer.WriteNumber("G", value.G);
                writer.WriteNumber("B", value.B);
                writer.WriteNumber("A", value.A);
                writer.WriteEndObject();
            }

            private static Color GetColorFromString(string colorName)
            {
                return colorName.ToLower() switch
                {
                    "red" => Color.Red,
                    "blue" => Color.Blue,
                    "green" => Color.Green,
                    "yellow" => Color.Yellow,
                    "cyan" => Color.Cyan,
                    "magenta" => Color.Magenta,
                    "white" => Color.White,
                    "black" => Color.Black,
                    "purple" => Color.Purple,
                    "orange" => Color.Orange,
                    _ => Color.White
                };
            }
        }

        public class DictionaryConverter : JsonConverter<Dictionary<string, object>>
        {
            public override Dictionary<string, object> Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                var dictionary = new Dictionary<string, object>();

                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        return dictionary;

                    if (reader.TokenType != JsonTokenType.PropertyName)
                        throw new JsonException();

                    string propertyName = reader.GetString();
                    reader.Read();

                    switch (reader.TokenType)
                    {
                        case JsonTokenType.String:
                            dictionary.Add(propertyName, reader.GetString());
                            break;
                        case JsonTokenType.Number:
                            if (reader.TryGetInt32(out int intValue))
                                dictionary.Add(propertyName, intValue);
                            else if (reader.TryGetDouble(out double doubleValue))
                                dictionary.Add(propertyName, doubleValue);
                            else
                                dictionary.Add(propertyName, reader.GetDecimal());
                            break;
                        case JsonTokenType.True:
                        case JsonTokenType.False:
                            dictionary.Add(propertyName, reader.GetBoolean());
                            break;
                        default:
                            throw new JsonException($"Unsupported token type: {reader.TokenType}");
                    }
                }

                throw new JsonException();
            }

            public override void Write(
                Utf8JsonWriter writer,
                Dictionary<string, object> value,
                JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                foreach (var kvp in value)
                {
                    writer.WritePropertyName(kvp.Key);
                    JsonSerializer.Serialize(writer, kvp.Value, options);
                }
                writer.WriteEndObject();
            }
        }

        private static LevelData CreateDefaultLevel(int levelNumber)
        {
            return new LevelData
            {
                LevelNumber = levelNumber,
                Waves = new List<WaveData>
                {
                    new WaveData
                    {
                        Enemies = new List<EnemySpawnData>
                        {
                            new EnemySpawnData { Position = new Vector2(0, 300) },
                            new EnemySpawnData { Position = new Vector2(0, 300) }
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
    }
}