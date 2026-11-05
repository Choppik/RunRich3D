using MyBuild.Scripts.Game.Common;
using UnityEngine;

namespace MyBuild.Scripts.Game.Components.LevelGenerate
{
    /// <summary>
    /// Дорога: прямая или поворот. Содержит триггер старта, точки спавна пикапов и точки декора.
    /// </summary>
    public class RoadSegment : SegmentBase
    {
        [Header("Road")]
        [SerializeField] private RoadShape _shape = RoadShape.Straight;
        [SerializeField] private Transform _playerSpawn;

        [Header("Bounds (относительно центра дороги)")]
        [Tooltip("Левая граница по X")]
        [SerializeField] private float _leftBound = -1f;
        [Tooltip("Правая граница по X")]
        [SerializeField] private float _rightBound = 1f;
        [Tooltip("Длина дороги по Z (для прямой)")]
        [SerializeField] private float _length = 10f;

        [Header("Decoration Points")]
        [Tooltip("Пустые объекты для скамеек")]
        [SerializeField] private Transform[] _decorationBenchPoints;
        [Tooltip("Пустые объекты для елементов финиша")]
        [SerializeField] private Transform[] _decorationFinishElementsPoints;
        [Tooltip("Пустые объекты для кустов")]
        [SerializeField] private Transform[] _decorationPlantPoints;
        [Tooltip("Пустые объекты для мусора")]
        [SerializeField] private Transform[] _decorationTrashPoints;
        [Tooltip("Пустые объекты для контрольной точки")]
        [SerializeField] private Transform[] _savePoints;
        [Tooltip("Пустые объекты для полосы финиша")]
        [SerializeField] private Transform[] _finishPoints;
        public Transform PlayerSpawn => _playerSpawn;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(AppConsts.Player)) return;
            NotifyPlayerEntered();
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
        /// Получение координат точек декора на дороге.
        /// </summary>
        /// <param name="typeDecor">Тип декора.</param>
        public Transform[] AddDecor(TypeDecor typeDecor)
        {
            switch (typeDecor)
            {
                case TypeDecor.Plant:
                    return CheckArr(_decorationPlantPoints) ? _decorationPlantPoints : null;
                case TypeDecor.Trash:
                    return CheckArr(_decorationTrashPoints) ? _decorationTrashPoints : null;
                case TypeDecor.Bench:
                    return CheckArr(_decorationBenchPoints) ? _decorationBenchPoints : null;
                case TypeDecor.FinishElements:
                    return CheckArr(_decorationFinishElementsPoints) ? _decorationFinishElementsPoints : null;
                case TypeDecor.FinishLine:
                    return CheckArr(_finishPoints) ? _finishPoints : null;
                // TODO: Еще линия сохранения и ковры должны быть.
                case TypeDecor.None:
                default:
                    return null;
            }
        }

        private bool CheckArr(Transform[] array)
        {
            return array != null && array.Length > 0;
        }
    }
}
