using MyBuild.Scripts.Game.Common;
using UnityEngine;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    /// <summary>
    /// Дорога: прямая или поворот. Содержит триггер старта, точки спавна пикапов и точки декора.
    /// </summary>
    public class RoadSegment : SegmentBase
    {
        [Header("Road")]
        [SerializeField] private RoadShape shape = RoadShape.Straight;

        [Header("Bounds (относительно центра дороги)")]
        [Tooltip("Левая граница по X")]
        [SerializeField] private float leftBound = -1f;
        [Tooltip("Правая граница по X")]
        [SerializeField] private float rightBound = 1f;
        [Tooltip("Длина дороги по Z (для прямой)")]
        [SerializeField] private float length = 10f;
        [SerializeField] private Transform startPoint;

        [Header("Trigger")]
        [Tooltip("Триггер в начале дороги. При входе — активируется сегмент.")]
        [SerializeField] private Collider enterTrigger;

        [Header("Decoration Points")]
        [Tooltip("Пустые объекты для скамеек")]
        [SerializeField] private Transform[] decorationBenchPoints;
        [Tooltip("Пустые объекты для елементов финиша")]
        [SerializeField] private Transform[] decorationFinishElementsPoints;
        [Tooltip("Пустые объекты для кустов")]
        [SerializeField] private Transform[] decorationPlantPoints;
        [Tooltip("Пустые объекты для мусора")]
        [SerializeField] private Transform[] decorationTrashPoints;
        [Tooltip("Пустые объекты для контрольной точки")]
        [SerializeField] private Transform[] savePoints;
        [Tooltip("Пустые объекты для полосы финиша")]
        [SerializeField] private Transform[] finishPoints;

        [Header("Check For Delete Last Segment")]
        [Tooltip("Триггер в середине дороги. При входе удаляется предыдущий сегмет.")]
        [SerializeField] private Collider checkForDeleteTrigger;

        // Ссылки (заполняются LevelGenerator'ом)
        [HideInInspector] public DoorSegment nextDoor;
        [HideInInspector] public bool isLastBeforeEnd = false;
        [HideInInspector] public EndTrigger endTrigger;

        public Transform StartPoint { get => startPoint; }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            NotifyPlayerEntered();
        }

        /// <summary>
        /// Вызывается когда игрок дошёл до конца дороги.
        /// </summary>
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

        /// <summary>
        /// Смена тира (визуал/материал).
        /// </summary>
        public override void SetTier(TierType newTier)
        {
            base.SetTier(newTier);
            // Здесь можно менять материалы/модельки в зависимости от тира
            // Например, через Addressables подгрузить другой вариант
        }

        /// <summary>
        /// Границы для PlayerController в локальных координатах дороги.
        /// </summary>
        public float GetLeftBound() => leftBound;
        public float GetRightBound() => rightBound;
        public RoadShape GetRoadShape() => shape;
    }
}
