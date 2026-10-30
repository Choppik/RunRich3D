using MyBuild.Scripts.Game.Common;
using R3;
using System;
using UnityEngine;

namespace MyBuild.Scripts.Game.Components.LevelGenerate
{
    [RequireComponent(typeof(Collider))]
    public class PickupTrigger : MonoBehaviour
    {
        [SerializeField] private PoolManager _poolManager;

        /// <summary>
        /// Тип пинапа — задаётся на префабе, скрипт зоны его не трогает.
        /// </summary>
        public bool IsPositive { get; private set; }

        /// <summary>
        /// true с момента подбора до Init. Блокирует повторное срабатывание.
        /// </summary>
        public bool IsCollected { get; private set; }

        /// <summary>
        /// Стреляет один раз при подборе, до возврата в пул.
        /// </summary>
        public Observable<PickupTrigger> CollectedAndReturned => _collectedAndReturned;

        private readonly Subject<PickupTrigger> _collectedAndReturned = new();

        /// <summary>
        /// Срабатывает при подборе: подписывается gameplay-логика
        /// (начислить/списать валюту, эффект, звук).
        /// </summary>
        public static Observable<PickupTrigger> OnCollected => _onCollected;

        private static readonly Subject<PickupTrigger> _onCollected = new();

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(AppConsts.Player))
                return;

            IsCollected = true;

            _onCollected.OnNext(this);
            _collectedAndReturned.OnNext(this);

            if (_poolManager != null)
                _poolManager.Despawn(gameObject);
            else
                gameObject.SetActive(false);
        }

        /// <summary>
        /// Прописывается зоной спавна в момент выдачи из пула,
        /// чтобы тип переживал переиспользование объекта из пула.
        /// </summary>
        public void Init(bool isPositive, PoolManager pool)
        {
            IsPositive = isPositive;
            IsCollected = false;
            _poolManager = pool;
        }
    }
}
