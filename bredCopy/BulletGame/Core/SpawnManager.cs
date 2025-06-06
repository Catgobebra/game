using BulletGame.Core;
using BulletGame;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using System.Linq;
using BulletGame.Controllers;
using BulletGame.Models;
using BulletGame.Views;

public class SpawnManager
{
    private readonly Random _rnd;
    private readonly Rectangle _gameArea;
    private readonly List<IEnemy> _enemies;
    private readonly List<BonusController> _bonuses;
    private readonly Stack<IWaveItem> _enemyWaveStack;
    private readonly IPlayer _player;
    private readonly List<AttackPattern> _attackPatterns;
    private readonly IPrimitiveRenderer _renderer;


    private const int SPAWN_BUFFER = 120;
    private const int MAX_SPAWN_ATTEMPTS = 100;
    private const float MIN_PLAYER_DISTANCE = 300f;
    private const float MIN_ENEMY_DISTANCE = 100f;
    private const float MIN_BONUS_DISTANCE = 100f;

    private const int POSITION_ATTEMPTS = 20;
    private const float POSITION_RADIUS = 200f;


    public SpawnManager(
        Random rnd,
        Rectangle gameArea,
        List<IEnemy> enemies,
        List<BonusController> bonuses,
        Stack<IWaveItem> enemyWaveStack,
        IPlayer player,
        IPrimitiveRenderer renderer)
    {
        _rnd = rnd;
        _gameArea = gameArea;
        _enemies = enemies;
        _bonuses = bonuses;
        _enemyWaveStack = enemyWaveStack;
        _player = player;
        _renderer = renderer;

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
                    endColor: Color.Purple)
            ),
            new AttackPattern(
                shootInterval: 0.1f,
                bulletSpeed: 500f,
                bulletsPerShot: 6,
                false,
                strategy: new A_StraightLineStrategy(_player, Color.Cyan)
            ),
            new AttackPattern(
                shootInterval: 0.5f,
                bulletSpeed: 300f,
                bulletsPerShot: 6,
                false,
                strategy: new RadiusBulletStrategy(_player, Color.Cyan)
            ),
            new AttackPattern(
                shootInterval: 0.1f,
                bulletSpeed: 300f,
                bulletsPerShot: 6,
                false,
                strategy: new AstroidStrategy(1.15f, Color.Cyan))
        };
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

            if (IsPositionValid(pos))
                return pos;
        }
        return null;
    }

    private Vector2 GetDirectionAimPlayer()
    {
        return Vector2.Normalize(
            _player.AimPosition - _player.Position);
    }

    private bool IsPositionValid(Vector2 position)
    {
        bool inSafeArea = position.X >= _gameArea.Left + SPAWN_BUFFER &&
                     position.X <= _gameArea.Right - SPAWN_BUFFER &&
                     position.Y >= _gameArea.Top + SPAWN_BUFFER &&
                     position.Y <= _gameArea.Bottom - SPAWN_BUFFER;

        return inSafeArea &&
               _gameArea.Contains(position.ToPoint()) &&
               Vector2.Distance(position, _player.Position) > MIN_PLAYER_DISTANCE &&
               _enemies.All(e => Vector2.Distance(position, e.Position) > MIN_ENEMY_DISTANCE) &&
               _bonuses.All(b => Vector2.Distance(position, b._model.Position) > MIN_BONUS_DISTANCE);
    }

    private Color GetRandomColor()
    {
        return new Color(
            _rnd.Next(50, 255),
            _rnd.Next(50, 255),
            _rnd.Next(50, 255)
        );
    }

    public bool SpawnEnemy(Color color, Vector2? position = null, IPattern pattern = null)
    {
        var finalColor = color;
        var finalPattern = pattern ?? GetRandomPattern();
        var finalPosition = FindValidSpawnPosition(position);

        if (!finalPosition.HasValue)
            return false;

        var enemyModel = new EnemyModel(finalPosition.Value, pattern, finalColor);
        _enemies.Add(new EnemyController(enemyModel, new EnemyView(enemyModel, _renderer)));
        return true;
    }

    private Vector2? FindValidSpawnPosition(Vector2? preferredPosition = null)
    {
        if (preferredPosition.HasValue)
        {
            var nearbyPosition = FindNearbyValidPosition(preferredPosition.Value);
            if (nearbyPosition.HasValue) return nearbyPosition;
        }

        return GetRandomValidPosition();
    }

    private Vector2? FindNearbyValidPosition(Vector2 center, int attempts = POSITION_ATTEMPTS, float radius = POSITION_RADIUS)
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
   

    private AttackPattern GetRandomPattern()
    {
        return _attackPatterns[_rnd.Next(_attackPatterns.Count)];
    }

    public void ProcessWaveData(WaveData wave)
    {
        foreach (var enemyInfo in wave.Enemies)
        {
            AttackPattern pattern = ParsePattern(
                enemyInfo.PatternType,
                enemyInfo.PatternParams
            );

            Color color = (enemyInfo.Color ?? GetRandomColor());

            Vector2? position = GetRandomValidPosition();

            if (position.HasValue)
            {
                SpawnEnemy(color, position.Value, pattern);
            }
        }
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

            if (Vector2.Distance(position, _player.Position) < minPlayerDistance)
                continue;

            bool tooCloseToEnemy = _enemies.Any(e =>
                Vector2.Distance(position, e.Position) < minEnemyDistance);

            bool tooCloseToBonus = _bonuses.Any(b =>
                Vector2.Distance(position, b._model.Position) < minBonusDistance);

            if (!tooCloseToEnemy && !tooCloseToBonus)
            {
                _bonuses.Add(CreateRandomBonus(position));
                return true;
            }
        }
        return false;
    }

    public BonusController CreateRandomBonus(Vector2 position)
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
        var modelBonus = new BonusModel(
            selected.Pattern,
            position,
            selected.Letter,
            selected.Name,
            selected.Color,
            selected.Health
        );
        return new BonusController(modelBonus, new BonusView(modelBonus));
    }

    private AttackPattern ParsePattern(string patternType, IPattern patternParams)
    {
        switch (patternType)
        {
            case "Predefined":
                if (patternParams is PredefinedPattern predefined)
                {
                    if (predefined.Index.HasValue)
                    {
                        return GetPatternByIndex(predefined.Index.Value);
                    }
                }
                break;

            case "QuantumThread":
                if (patternParams is QuantumThreadPattern quantumThread)
                {
                    Color color1 = ParseColorName(quantumThread.Color1);
                    Color color2 = ParseColorName(quantumThread.Color2);
                    float waveSpeed = quantumThread.WaveSpeed;

                    return new AttackPattern(
                        shootInterval: quantumThread.ShootInterval,
                        bulletSpeed: quantumThread.BulletSpeed,
                        bulletsPerShot: quantumThread.BulletsPerShot,
                        playerBullet: false,
                        strategy: new QuantumThreadStrategy(color1, color2, waveSpeed)
                    );
                }
                break;

            case "MirrorSpiral":
                if (patternParams is MirrorSpiralPattern mirrorSpiral)
                {
                    Color color = ParseColorName(mirrorSpiral.Color);
                    bool mirror = mirrorSpiral.Mirror;

                    return new AttackPattern(
                        shootInterval: mirrorSpiral.ShootInterval,
                        bulletSpeed: mirrorSpiral.BulletSpeed,
                        bulletsPerShot: mirrorSpiral.BulletsPerShot,
                        playerBullet: false,
                        strategy: new MirrorSpiralStrategy(color, mirror)
                    );
                }
                break;

            case "PulsingQuantum":
                if (patternParams is PulsingQuantumPattern pulsingQuantum)
                {
                    Color color = ParseColorName(pulsingQuantum.Color);

                    return new AttackPattern(
                        shootInterval: pulsingQuantum.ShootInterval,
                        bulletSpeed: pulsingQuantum.BulletSpeed,
                        bulletsPerShot: pulsingQuantum.BulletsPerShot,
                        playerBullet: false,
                        strategy: new PulsingQuantumStrategy(color)
                    );
                }
                break;

            case "QuantumVortex":
                if (patternParams is QuantumVortexPattern quantumVortex)
                {
                    Color coreColor = ParseColorName(quantumVortex.CoreColor);
                    Color orbitColor = ParseColorName(quantumVortex.OrbitColor);
                    float rotationSpeed = quantumVortex.RotationSpeed;

                    return new AttackPattern(
                        shootInterval: quantumVortex.ShootInterval,
                        bulletSpeed: quantumVortex.BulletSpeed,
                        bulletsPerShot: quantumVortex.BulletsPerShot,
                        playerBullet: false,
                        strategy: new QuantumVortexStrategy(coreColor, orbitColor, rotationSpeed)
                    );
                }
                break;

            case "PulsingNova":
                if (patternParams is PulsingNovaPattern pulsingNova)
                {
                    Color pulseColor = ParseColorName(pulsingNova.PulseColor);
                    float explosionRadius = pulsingNova.ExplosionRadius;

                    return new AttackPattern(
                        shootInterval: pulsingNova.ShootInterval,
                        bulletSpeed: pulsingNova.BulletSpeed,
                        bulletsPerShot: pulsingNova.BulletsPerShot,
                        playerBullet: false,
                        strategy: new PulsingNovaStrategy(pulseColor, explosionRadius)
                    );
                }
                break;

            case "ChaosSphere":
                if (patternParams is ChaosSpherePattern chaosSphere)
                {
                    int layers = chaosSphere.Layers;
                    int projectileCount = chaosSphere.ProjectileCount;
                    int bulletsPerShot = chaosSphere.BulletsPerShot;

                    return new AttackPattern(
                        shootInterval: chaosSphere.ShootInterval,
                        bulletSpeed: chaosSphere.BulletSpeed,
                        bulletsPerShot: bulletsPerShot,
                        playerBullet: false,
                        strategy: new ChaosSphereStrategy(Color.White, layers, projectileCount)
                    );
                }
                break;
        }

        return GetRandomPattern();
    }

    public class QuantumThreadPattern : IPattern
    {
        public string PatternType => "QuantumThread";
        public string Color1 { get; set; } = "Cyan";
        public string Color2 { get; set; } = "Magenta";
        public float WaveSpeed { get; set; } = 1.0f;
        public float ShootInterval { get; set; } = 0.3f;
        public float BulletSpeed { get; set; } = 600f;
        public int BulletsPerShot { get; set; } = 8;
    }

    public class MirrorSpiralPattern : IPattern
    {
        public string PatternType => "MirrorSpiral";
        public string Color { get; set; } = "Yellow";
        public bool Mirror { get; set; } = true;
        public float ShootInterval { get; set; } = 0.4f;
        public float BulletSpeed { get; set; } = 500f;
        public int BulletsPerShot { get; set; } = 24;
    }

    public class PulsingQuantumPattern : IPattern
    {
        public string PatternType => "PulsingQuantum";
        public string Color { get; set; } = "Red";
        public float ShootInterval { get; set; } = 0.3f;
        public float BulletSpeed { get; set; } = 350f;
        public int BulletsPerShot { get; set; } = 36;
    }

    public class QuantumVortexPattern : IPattern
    {
        public string PatternType => "QuantumVortex";
        public string CoreColor { get; set; } = "Purple";
        public string OrbitColor { get; set; } = "Orange";
        public float RotationSpeed { get; set; } = 1.0f;
        public float ShootInterval { get; set; } = 0.5f;
        public float BulletSpeed { get; set; } = 320f;
        public int BulletsPerShot { get; set; } = 24;
    }

    public class PulsingNovaPattern : IPattern
    {
        public string PatternType => "PulsingNova";
        public string PulseColor { get; set; } = "Orange";
        public float ExplosionRadius { get; set; } = 120f;
        public float ShootInterval { get; set; } = 0.4f;
        public float BulletSpeed { get; set; } = 280f;
        public int BulletsPerShot { get; set; } = 16;
    }

    public class ChaosSpherePattern : IPattern
    {
        public string PatternType => "ChaosSphere";
        public int Layers { get; set; } = 3;
        public int ProjectileCount { get; set; } = 24;
        public float ShootInterval { get; set; } = 0.8f;
        public float BulletSpeed { get; set; } = 250f;
        public int BulletsPerShot { get; set; } = 24;  // Используется ProjectileCount по умолчанию
    }

    public class PredefinedPattern : IPattern
    {
        public string PatternType => "Predefined";
        public int? Index { get; set; }  // Для поиска по индексу
        public string MovementPath { get; set; } = "Linear";
        public float Speed { get; set; } = 1.0f;
    }

    private Color ParseColorName(string colorName)
    {
        return colorName.ToLower() switch
        {
            "cyan" => new Color(0, 255, 255),
            "magenta" => new Color(255, 0, 255),
            "yellow" => new Color(255, 255, 0),
            "red" => new Color(255, 0, 0),
            "purple" => new Color(128, 0, 128),
            "orange" => new Color(255, 165, 0),
            _ => Color.White
        };
    }

    private AttackPattern GetPatternByIndex(int index)
    {
        if (index >= 0 && index < _attackPatterns.Count)
        {
            return _attackPatterns[index];
        }
        return GetRandomPattern();
    }

    public void InitializeWaveStack(LevelData levelData)
    {
        _enemyWaveStack.Clear();

        for (int i = levelData.Waves.Count - 1; i >= 0; i--)
        {
            var wave = levelData.Waves[i];
            var waveItems = new List<IWaveItem>();

            foreach (var enemy in wave.Enemies)
            {
                var enemyCopy = enemy;
                waveItems.Add(new ActionWaveItem(() => SpawnEnemy((Color)enemyCopy.Color, enemyCopy.Position, enemyCopy.Pattern)));
            }

            _enemyWaveStack.Push(new GroupWaveItem(waveItems));
        }
    }
}