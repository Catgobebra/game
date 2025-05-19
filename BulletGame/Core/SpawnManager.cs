using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BulletGame
{
    public class SpawnManager
    {
        private readonly Random _rnd;
        private readonly Rectangle _gameArea;
        private readonly List<EnemyController> _enemies;
        private readonly List<Bonus> _bonuses;
        private readonly Stack<object> _enemyWaveStack;
        private readonly List<AttackPattern> _attackPatterns;
        private readonly PlayerController _player;

        public SpawnManager(
            Random rnd,
            Rectangle gameArea,
            List<EnemyController> enemies,
            List<Bonus> bonuses,
            Stack<object> nemyWaveStack,
            List<AttackPattern> attackPatterns,
            PlayerController player)
        {
            _rnd = rnd;
            _gameArea = gameArea;
            _enemies = enemies;
            _bonuses = bonuses;
            _enemyWaveStack = nemyWaveStack;
            _attackPatterns = attackPatterns;
            _player = player;
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

        public bool SpawnEnemy(Color? color = null, Vector2? position = null, AttackPattern pattern = null)
        {
            const int buffer = 100;
            const int maxAttempts = 50;
            const float minPlayerDistance = 300f;

            var finalPattern = pattern ?? _attackPatterns[_rnd.Next(_attackPatterns.Count)];
            var finalColor = color ?? Color.Crimson;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var pos = position ?? GetRandomPosition(buffer);

                if (IsValidPosition(pos, minPlayerDistance))
                {
                    var enemyModel = new EnemyModel(pos, finalPattern, finalColor);
                    _enemies.Add(new EnemyController(enemyModel, new EnemyView(enemyModel)));
                    return true;
                }
            }
            return false;
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


        public void InitializeWaveStack()
        {
            var wave7 = new List<Action>
            {
                () => SpawnEnemy(
                    position: _gameArea.Center.ToVector2() + new Vector2(-500, -200),
                    pattern: new AttackPattern(
                        0.3f, 600f, 8, false,
                        new QuantumThreadStrategy(Color.Cyan, Color.Magenta, waveSpeed: 3f)
                    )
                ),
                () => SpawnEnemy(
                    position: _gameArea.Center.ToVector2() + new Vector2(-500, 200),
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
                    position: FindValidPosition(_gameArea.Center.ToVector2() + new Vector2(-400, 0), 10, 200, 150, 100),
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
                    position: FindValidPosition(_gameArea.Center.ToVector2() + new Vector2(400, 0), 10, 200, 150, 100),
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
                    position: _gameArea.Center.ToVector2(),
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
                    position: FindValidPosition(new Vector2(_gameArea.Left + 200, _gameArea.Top + 150), 10, 200, 150, 100),
                    pattern: _attackPatterns[1] 
                ),
                () => SpawnEnemy(
                    color: Color.YellowGreen,
                    position: FindValidPosition(new Vector2(_gameArea.Right - 200, _gameArea.Bottom - 150), 10, 200, 150, 100),
                    pattern: _attackPatterns[1]
                )
            };

            var wave3 = new List<Action>
            {
                () => {
                    SpawnEnemy(
                        color: Color.LimeGreen,
                        position: FindValidPosition(_gameArea.Center.ToVector2() + new Vector2(-400, -200), 10, 200, 150, 100),
                        pattern: _attackPatterns[3]
                    );
                },
                () => {
                     SpawnEnemy(
                        color: Color.LimeGreen,
                        position: FindValidPosition(_gameArea.Center.ToVector2() + new Vector2(400, -200), 10, 200, 150, 100),
                        pattern: _attackPatterns[3]
                    );
                }
            };

            var wave2 = new List<Action>
            {
                () => SpawnEnemy(
                    color: Color.Cyan,
                    position: _gameArea.Center.ToVector2(),
                    pattern: _attackPatterns[2] 
                ),
                () => SpawnEnemy(
                    color: Color.DarkRed,
                    position: FindValidPosition(_gameArea.Center.ToVector2() + new Vector2(0, -300), 10, 200, 150, 100),
                    pattern: _attackPatterns[1] 
                )
            };

                    var wave1 = new List<Action>
            {
                () => SpawnEnemy(
                    color: Color.Crimson,
                    position: FindValidPosition(_gameArea.Center.ToVector2() + new Vector2(-300, 0), 10, 200, 150, 100),
                    pattern: _attackPatterns[0]
                ),
                () => SpawnEnemy(
                    color: Color.Crimson,
                    position: FindValidPosition(_gameArea.Center.ToVector2() + new Vector2(300, 0), 10, 200, 150, 100),
                    pattern: _attackPatterns[0]
                )
            };

            _enemyWaveStack.Push(wave7);
            _enemyWaveStack.Push(wave6);
            _enemyWaveStack.Push(wave5);
            _enemyWaveStack.Push(wave4);
            _enemyWaveStack.Push(wave3);
            _enemyWaveStack.Push(wave2);
            _enemyWaveStack.Push(wave1);
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

        private bool IsValidPosition(Vector2 position, float minPlayerDist)
        {
            return Vector2.Distance(position, _player.Model.Position) >= minPlayerDist &&
                   !IsTooCloseToOtherObjects(position);
        }

        private bool IsTooCloseToOtherObjects(Vector2 position)
        {
            const float minEnemyDistance = 150f;
            const float minBonusDistance = 100f;

            return _enemies.Exists(e => Vector2.Distance(position, e.Model.Position) < minEnemyDistance) ||
                   _bonuses.Exists(b => Vector2.Distance(position, b.Position) < minBonusDistance);
        }

        private Vector2 FindValidPosition(Vector2 preferredPosition, int maxAttempts,
                                        float minPlayerDist, float minEnemyDist, float minBonusDist)
        {
            int buffer = 100;
            if (IsPositionValid(preferredPosition, minPlayerDist, minEnemyDist, minBonusDist))
            {
                return preferredPosition;
            }

            for (int i = 0; i < maxAttempts; i++)
            {
                var pos = new Vector2(
                    _rnd.Next(_gameArea.Left + buffer, _gameArea.Right - buffer),
                    _rnd.Next(_gameArea.Top + buffer, _gameArea.Bottom - buffer)
                );

                if (IsPositionValid(pos, minPlayerDist, minEnemyDist, minBonusDist))
                {
                    return pos;
                }
            }

            return new Vector2(
                _rnd.Next(_gameArea.Left + 100, _gameArea.Right - 100),
                _rnd.Next(_gameArea.Top + 100, _gameArea.Bottom - 100)
            );
        }

        private bool IsPositionValid(Vector2 position, float minPlayerDist, float minEnemyDist, float minBonusDist)
        {
            if (Vector2.Distance(position, _player.Model.Position) < minPlayerDist)
                return false;

            if (_enemies.Any(e => Vector2.Distance(position, e.Model.Position) < minEnemyDist))
                return false;

            if (_bonuses.Any(b => Vector2.Distance(position, b.Position) < minBonusDist))
                return false;

            return true;
        }
    }
}