using MyBuild.Scripts.Game.Common;
using R3;
using UnityEngine;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    /// <summary>
    /// Абстрактный класс сегмента (дорога, дверь, финал). 
    /// </summary>
    public abstract class SegmentBase : MonoBehaviour
    {
        public enum SegmentType { Road, Door, End } // Типы сегментов.

        [Header("Connection Points")]
        [SerializeField] private Transform entryPoint;
        [SerializeField] private Transform exitPoint;

        [Header("Type Segment")]
        [SerializeField] private SegmentType type = SegmentType.Road;
        [Header("Level")]
        [SerializeField] private TierType tire = TierType.None;

        /// <summary>
        /// Точка, которой сегмент стыкуется с предыдущим.
        /// </summary>
        public Transform ConnectPoint => entryPoint;

        /// <summary>
        /// Точка, откуда начинается следующий сегмент.
        /// </summary>
        public Transform LeavePoint => exitPoint;

        private int _segmentIndex = -1;

        public Observable<int> OnPlayerEnteredCallback => _playerEnteredCallback;

        private readonly Subject<int> _playerEnteredCallback = new();

        private bool _activated = false;

        /// <summary>
        /// Установка ииндекса текущего сегмента.
        /// </summary>
        public void SetSegmentIndex(int index)
        {
            _segmentIndex = index;
        }

        /// <summary>
        /// Установка текущего сегмента на определенную позицию.
        /// </summary>
        public void AlignEntryTo(Vector3 worldPos, Quaternion worldRot)
        {
            Transform connect = ConnectPoint;

            transform.SetPositionAndRotation(
                worldPos - transform.TransformVector(connect.localPosition),
                worldRot * Quaternion.Inverse(connect.localRotation)
            );
        }

        /// <summary>
        /// Получение точки стыковки текущего сегммента со следующим.
        /// </summary>
        public void GetExitData(out Vector3 pos, out Quaternion rot)
        {
            pos = LeavePoint.position;
            rot = LeavePoint.rotation;
        }

        /// <summary>
        /// Установка уровня.
        /// </summary>
        public virtual void SetTier(TierType value)
        {
            tire = value;
        }

        /// <summary>
        /// Сброс состояния при переиспользовании из пула.
        /// </summary>
        public virtual void ResetState()
        {
            _activated = false;
            _segmentIndex = -1;
        }

        /// <summary>
        /// Вызывается при входе игрока в триггер сегмента.
        /// </summary>
        protected void NotifyPlayerEntered()
        {
            if (_activated) return;
            _activated = true;
            _playerEnteredCallback.OnNext(_segmentIndex);
        }
    }
}
