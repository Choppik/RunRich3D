using MyBuild.Scripts.Game.Common;
using MyBuild.Scripts.Game.Components.LevelGenerate;
using R3;
using System;
using UnityEngine;

namespace MyBuild.Scripts.Game.Gameplay.Managers
{
    /// <summary>
    /// Управляет тирами дороги, двери и игрока. Реагирует на изменение счета.
    /// </summary>
    public class QualityManager : IDisposable
    {
        public TierType CurrentTier { get; private set; } = TierType.Poor;
        private LevelGenerator _levelGen;
        private ScoreManager _scoreManager;
        private readonly CompositeDisposable _compositeDisposable = new();

        public QualityManager(LevelGenerator levelGen, ScoreManager scoreManager)
        {
            _levelGen = levelGen;
            _scoreManager = scoreManager;
        }

        /// <summary>
        /// Объявление готовности менеджера.
        /// </summary>
        public void ReadyQualityManager()
        {
            _scoreManager.OnScoreChanged.Subscribe(OnTierChanged).AddTo(_compositeDisposable);
            ApplyTiers();
        }

        /// <summary>
        /// Обработчик события изменения счетчика.
        /// </summary>
        public void OnTierChanged(int value)
        {
            if (value == 0 || _scoreManager == null) return;

            float v = value;

            float percentage = (v / _scoreManager.MaxScore) * 100f;

            // Опционально: ограничиваем диапазон от 0 до 100, если значения могут выходить за пределы
            var result = Mathf.Clamp(percentage, 0f, 100f);

            if (result <= 25)
            {
                CurrentTier = TierType.Poor;
            }
            else if (result > 25 && result <= 50)
            {
                CurrentTier = TierType.Casual;
            }
            else if (result > 50 && result <= 75)
            {
                CurrentTier = TierType.Rich;
            }
            else if (result > 75 && result <= 100)
            {
                CurrentTier = TierType.Millionaire;
            }

            ApplyTiers();
        }

        /// <summary>
        /// Применение нового тира.
        /// </summary>
        private void ApplyTiers()
        {
            if (_levelGen != null)
                _levelGen.ApplyTiers(CurrentTier);
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}
