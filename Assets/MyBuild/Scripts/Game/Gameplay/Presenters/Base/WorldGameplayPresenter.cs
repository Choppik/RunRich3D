using MyBuild.Scripts.Utils.LevelGenerate;
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
        private readonly CompositeDisposable _compositeDisposable = new();

        public WorldGameplayPresenter(PoolManager poolManager, LevelGenerator levelGenerator, LevelManager levelManager)
        {
            _poolManager = poolManager;
            _levelGenerator = levelGenerator;
            _levelManager = levelManager;
        }

        /// <summary>
        /// Запуск генератора уровня.
        /// </summary>
        public IEnumerator StartLevelGeneration()
        {
            yield return _poolManager.Initialize();

            _levelManager.StartCurrentLevel();

            yield return new WaitUntil(() => _levelGenerator.IsGenerationComplete);

            Debug.Log($"[{nameof(WorldGameplayPresenter)}] Генерация уровня запущена.");
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}
