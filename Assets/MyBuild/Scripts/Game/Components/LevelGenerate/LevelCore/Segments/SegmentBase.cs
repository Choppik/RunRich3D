using MyBuild.Scripts.Game.Common;
using R3;
using UnityEngine;

namespace MyBuild.Scripts.Game.Components.LevelGenerate
{
    /// <summary>
    /// Абстрактный класс сегмента (дорога, дверь, финал). 
    /// </summary>
    public abstract class SegmentBase : MonoBehaviour
    {
        public enum SegmentType { Road, Door, End } // Типы сегментов.

        [Header("Connection Points")]
        [SerializeField] private Transform _entryPoint;
        [SerializeField] private Transform _exitPoint;

        [Header("Pickup Zone")]
        [SerializeField] private PickupSpawnZone _pickupSpawnZone;

        [Header("Type Segment")]
        [SerializeField] private SegmentType _type = SegmentType.Road;

        [Header("Level")]
        [SerializeField] private TierType _tire = TierType.None;

        /// <summary>
        /// Точка, которой сегмент стыкуется с предыдущим.
        /// </summary>
        public Transform ConnectPoint => _entryPoint;

        /// <summary>
        /// Точка, откуда начинается следующий сегмент.
        /// </summary>
        public Transform LeavePoint => _exitPoint;

        public int SegmentIndex => _segmentIndex;

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
            if (_exitPoint == null)
            {
                // Громко ругаемся один раз, но не роняем генерацию
                Debug.LogError($"[{name}] Exit Point не назначен! Использую позицию самого сегмента как выход.");

                // Дефолт: выход = вход сегмента (объект просто продолжит дорогу по прямой)
                pos = _entryPoint != null
                    ? _entryPoint.position + transform.forward * 10f
                    : transform.position + transform.forward * 10f;

                rot = transform.rotation;
                return;
            }

            pos = _exitPoint.position;
            rot = _exitPoint.rotation;
        }

        /// <summary>
        /// Установка уровня.
        /// </summary>
        public virtual void SetTier(TierType value)
        {
            _tire = value;
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
        /// Направление движения дороги в заданной мировой точке.
        /// Ось сегментов направлена против движения, поэтому forward точек инвертируется.
        /// </summary>
        public Vector3 GetMoveDirectionAt(Vector3 worldPos)
        {
            Vector3 entryDir = -ConnectPoint.forward;
            Vector3 exitDir = -LeavePoint.forward;

            Vector3 localEntry = transform.InverseTransformPoint(ConnectPoint.position);
            Vector3 localExit = transform.InverseTransformPoint(LeavePoint.position);
            Vector3 localPos = transform.InverseTransformPoint(worldPos);

            float t = Mathf.InverseLerp(localEntry.z, localExit.z, localPos.z);
            t = Mathf.Clamp01(t);

            // ВАЖНО: Просто берем направление. Если нужна дуга - оставь Lerp, 
            // но проблема "недоповорота" решается не здесь, а в Move().
            Vector3 dir = Vector3.Lerp(entryDir, exitDir, t);

            dir.y = 0f;
            return dir.sqrMagnitude > 0.001f ? dir.normalized : dir;
        }

        /// <summary>
        /// Спавнит контент сегмента. Вызывать ПОСЛЕ AlignEntryTo,
        /// когда позиция и поворот сегмента уже финальные.
        /// </summary>
        public void SpawnContent()
        {
            _pickupSpawnZone?.SpawnContent();
        }

        /// <summary>
        /// Плотный спавн контента.
        /// </summary>
        public void SpawnDenseContent()
        {
            _pickupSpawnZone?.SpawnDenseContent();
        }

        /// <summary>
        /// Сдаёт контент при возврате сегмента в пул.
        /// </summary>
        public void DespawnContent()
        {
            _pickupSpawnZone?.DespawnAll();
        }

        /// <summary>
        /// Вызывается при входе игрока в триггер сегмента.
        /// </summary>
        protected void NotifyPlayerEntered()
        {
            Debug.Log($"Обработчик сегмента сработал {nameof(NotifyPlayerEntered)}");
            if (_activated) return;
            _activated = true;
            _playerEnteredCallback.OnNext(_segmentIndex);
        }
    }
}
