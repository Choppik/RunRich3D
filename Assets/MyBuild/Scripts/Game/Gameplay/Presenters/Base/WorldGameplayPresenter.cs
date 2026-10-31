using MyBuild.Scripts.Game.Components.LevelGenerate;
using R3;
using System;
using System.Collections;
using UnityEngine;

namespace MyBuild.Scripts.Game.Gameplay.Managers
{
    /// <summary>
    /// Презентер мира: управляет генерацией уровня и связывает сервисы.
    /// </summary>
    public class WorldGameplayPresenter : IDisposable
    {
        private readonly PoolManager _poolManager;
        private readonly LevelGenerator _levelGenerator;
        private readonly LevelManager _levelManager;
        private readonly ScoreManager _scoreManager;
        private readonly QualityManager _qualityManager;
        private readonly CompositeDisposable _compositeDisposable = new();

        public WorldGameplayPresenter(PoolManager poolManager, LevelGenerator levelGenerator, LevelManager levelManager, ScoreManager scoreManager, QualityManager qualityManager)
        {
            _poolManager = poolManager;
            _levelGenerator = levelGenerator;
            _levelManager = levelManager;
            _scoreManager = scoreManager;
            _qualityManager = qualityManager;
        }

        /// <summary>
        /// Запуск генератора уровня.
        /// </summary>
        public IEnumerator StartLevelGeneration()
        {
            yield return _poolManager.Initialize();

            _levelManager.StartCurrentLevel();
            _qualityManager.ReadyQualityManager();
            _scoreManager.SubscribeToPickups();

            yield return new WaitUntil(() => _levelGenerator.IsGenerationComplete);

            Debug.Log($"[{nameof(WorldGameplayPresenter)}] Генерация уровня запущена.");
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}
