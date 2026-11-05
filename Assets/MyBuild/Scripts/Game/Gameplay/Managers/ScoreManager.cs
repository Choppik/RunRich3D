using R3;
using MyBuild.Scripts.Game.Components.LevelGenerate;
using UnityEngine;
using System;

namespace MyBuild.Scripts.Game.Gameplay.Managers
{
    /// <summary>
    /// Менеджер управления счётом.
    /// </summary>
    public class ScoreManager : IDisposable
    {
        public int Score { get; private set; }
        public int MaxScore { get; private set; } = 5000;

        public Observable<int> OnScoreChanged => _onScoreChanged;
        private readonly Subject<int> _onScoreChanged = new();

        public Observable<Unit> OnCollectPositive => _onCollectPositive;
        private readonly Subject<Unit> _onCollectPositive = new();

        public Observable<Unit> OnCollectNegative => _onCollectNegative;
        private readonly Subject<Unit> _onCollectNegative = new();

        private readonly CompositeDisposable _compositeDisposable = new();

        /// <summary>
        /// Установка начального значения счёта.
        /// </summary>
        public void SetScore(int startScore) => Score = startScore;

        /// <summary>
        /// Подписка на статический поток подборов. Вызвать один раз при старте забега.
        /// </summary>
        public void SubscribeToPickups()
        {
            PickupTrigger.OnCollected
                .Subscribe(OnPickupCollected)
                .AddTo(_compositeDisposable);

            DoorSegment.OnOpenDoor
                .Subscribe(MultiplyScore)
                .AddTo(_compositeDisposable);
        }

        private void OnPickupCollected(PickupTrigger pickup)
        {
            if (pickup.IsPositive)
                CollectPositive(pickup.Value);
            else
                CollectNegative(pickup.Value);
        }

        /// <summary>
        /// Собрали позитивный пинап.
        /// </summary>
        public void CollectPositive(int value = 1)
        {
            AddScore(value);
            _onCollectPositive.OnNext(Unit.Default);
            Debug.Log($"[{nameof(ScoreManager)}] +{value}. Счёт: {Score}.");
        }

        /// <summary>
        /// Собрали негативный пинап.
        /// </summary>
        public void CollectNegative(int value = 1)
        {
            AddScore(-value);
            _onCollectNegative.OnNext(Unit.Default);
            Debug.Log($"[{nameof(ScoreManager)}] -{value}. Счёт: {Score}.");
        }

        /// <summary>
        /// Сброс состояния на новый забег.
        /// </summary>
        public void Reset()
        {
            Score = 0;
            _onScoreChanged.OnNext(Score);
        }

        private void AddScore(int amount)
        {
            Score += amount;
            Score = Mathf.Clamp(Score, 0, MaxScore);
            _onScoreChanged.OnNext(Score);
        }

        private void MultiplyScore(int amount)
        {
            Score *= amount;
            Score = Mathf.Clamp(Score, 0, MaxScore);
            _onScoreChanged.OnNext(Score);
        }

        /// <summary>
        /// Снять подписку на подборы. Вызвать при уничтожении владельца менеджера.
        /// </summary>
        public void Dispose() => _compositeDisposable.Dispose();
    }
}