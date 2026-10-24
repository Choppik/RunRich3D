using R3;
using UnityEngine;

namespace MyBuild.Scripts.Game.Gameplay.Managers
{
    /// <summary>
    /// Менеджер управления счётом.
    /// </summary>
    public class ScoreManager
    {
        private int _score = 0;
        private readonly int _maxScore = 1000;
        public Observable<int> OnScoreChanged => _onScoreChanged;
        private readonly Subject<int> _onScoreChanged = new();
        public Observable<Unit> OnCollectPositive => _onCollectPositive;
        private readonly Subject<Unit> _onCollectPositive = new();
        public Observable<Unit> OnCollectNegative => _onCollectNegative;
        private readonly Subject<Unit> _onCollectNegative = new();

        /// <summary>
        /// Установка начального значения счета.
        /// </summary>
        public void SetScore(int startScore) => _score = startScore;

        /// <summary>
        /// Собрали позитивный пинап.
        /// </summary>
        public void CollectPositive(int value = 1)
        {
            AddScore(value);
            _onCollectPositive.OnNext(Unit.Default);
            Debug.Log($"[ScoreManager] +{value}. Счёт: {_score}.");
        }

        /// <summary>
        /// Собрали негативный пинап.
        /// </summary>
        public void CollectNegative(int value = 1)
        {
            AddScore(-value);
            _onCollectNegative.OnNext(Unit.Default);
            Debug.Log($"[ScoreManager] -{value}. Счёт: {_score}.");
        }

        /// <summary>
        /// Изменение счета.
        /// </summary>
        private void AddScore(int amount)
        {
            _score += amount;
            _score = Mathf.Max(0, _score);
            _score = Mathf.Max(_maxScore, _score);
            _onScoreChanged.OnNext(_score);
        }
    }
}
