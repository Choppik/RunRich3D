using MyBuild.Scripts.Game.Common;
using R3;
using UnityEngine;

namespace MyBuild.Scripts.Game.Components.LevelGenerate
{
    /// <summary>
    /// Дверь между сегментами дороги. Открывается только если шкала достаточна.
    /// </summary>
    public class DoorSegment : SegmentBase
    {
        [Tooltip("Минимальная шкала (0..1), чтобы дверь открылась")]
        [SerializeField] private float _requiredScale = 0.3f;

        [Tooltip("Аниматор двери (или Transform для вращения/сдвига)")]
        [SerializeField] private Transform _leftDoor;
        [SerializeField] private Transform _rightDoor;

        [SerializeField, Range(1, 5)] private int _doorXValue;

        [Header("Visuals")]
        [SerializeField] private float _openAngle = 90f;
        [SerializeField] private float _openSpeed = 2f;
        [SerializeField] private bool _isFinalDoor = false;

        private bool _isOpen = false;
        private bool _playerReached = false;
        private Quaternion _leftClosed;
        private Quaternion _rightClosed;
        private Quaternion _leftOpen;
        private Quaternion _rightOpen;

        /// <summary>
        /// Срабатывает при открытии двери.
        /// </summary>
        public static Observable<int> OnOpenDoor => _onOpenDoor;

        private static readonly Subject<int> _onOpenDoor = new();

        public bool IsFinalDoor => _isFinalDoor;

        public bool IsOpen => _isOpen;

        private void Awake()
        {
            if (_leftDoor != null) _leftClosed = _leftDoor.localRotation;
            if (_rightDoor != null) _rightClosed = _rightDoor.localRotation;

            _leftOpen = _leftClosed * Quaternion.Euler(0, -_openAngle, 0);
            _rightOpen = _rightClosed * Quaternion.Euler(0, _openAngle, 0);
        }

        private void Update()
        {
            if (_isOpen)
            {
                if (_leftDoor != null)
                    _leftDoor.localRotation = Quaternion.Lerp(_leftDoor.localRotation, _leftOpen, Time.deltaTime * _openSpeed);
                if (_rightDoor != null)
                    _rightDoor.localRotation = Quaternion.Lerp(_rightDoor.localRotation, _rightOpen, Time.deltaTime * _openSpeed);
            }
        }

        private void OnDisable()
        {
            ResetDoor();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(AppConsts.Player)) return;
            if (!_playerReached)
            {
                NotifyPlayerEntered();
                _playerReached = true;
                TryOpen();
            }
        }

        /// <summary>
        /// Полный сброс двери в закрытое состояние.
        /// Вызывается автоматически при деспавне (OnDisable)
        /// и вручную — при выдаче из пула, если нужно.
        /// </summary>
        public void ResetDoor()
        {
            _isOpen = false;
            _playerReached = false;

            if (_leftDoor != null)
                _leftDoor.localRotation = _leftClosed;

            if (_rightDoor != null)
                _rightDoor.localRotation = _rightClosed;
        }

        /// <summary>
        /// Проверяет шкалу и открывает либо останавливает игру.
        /// </summary>
        public void TryOpen()
        {
             _isOpen = true;
            _onOpenDoor.OnNext(_doorXValue);
            //float scale = ScoreManager.Instance != null ? ScoreManager.Instance.GetNormalizedScale() : 0f;
            //
            //if (scale >= requiredScale)
            //{
            //    Debug.Log($"[DoorSegment] Дверь открыта! Шкала: {scale:F2}, нужно: {requiredScale:F2}");
            //
            //    // Если финальная дверь — победа
            //    if (isFinalDoor)
            //    {
            //        ScoreManager.Instance?.OnFinalDoorOpened();
            //    }
            //}
            //else
            //{
            //    Debug.Log($"[DoorSegment] Недостаточно шкалы! Есть: {scale:F2}, нужно: {requiredScale:F2}");
            //    // Останавливаем игру
            //    ScoreManager.Instance?.OnDoorFailed();
            //}
        }
    }
}
