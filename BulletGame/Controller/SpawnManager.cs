using BulletGame;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Globalization;
using BulletGame.Controllers;
using BulletGame.Models;
using BulletGame.Views;

public class SpawnManager
{
    private readonly Random _rnd;
    private readonly Rectangle _gameArea;
    private readonly List<EnemyController> _enemies;
    private readonly List<BonusController> _bonuses;
    private readonly Stack<object> _enemyWaveStack;
    private readonly PlayerController _player;
    private readonly List<AttackPattern> _attackPatterns;

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
        List<EnemyController> enemies,
        List<BonusController> bonuses,
        Stack<object> enemyWaveStack,
        PlayerController player)
    {
        _rnd = rnd;
        _gameArea = gameArea;
        _enemies = enemies;
        _bonuses = bonuses;
        _enemyWaveStack = enemyWaveStack;
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
            _player.Model.AimPosition - _player.Model.Position);
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

    public bool SpawnEnemy(Color color, Vector2? position = null, AttackPattern pattern = null)
    {
        var finalColor = color;
        var finalPattern = pattern ?? GetRandomPattern();
        var finalPosition = FindValidSpawnPosition(position);

        if (!finalPosition.HasValue)
            return false;

        var enemyModel = new EnemyModel(finalPosition.Value, pattern, finalColor);
        _enemies.Add(new EnemyController(enemyModel, new EnemyView(enemyModel)));
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

    public class QuantumThreadPattern
    {
        private readonly float _shootInterval;
        private readonly float _bulletSpeed;
        private readonly int _bulletsPerShot;
        private readonly Color _color1;
        private readonly Color _color2;
        private readonly float _waveSpeed;

        public QuantumThreadPattern(float shootInterval, float bulletSpeed, int bulletsPerShot,
                                  Color color1, Color color2, float waveSpeed)
        {
            _shootInterval = shootInterval;
            _bulletSpeed = bulletSpeed;
            _bulletsPerShot = bulletsPerShot;
            _color1 = color1;
            _color2 = color2;
            _waveSpeed = waveSpeed;
        }

        public AttackPattern CreateAttackPattern()
        {
            return new AttackPattern(
                shootInterval: _shootInterval,
                bulletSpeed: _bulletSpeed,
                bulletsPerShot: _bulletsPerShot,
                playerBullet: false,
                strategy: new QuantumThreadStrategy(_color1, _color2, _waveSpeed)
            );
        }
    }

    public class MirrorSpiralPattern
    {
        private readonly float _shootInterval;
        private readonly float _bulletSpeed;
        private readonly int _bulletsPerShot;
        private readonly Color _color;
        private readonly bool _mirror;

        public MirrorSpiralPattern(float shootInterval, float bulletSpeed, int bulletsPerShot,
                                 Color color, bool mirror)
        {
            _shootInterval = shootInterval;
            _bulletSpeed = bulletSpeed;
            _bulletsPerShot = bulletsPerShot;
            _color = color;
            _mirror = mirror;
        }

        public AttackPattern CreateAttackPattern()
        {
            return new AttackPattern(
                shootInterval: _shootInterval,
                bulletSpeed: _bulletSpeed,
                bulletsPerShot: _bulletsPerShot,
                playerBullet: false,
                strategy: new MirrorSpiralStrategy(_color, _mirror)
            );
        }
    }

    public class PulsingQuantumPattern
    {
        private readonly float _shootInterval;
        private readonly float _bulletSpeed;
        private readonly int _bulletsPerShot;
        private readonly Color _baseColor;

        public PulsingQuantumPattern(float shootInterval, float bulletSpeed, int bulletsPerShot,
                                   Color baseColor)
        {
            _shootInterval = shootInterval;
            _bulletSpeed = bulletSpeed;
            _bulletsPerShot = bulletsPerShot;
            _baseColor = baseColor;
        }

        public AttackPattern CreateAttackPattern()
        {
            return new AttackPattern(
                shootInterval: _shootInterval,
                bulletSpeed: _bulletSpeed,
                bulletsPerShot: _bulletsPerShot,
                playerBullet: false,
                strategy: new PulsingQuantumStrategy(_baseColor)
            );
        }
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
                SpawnEnemy(color, position, pattern);
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

            if (Vector2.Distance(position, _player.Model.Position) < minPlayerDistance)
                continue;

            bool tooCloseToEnemy = _enemies.Any(e =>
                Vector2.Distance(position, e.Model.Position) < minEnemyDistance);

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

    private AttackPattern ParsePattern(string patternType, Dictionary<string, object> patternParams)
    {
        if (patternType == "Predefined")
        {
            if (patternParams.TryGetValue("Index", out var indexObj) &&
                int.TryParse(indexObj.ToString(), out int index))
            {
                return GetPatternByIndex(index);
            }
        }
        else if (patternType == "QuantumThread")
        {
            Color color1 = ParseColorName(patternParams["Color1"].ToString());
            Color color2 = ParseColorName(patternParams["Color2"].ToString());
            float waveSpeed = float.Parse(patternParams["WaveSpeed"].ToString(), CultureInfo.InvariantCulture);

            float shootInterval = 0.3f;
            float bulletSpeed = 600f;
            int bulletsPerShot = 8;

            if (patternParams.TryGetValue("ShootInterval", out var intervalObj))
                shootInterval = Convert.ToSingle(intervalObj);
            if (patternParams.TryGetValue("BulletSpeed", out var speedObj))
                bulletSpeed = Convert.ToSingle(speedObj);
            if (patternParams.TryGetValue("BulletsPerShot", out var bulletsObj))
                bulletsPerShot = Convert.ToInt32(bulletsObj);

            return new AttackPattern(
                shootInterval: shootInterval,
                bulletSpeed: bulletSpeed,
                bulletsPerShot: bulletsPerShot,
                playerBullet: false,
                strategy: new QuantumThreadStrategy(color1, color2, waveSpeed)
            );
        }
        else if (patternType == "MirrorSpiral")
        {
            Color color = ParseColorName(patternParams["Color"].ToString());
            bool mirror = bool.Parse(patternParams["Mirror"].ToString());

            float shootInterval = 0.4f;
            float bulletSpeed = 500f;
            int bulletsPerShot = 24;

            if (patternParams.TryGetValue("ShootInterval", out var intervalObj))
                shootInterval = Convert.ToSingle(intervalObj);
            if (patternParams.TryGetValue("BulletSpeed", out var speedObj))
                bulletSpeed = Convert.ToSingle(speedObj);
            if (patternParams.TryGetValue("BulletsPerShot", out var bulletsObj))
                bulletsPerShot = Convert.ToInt32(bulletsObj);

            return new AttackPattern(
                shootInterval: shootInterval,
                bulletSpeed: bulletSpeed,
                bulletsPerShot: bulletsPerShot,
                playerBullet: false,
                strategy: new MirrorSpiralStrategy(color, mirror)
            );
        }
        else if (patternType == "PulsingQuantum")
        {
            Color color = ParseColorName(patternParams["Color"].ToString());

            float shootInterval = 0.3f;
            float bulletSpeed = 350f;
            int bulletsPerShot = 36;

            if (patternParams.TryGetValue("ShootInterval", out var intervalObj))
                shootInterval = Convert.ToSingle(intervalObj);
            if (patternParams.TryGetValue("BulletSpeed", out var speedObj))
                bulletSpeed = Convert.ToSingle(speedObj);
            if (patternParams.TryGetValue("BulletsPerShot", out var bulletsObj))
                bulletsPerShot = Convert.ToInt32(bulletsObj);

            return new AttackPattern(
                shootInterval: shootInterval,
                bulletSpeed: bulletSpeed,
                bulletsPerShot: bulletsPerShot,
                playerBullet: false,
                strategy: new PulsingQuantumStrategy(color)
            );
        }
        else if (patternType == "QuantumVortex")
        {
            Color coreColor = ParseColorName(patternParams["CoreColor"].ToString());
            Color orbitColor = ParseColorName(patternParams["OrbitColor"].ToString());
            float rotationSpeed = float.Parse(patternParams["RotationSpeed"].ToString(), CultureInfo.InvariantCulture);

            float shootInterval = 0.5f;
            float bulletSpeed = 320f;
            int bulletsPerShot = 24;

            if (patternParams.TryGetValue("ShootInterval", out var intervalObj))
                shootInterval = Convert.ToSingle(intervalObj);
            if (patternParams.TryGetValue("BulletSpeed", out var speedObj))
                bulletSpeed = Convert.ToSingle(speedObj);
            if (patternParams.TryGetValue("BulletsPerShot", out var bulletsObj))
                bulletsPerShot = Convert.ToInt32(bulletsObj);

            return new AttackPattern(
                shootInterval: shootInterval,
                bulletSpeed: bulletSpeed,
                bulletsPerShot: bulletsPerShot,
                playerBullet: false,
                strategy: new QuantumVortexStrategy(coreColor, orbitColor, rotationSpeed)
            );
        }
        else if (patternType == "PulsingNova")
        {
            Color pulseColor = ParseColorName(patternParams["PulseColor"].ToString());
            float explosionRadius = 120f;

            if (patternParams.TryGetValue("ExplosionRadius", out var radiusObj))
                explosionRadius = Convert.ToSingle(radiusObj);

            float shootInterval = 0.4f;
            float bulletSpeed = 280f;
            int bulletsPerShot = 16;

            if (patternParams.TryGetValue("ShootInterval", out var intervalObj))
                shootInterval = Convert.ToSingle(intervalObj);
            if (patternParams.TryGetValue("BulletSpeed", out var speedObj))
                bulletSpeed = Convert.ToSingle(speedObj);
            if (patternParams.TryGetValue("BulletsPerShot", out var bulletsObj))
                bulletsPerShot = Convert.ToInt32(bulletsObj);

            return new AttackPattern(
                shootInterval: shootInterval,
                bulletSpeed: bulletSpeed,
                bulletsPerShot: bulletsPerShot,
                playerBullet: false,
                strategy: new PulsingNovaStrategy(pulseColor, explosionRadius)
            );
        }
        else if (patternType == "ChaosSphere")
        {
            int layers = 3;
            int projectileCount = 24;

            if (patternParams.TryGetValue("Layers", out var layersObj))
                layers = Convert.ToInt32(layersObj);
            if (patternParams.TryGetValue("ProjectileCount", out var countObj))
                projectileCount = Convert.ToInt32(countObj);

            float shootInterval = 0.8f;
            float bulletSpeed = 250f;
            int bulletsPerShot = projectileCount;

            if (patternParams.TryGetValue("ShootInterval", out var intervalObj))
                shootInterval = Convert.ToSingle(intervalObj);
            if (patternParams.TryGetValue("BulletSpeed", out var speedObj))
                bulletSpeed = Convert.ToSingle(speedObj);
            if (patternParams.TryGetValue("BulletsPerShot", out var bulletsObj))
                bulletsPerShot = Convert.ToInt32(bulletsObj);

            return new AttackPattern(
                shootInterval: shootInterval,
                bulletSpeed: bulletSpeed,
                bulletsPerShot: bulletsPerShot,
                playerBullet: false,
                strategy: new ChaosSphereStrategy(Color.White, layers, projectileCount)
            );
        }
        return GetRandomPattern();
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
            _enemyWaveStack.Push(levelData.Waves[i]);
        }
    }
}