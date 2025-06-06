using BulletGame.Models;
using BulletGame.Views;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using BulletGame;
using BulletGame.Controllers;
using BulletGame.Controller;
using System.Linq;
using static BulletGame.Game1;


public class GameplayManager
{
    public float _spawnTimer; //

    private const float PreBattleDelay = 5.0f;
    private const float EnemySpawnInterval = 2.0f;
    private const float BonusSpawnCooldown = 10.0f;
    private float _bonusSpawnTimer = 0f;
    private int CountBonusNow = 0;
    private int MaxCountBonus = 1;
    private float _enemySpawnTimer = 0f;
    private float preBattleTimer = 0f;

    private bool _isWaveInProgress = false;
    private bool battleStarted = false;
    private string Name = "Пустота";
    private Color NameColor = Color.White;
    public bool _canSpawnBonus = false;

    private GraphicsDevice _graphicsDevice;
    private GameRenderer _gameRenderer;

    private PlayerController _player;
    private List<EnemyController> _enemies;
    private List<BonusController> _bonuses;
    private Stack<object> _enemyWaveStack;
    private SpawnManager _spawnManager;
    private WaveProcessor _waveProcessor;
    private OptimizedBulletPool _bulletPool;
    private BulletManager _bulletManager;
    private UIManager _uiManager;
    private Rectangle _gameArea;

    private int Lvl;
    private LevelData _currentLevelData;
    private BonusController _defaultBonus;

    public bool IsPlayerAlive => _player.Model.Health > 0;

    public GameplayManager(
        PlayerController player,
        List<EnemyController> enemies,
        List<BonusController> bonuses,
        Stack<object> enemyWaveStack,
        SpawnManager spawnManager,
        WaveProcessor waveProcessor,
        OptimizedBulletPool bulletPool,
        BulletManager bulletManager,
        UIManager uiManager,
        Rectangle gameArea,
        GraphicsDevice graphicsDevice)
    {
        _player = player;
        _enemies = enemies;
        _bonuses = bonuses;
        _enemyWaveStack = enemyWaveStack;
        _spawnManager = spawnManager;
        _waveProcessor = waveProcessor;
        _bulletPool = bulletPool;
        _bulletManager = bulletManager;
        _uiManager = uiManager;
        _gameArea = gameArea;
        _graphicsDevice = graphicsDevice;
    }

    public void Update(GameTime gameTime, InputHandler inputHandler)
    {
        if (!IsPlayerAlive) return;
        preBattleTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        _spawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (!battleStarted)
        {
            if (preBattleTimer >= PreBattleDelay)
            {
                battleStarted = true;
                _spawnTimer = 0f;
            }
            return;
        }

        if (!_isWaveInProgress)
            _enemySpawnTimer += deltaTime;

        UpdateBonuses(deltaTime);
        TrySpawnBonus(deltaTime);
        ProcessEnemyWaves(deltaTime);
        UpdateEnemies(gameTime);
        CheckPlayerCollisions();
        CleanupBullets();
        UpdateBulletManager(gameTime);
        UpdatePlayer(gameTime);
    }

    private void UpdateBonuses(float deltaTime)
    {
        foreach (var bonus in _bonuses.ToList())
        {
            bonus.Update(deltaTime);
            if (bonus._model.TimeLeft <= 0)
            {
                _bonuses.Remove(bonus);
                CountBonusNow--;
                _bonusSpawnTimer = BonusSpawnCooldown;
                _canSpawnBonus = false;
            }
        }
    }

    private void TrySpawnBonus(float deltaTime)
    {
        if (!_canSpawnBonus)
        {
            _bonusSpawnTimer -= deltaTime;
            if (_bonusSpawnTimer <= 0) _canSpawnBonus = true;
        }

        if (_canSpawnBonus && MaxCountBonus > CountBonusNow)
        {
            SpawnBonus();
            _canSpawnBonus = false;
            _bonusSpawnTimer = BonusSpawnCooldown;
        }
    }

    private void ProcessEnemyWaves(float deltaTime)
    {
        if (_enemies.Count == 0 && _enemyWaveStack.Count > 0)
        {
            if (_isWaveInProgress) _isWaveInProgress = false;

            if (_enemySpawnTimer >= EnemySpawnInterval && !_isWaveInProgress)
            {
                _isWaveInProgress = true;
                var nextWave = _enemyWaveStack.Pop();

                if (nextWave is WaveData waveData)
                    _spawnManager.ProcessWaveData(waveData);
                else if (nextWave is List<Action> actionWave)
                    _waveProcessor.ProcessWaveItems(actionWave);

                _enemySpawnTimer = 0f;
            }
        }
    }

    private void UpdateEnemies(GameTime gameTime)
    {
        foreach (var enemy in _enemies.ToList())
        {
            enemy.Update(gameTime, _bulletPool);
        }
    }

    private void CheckPlayerCollisions()
    {
        foreach (var bonus in _bonuses.ToList())
        {
            if (SATCollision.CheckCollision(_player.Model.GetVertices(), bonus._view.GetVertices()))
            {
                bonus.ApplyEffect(_player.Model);
                Name = bonus._model.Name;
                NameColor = bonus._model.Color;
                CountBonusNow--;
                _bonuses.Remove(bonus);
            }
        }
    }

    private void CleanupBullets() => _bulletPool.Cleanup();
    private void UpdateBulletManager(GameTime gameTime) => _bulletManager.Update(gameTime);

    private void UpdatePlayer(GameTime gameTime)
    {
        _player.SetViewport(_graphicsDevice.Viewport);
        _player.SetGeameArea(_gameArea);
        _player.Update(gameTime);
    }

    private bool SpawnBonus()
    {
        bool spawned = _spawnManager.SpawnBonus();
        if (spawned)
        {
            CountBonusNow++;
            return true;
        }
        return false;
    }

    public void ResetGameState(int lvl = 1)
    {
        _enemies.Clear();
        _bonuses.Clear();

        _bulletPool.ForceCleanup();
        _bulletPool.Cleanup();

        Lvl = lvl;

        battleStarted = false;
        preBattleTimer = 0f;
        _spawnTimer = 0f;
        _isWaveInProgress = false;

        _bonusSpawnTimer = 0f;
        _canSpawnBonus = false;
        CountBonusNow = 0;

        if (_player.Model.GameArea.Width == 0 || _player.Model.GameArea.Height == 0)
        {
            _player.Model.GameArea = _gameArea;
        }
        _currentLevelData = LevelLoader.LoadLevel(lvl);
        _player.Model.UpdatePosition(_currentLevelData.PlayerStart.Position);

        MaxCountBonus = _currentLevelData.MaxBonusCount;
        Name = _currentLevelData.LevelName;
        NameColor = _currentLevelData.LevelNameColor;

        _defaultBonus.ApplyEffect(_player.Model);
        _player.Model.Health = _currentLevelData.PlayerStart.Health;

        _spawnManager.InitializeWaveStack(_currentLevelData);

        Name = _defaultBonus._model.Name;
        NameColor = _defaultBonus._model.Color;
    }

}