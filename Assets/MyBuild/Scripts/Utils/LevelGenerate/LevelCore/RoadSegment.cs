using UnityEngine;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    /// <summary>
    /// Дорога: прямая или поворот. Содержит триггер старта, точки спавна пикапов и точки декора.
    /// </summary>
    public class RoadSegment : SegmentBase
    {
        public enum RoadShape { Straight, Turn }

        [Header("Road")]
        public RoadShape shape = RoadShape.Straight;
        public int tier = 0; // 0=база, 1=лучшая, 2=лучшая

        [Header("Bounds (относительно центра дороги)")]
        [Tooltip("Левая граница по X")]
        public float leftBound = -2f;
        [Tooltip("Правая граница по X")]
        public float rightBound = 2f;
        [Tooltip("Длина дороги по Z (для прямой)")]
        public float length = 10f;

        [Header("Trigger")]
        [Tooltip("Триггер в начале дороги. При входе — активируется сегмент.")]
        public Collider enterTrigger;

        [Header("Pickup Spawn Points")]
        [Tooltip("Пустые объекты на дороге, где могут появиться пикапы")]
        public Transform[] pickupSpawnPoints;

        [Header("Decoration Points")]
        [Tooltip("Пустые объекты для декора (кусты, ковры и т.д.)")]
        public Transform[] decorationPoints;

        [Header("End Check")]
        [Tooltip("Триггер в конце дороги (перед дверью). null если двери нет.")]
        public Collider endCheckTrigger;

        // Ссылки (заполняются LevelGenerator'ом)
        [HideInInspector] public DoorSegment nextDoor;
        [HideInInspector] public bool isLastBeforeEnd = false;
        [HideInInspector] public EndTrigger endTrigger;

        private bool _activated = false;

        void Awake()
        {
            type = SegmentType.Road;
        }

        void OnTriggerEnter(Collider other)
        {
            if (_activated) return;
            if (!other.CompareTag("Player")) return;

            _activated = true;
            OnPlayerEntered();
        }

        void OnPlayerEntered()
        {
            // 1. Спавним пикапы (если есть PickupSpawner)
            var spawner = GetComponent<PickupSpawner>();
            if (spawner != null)
                spawner.SpawnRandom();

            // 2. Спавним декор (если есть DecorationPlacer)
            var decor = GetComponent<DecorationPlacer>();
            if (decor != null)
                decor.PlaceDecorations();
        }

        /// <summary>Вызывается когда игрок дошёл до конца дороги.</summary>
        public void OnPlayerReachedEnd()
        {
            if (nextDoor != null)
            {
                nextDoor.TryOpen();
            }
            else if (isLastBeforeEnd && endTrigger != null)
            {
                endTrigger.OnPlayerReached();
            }
        }

        /// <summary>Смена тира (визуал/материал).</summary>
        public void SetTier(int newTier)
        {
            tier = Mathf.Clamp(newTier, 0, 2);
            // Здесь можно менять материалы/модельки в зависимости от тира
            // Например, через Addressables подгрузить другой вариант
        }

        /// <summary>Границы для PlayerController в локальных координатах дороги.</summary>
        public float GetLeftBound() => leftBound;
        public float GetRightBound() => rightBound;
    }
}
