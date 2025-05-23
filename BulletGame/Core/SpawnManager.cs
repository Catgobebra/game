using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace BulletGame
{
    public class SpawnManager
    {
        private readonly Random _rnd;
        private readonly Rectangle _gameArea;
        private readonly List<EnemyController> _enemies;
        private readonly List<Bonus> _bonuses;
        private readonly Stack<object> _enemyWaveStack;
        private readonly PlayerController _player;
        private readonly List<AttackPattern> _attackPatterns;

        private const int SPAWN_BUFFER = 120;
        private const int MAX_SPAWN_ATTEMPTS = 100;
        private const float MIN_PLAYER_DISTANCE = 300f;
        private const float MIN_ENEMY_DISTANCE = 100f;
        private const float MIN_BONUS_DISTANCE = 100f;

        public SpawnManager(
            Random rnd,
            Rectangle gameArea,
            List<EnemyController> enemies,
            List<Bonus> bonuses,
            Stack<object> nemyWaveStack,
            PlayerController player)
        {
            _rnd = rnd;
            _gameArea = gameArea;
            _enemies = enemies;
            _bonuses = bonuses;
            _enemyWaveStack = nemyWaveStack;
            _player = player;
            _attackPatterns = new List<AttackPattern>
            {
                new AttackPattern(
                shootInterval: 0.1f,
                bulletSpeed: 500f,
                bulletsPerShot: 6,
                false,
                strategy: new SpiralStrategy(
                spiralSpeed: 2.2f,
                radiusStep: 2.0f,
                startColor: Color.Cyan,
                endColor: Color.Purple)),
                new AttackPattern(
                shootInterval: 0.1f,
                bulletSpeed: 500f,
                bulletsPerShot: 6,
                false,
                strategy: new A_StraightLineStrategy(_player, Color.Cyan)),
                new AttackPattern(
                shootInterval: 0.5f,
                bulletSpeed: 300f,
                bulletsPerShot: 6,
                false,
                strategy: new RadiusBulletStrategy(_player, Color.Cyan)),
                new AttackPattern(
                shootInterval: 0.1f,
                bulletSpeed: 300f,
                bulletsPerShot: 6,
                false,
                strategy: new AstroidStrategy(1.15f, Color.Cyan)),

            };
        }

        public Bonus CreateRandomBonus(Vector2 position)
        {
            var bonusTemplates = new[]
            {
            new {
            Pattern = new AttackPattern(
                0.2f, 900f, 12, true,
                new ZRadiusBulletStrategy(GetDirectionAimPlayer, Color.White)),
            Letter = "空",
            Name = "Пустота",
            Color = Color.White,
            Health = 1
            },
            new {
                Pattern = new AttackPattern(
                    0.2f, 900f, 12, true,
                    new QuantumCircleStrategy(Color.Red)),
                Letter = "火",
                Name = "Огонь",
                Color = Color.Red,
                Health = 2
            },
            new {
                Pattern = new AttackPattern(
                    0.2f, 900f, 1, true,
                    new FractalSquareStrategy(Color.Blue)),
                Letter = "水",
                Name = "Вода",
                Color = Color.Blue,
                Health = 3
            },
            new {
                Pattern = new AttackPattern(
                    0.2f, 900f, 1, true,
                    new PlayerExplosiveShotStrategy(Color.Brown, Color.Brown)),
                Letter = "土",
                Name = "Земля",
                Color = Color.Brown,
                Health = 2
            },
            new {
                Pattern = new AttackPattern(
                    0.2f, 900f, 1, true,
                    new CrystalFanStrategy(Color.Yellow, GetDirectionAimPlayer)),
                Letter = "風",
                Name = "Ветер",
                Color = Color.Yellow,
                Health = 4
            }
            };

            var selected = bonusTemplates[_rnd.Next(bonusTemplates.Length)];
            return new Bonus(
                selected.Pattern,
                position,
                selected.Letter,
                selected.Name,
                selected.Color,
                selected.Health 
            );
        }

        public bool SpawnEnemy(Color? color = null, Vector2? prefPosition = null, AttackPattern pattern = null)
        {
            var finalColor = color ?? GetRandomColor();
            var finalPattern = pattern ?? GetRandomPattern();
            var position = FindValidSpawnPosition(prefPosition);

            if (!position.HasValue) return false;

            var enemyModel = new EnemyModel(position.Value, finalPattern, finalColor);
            _enemies.Add(new EnemyController(enemyModel, new EnemyView(enemyModel)));
            return true;
        }

        private Color GetRandomColor()
        {
            return new Color(
                _rnd.Next(50, 255),
                _rnd.Next(50, 255),
                _rnd.Next(50, 255)
            );
        }

        private AttackPattern GetRandomPattern()
        {
            return _attackPatterns[_rnd.Next(_attackPatterns.Count)];
        }

        private Vector2? FindValidSpawnPosition(Vector2? preferredPosition = null)
        {
            if (preferredPosition.HasValue)
            {
                var position = FindNearbyValidPosition(preferredPosition.Value);
                if (position.HasValue) return position;
            }

            return GetRandomValidPosition();
        }

        private Vector2? GetRandomValidPosition()
        {
            for (int i = 0; i < MAX_SPAWN_ATTEMPTS; i++)
            {
                var pos = new Vector2(
                    _rnd.Next(_gameArea.Left + SPAWN_BUFFER, _gameArea.Right - SPAWN_BUFFER),
                    _rnd.Next(_gameArea.Top + SPAWN_BUFFER, _gameArea.Bottom - SPAWN_BUFFER)
                );

                pos.X = MathHelper.Clamp(pos.X,
                    _gameArea.Left + SPAWN_BUFFER,
                    _gameArea.Right - SPAWN_BUFFER);
                pos.Y = MathHelper.Clamp(pos.Y,
                    _gameArea.Top + SPAWN_BUFFER,
                    _gameArea.Bottom - SPAWN_BUFFER);

                if (IsPositionValid(pos)) return pos;
            }
            return null;
        }

        private bool IsPositionValid(Vector2 position)
        {
            bool inSafeArea = position.X >= _gameArea.Left + SPAWN_BUFFER &&
                     position.X <= _gameArea.Right - SPAWN_BUFFER &&
                     position.Y >= _gameArea.Top + SPAWN_BUFFER &&
                     position.Y <= _gameArea.Bottom - SPAWN_BUFFER;

            return inSafeArea &&
                   _gameArea.Contains(position.ToPoint()) &&
                   Vector2.Distance(position, _player.Model.Position) > MIN_PLAYER_DISTANCE &&
                   _enemies.All(e => Vector2.Distance(position, e.Model.Position) > MIN_ENEMY_DISTANCE) &&
                   _bonuses.All(b => Vector2.Distance(position, b.Position) > MIN_BONUS_DISTANCE);
        }

        public bool SpawnBonus(int maxAttempts = 50,
                      float minPlayerDistance = 300f,
                      float minEnemyDistance = 50f,
                      float minBonusDistance = 100f)
        {
            const int buffer = 100;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector2 position = new Vector2(
                    _rnd.Next(_gameArea.Left + buffer, _gameArea.Right - buffer),
                    _rnd.Next(_gameArea.Top + buffer, _gameArea.Bottom - buffer)
                );

                if (Vector2.Distance(position, _player.Model.Position) < minPlayerDistance)
                    continue;

                bool tooCloseToEnemy = _enemies.Any(e =>
                    Vector2.Distance(position, e.Model.Position) < minEnemyDistance);

                bool tooCloseToBonus = _bonuses.Any(b =>
                    Vector2.Distance(position, b.Position) < minBonusDistance);

                if (!tooCloseToEnemy && !tooCloseToBonus)
                {
                    _bonuses.Add(CreateRandomBonus(position));
                    return true;
                }
            }
            return false;
        }

        private Vector2? FindNearbyValidPosition(Vector2 center, int attempts = 20, float radius = 200f)
        {
            for (int i = 0; i < attempts; i++)
            {
                var angle = _rnd.NextDouble() * Math.PI * 2;
                var distance = (float)(_rnd.NextDouble() * radius);
                var position = center + new Vector2(
                    (float)(Math.Cos(angle) * distance),
                    (float)(Math.Sin(angle) * distance)
                );

                position.X = MathHelper.Clamp(position.X,
                    _gameArea.Left + SPAWN_BUFFER,
                    _gameArea.Right - SPAWN_BUFFER);
                position.Y = MathHelper.Clamp(position.Y,
                    _gameArea.Top + SPAWN_BUFFER,
                    _gameArea.Bottom - SPAWN_BUFFER);

                if (IsPositionValid(position)) return position;
            }
            return null;
        }

      
        public void InitializeWaveStack()
        {
            var wave9 = new List<Action>
            {
                () => SpawnEnemy(
                color: Color.SkyBlue,
                pattern: new AttackPattern(
                    0.1f, 250f, 1, false,
                    new StarPatternStrategy(Color.SkyBlue)
                    )
                )
            };


            /*var wave8 = new List<Action>
            {
                () => SpawnEnemy(
                    color: Color.Turquoise,
                    prefPosition: _gameArea.Center.ToVector2() + new Vector2(0, -300),
                    pattern: new AttackPattern(
                        0.2f, 400f, 16, false,
                        new SpiralStrategy(3.5f, 0.8f, Color.Cyan, Color.Navy)
                    )
                ),
                () => SpawnEnemy(
                    color: Color.Gold,
                    prefPosition: _gameArea.Center.ToVector2() + new Vector2(0, 300),
                    pattern: new AttackPattern(
                        0.2f, 400f, 16, false,
                        new SpiralStrategy(2.5f, 0.6f, Color.Gold, Color.DarkRed)
                    )
                )
            };*/

            var wave7 = new List<Action>
            {
                () => SpawnEnemy(
                    prefPosition: _gameArea.Center.ToVector2() + new Vector2(-500, -200),
                    pattern: new AttackPattern(
                        0.3f, 600f, 8, false,
                        new QuantumThreadStrategy(Color.Cyan, Color.Magenta, waveSpeed: 3f)
                    )
                ),
                () => SpawnEnemy(
                    prefPosition: _gameArea.Center.ToVector2() + new Vector2(-500, 200),
                    pattern: new AttackPattern(
                        0.3f, 600f, 8, false,
                        new QuantumThreadStrategy(Color.Yellow, Color.Red, waveSpeed: -3f)
                    )
                )
            };

            var wave6 = new List<Action>
            {
                () => SpawnEnemy(
                    color: Color.Purple,
                    prefPosition: FindValidPosition(_gameArea.Center.ToVector2() + new Vector2(-400, 0), 10, 200, 150, 100),
                    pattern: new AttackPattern(
                        0.4f,
                        500f,
                        24,
                        false,
                        new MirrorSpiralStrategy(Color.Purple, mirror: true)
                    )
                ),
                () => SpawnEnemy(
                    color: Color.Cyan,
                    prefPosition: FindValidPosition(_gameArea.Center.ToVector2() + new Vector2(400, 0), 10, 200, 150, 100),
                    pattern: new AttackPattern(
                        0.4f,
                        500f,
                        24,
                        false,
                        new MirrorSpiralStrategy(Color.Cyan, mirror: false)
                    )
                )
            };

            var wave5 = new List<Action>
            {
                () => SpawnEnemy(
                    color: Color.Orange,
                    prefPosition: _gameArea.Center.ToVector2(),
                    pattern: new AttackPattern(
                        0.3f,
                        350f,
                        36,
                        false,
                        new PulsingQuantumStrategy(Color.Orange)
                    )
                )
            };

            var wave4 = new List<Action>
            {
                () => SpawnEnemy(
                    color: Color.YellowGreen,
                    prefPosition: FindValidPosition(new Vector2(_gameArea.Left + 200, _gameArea.Top + 150), 10, 200, 150, 100),
                    pattern: _attackPatterns[1] 
                ),
                () => SpawnEnemy(
                    color: Color.YellowGreen,
                    prefPosition: FindValidPosition(new Vector2(_gameArea.Right - 200, _gameArea.Bottom - 150), 10, 200, 150, 100),
                    pattern: _attackPatterns[1]
                )
            };

            var wave3 = new List<Action>
            {
                () => {
                    SpawnEnemy(
                        color: Color.LimeGreen,
                        prefPosition: FindValidPosition(_gameArea.Center.ToVector2() + new Vector2(-400, -200), 10, 200, 150, 100),
                        pattern: _attackPatterns[3]
                    );
                },
                () => {
                     SpawnEnemy(
                        color: Color.LimeGreen,
                        prefPosition: FindValidPosition(_gameArea.Center.ToVector2() + new Vector2(400, -200), 10, 200, 150, 100),
                        pattern: _attackPatterns[3]
                    );
                }
            };

            var wave2 = new List<Action>
            {
                () => SpawnEnemy(
                    color: Color.Cyan,
                    prefPosition: _gameArea.Center.ToVector2(),
                    pattern: _attackPatterns[2] 
                ),
                () => SpawnEnemy(
                    color: Color.DarkRed,
                    prefPosition: FindValidPosition(_gameArea.Center.ToVector2() + new Vector2(0, -300), 10, 200, 150, 100),
                    pattern: _attackPatterns[1] 
                )
            };

                    var wave1 = new List<Action>
            {
                () => SpawnEnemy(
                    color: Color.Crimson,
                    prefPosition: FindValidPosition(_gameArea.Center.ToVector2() + new Vector2(-300, 0), 10, 200, 150, 100),
                    pattern: _attackPatterns[0]
                ),
                () => SpawnEnemy(
                    color: Color.Crimson,
                    prefPosition: FindValidPosition(_gameArea.Center.ToVector2() + new Vector2(300, 0), 10, 200, 150, 100),
                    pattern: _attackPatterns[0]
                )
            };
            _enemyWaveStack.Push(wave4);

            //_enemyWaveStack.Push(wave9);
            //_enemyWaveStack.Push(wave8);
            _enemyWaveStack.Push(wave7);
            _enemyWaveStack.Push(wave6);
            _enemyWaveStack.Push(wave5);
            _enemyWaveStack.Push(wave4);
            _enemyWaveStack.Push(wave3);
            _enemyWaveStack.Push(wave2);
            //_enemyWaveStack.Push(wave1);
        }

        private Vector2 GetDirectionAimPlayer()
        {
            return Vector2.Normalize(
                _player.Model.AimPosition - _player.Model.Position);
        }

        private Vector2 GetRandomPosition(int buffer)
        {
            return new Vector2(
                _rnd.Next(_gameArea.Left + buffer, _gameArea.Right - buffer),
                _rnd.Next(_gameArea.Top + buffer, _gameArea.Bottom - buffer));
        }

        private bool IsTooCloseToOtherObjects(Vector2 position)
        {
            const float minEnemyDistance = 100f;
            const float minBonusDistance = 100f;

            return _enemies.Exists(e => Vector2.Distance(position, e.Model.Position) < minEnemyDistance) ||
                   _bonuses.Exists(b => Vector2.Distance(position, b.Position) < minBonusDistance);
        }

        private Vector2 FindValidPosition(Vector2 preferredPosition, int maxAttempts,
                                        float minPlayerDist, float minEnemyDist, float minBonusDist)
        {
            if (IsPositionValid(preferredPosition))
            {
                return preferredPosition;
            }

            for (int i = 0; i < maxAttempts; i++)
            {
                var pos = new Vector2(
                    _rnd.Next(_gameArea.Left + SPAWN_BUFFER, _gameArea.Right - SPAWN_BUFFER),
                    _rnd.Next(_gameArea.Top + SPAWN_BUFFER, _gameArea.Bottom - SPAWN_BUFFER)
                );

                pos.X = MathHelper.Clamp(pos.X,
                    _gameArea.Left + SPAWN_BUFFER,
                    _gameArea.Right - SPAWN_BUFFER);
                pos.Y = MathHelper.Clamp(pos.Y,
                    _gameArea.Top + SPAWN_BUFFER,
                    _gameArea.Bottom - SPAWN_BUFFER);

                if (IsPositionValid(pos)) return pos;
            }

            return new Vector2(
                MathHelper.Clamp(_rnd.Next(_gameArea.Left, _gameArea.Right),
                    _gameArea.Left + SPAWN_BUFFER,
                    _gameArea.Right - SPAWN_BUFFER),
                MathHelper.Clamp(_rnd.Next(_gameArea.Top, _gameArea.Bottom),
                    _gameArea.Top + SPAWN_BUFFER,
                    _gameArea.Bottom - SPAWN_BUFFER)
            );
        }
    }
}