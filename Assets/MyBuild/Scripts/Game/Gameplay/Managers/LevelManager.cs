using DI;
using MyBuild.Scripts.Game.Settings;
using MyBuild.Scripts.Game.State.Providers;
using MyBuild.Scripts.Game.State.Root.Proxy;
using MyBuild.Scripts.Utils;
using MyBuild.Scripts.Utils.LevelGenerate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MyBuild.Scripts.Game.Gameplay.Managers
{
    /// <summary>
    /// Менеджер управления уровнями.
    /// </summary>
    public class LevelManager
    {
        private readonly ScoreManager _scoreManager;
        private readonly QualityManager _qualityManager;
        private readonly IGameStateProvider _gameState;
        private readonly LevelGenerator _levelGenerator;
        private readonly PoolManager _poolManager;
        private readonly Coroutines _coroutine;
        private readonly ISettingsProvider _settings;

        public int CurrentLevel => GetCurrentLevel();
        public bool IsLevelGenerate => _levelGenerator.IsGenerationComplete;

        public LevelManager(
            ScoreManager scoreManager,
            QualityManager qualityManager,
            IGameStateProvider gameState,
            LevelGenerator levelGenerator,
            PoolManager poolManager,
            Coroutines coroutine,
            ISettingsProvider settings)
        {
            _scoreManager = scoreManager;
            _qualityManager = qualityManager;
            _gameState = gameState;
            _levelGenerator = levelGenerator;
            _poolManager = poolManager;
            _coroutine = coroutine;
            _settings = settings;
        }

        private void GenerateNewLevel()
        {
            _levelGenerator.ClearLevel();

            var levelSetting = _settings.GameSettings.LevelsList.Lvls
                .Where(l => CurrentLevel < l.MaxNumberLevel)
                .OrderBy(l => l.MaxNumberLevel)
                .FirstOrDefault();

            if (levelSetting != null)
            {
                _levelGenerator.SetLevelParams(levelSetting.StraightCount, levelSetting.TurnCount, levelSetting.DoorCount);
            }

            _coroutine.StartCoroutine(_levelGenerator.GenerateLevel());
        }

        public void StartLevel(int level) // TODO: Правка нужна
        {
            var map = _gameState.GameState.GetGurrentMap();
            if (map != null)
            {
                var data = map.Dataes.FirstOrDefault();

                data?.CurrentLevel.OnNext(level);
            }

            GenerateNewLevel();
        }

        public void Run()
        {
            _levelGenerator.SetMove(true);
        }

        public void StartCurrentLevel()
        {
            StartLevel(CurrentLevel);
        }

        private int GetCurrentLevel() // TODO: Правка нужна
        {
            var map = _gameState.GameState.GetGurrentMap();
            if (map != null)
            {
                var data = map.Dataes.FirstOrDefault();

                if (data != null)
                {
                    return data.CurrentLevel.CurrentValue;
                }
            }
            
            return 0;
        }
    }
}
