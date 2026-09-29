using UnityEngine;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    /// <summary>
    /// Дверь между сегментами дороги. Открывается только если шкала достаточна.
    /// </summary>
    public class DoorSegment : SegmentBase
    {
        [Tooltip("Минимальная шкала (0..1), чтобы дверь открылась")]
        [SerializeField] private float requiredScale = 0.3f;

        [Tooltip("Аниматор двери (или Transform для вращения/сдвига)")]
        [SerializeField] private Transform leftDoor;
        [SerializeField] private Transform rightDoor;

        [Tooltip("Триггер перед дверью")]
        [SerializeField] private Collider doorTrigger;

        [Header("Visuals")]
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float openSpeed = 2f;
        [SerializeField] private bool isFinalDoor = false;

        private bool _isOpen = false;
        private bool _playerReached = false;
        private Quaternion _leftClosed;
        private Quaternion _rightClosed;
        private Quaternion _leftOpen;
        private Quaternion _rightOpen;

        public bool IsFinalDoor => isFinalDoor;

        void Start()
        {
            if (leftDoor != null) _leftClosed = leftDoor.localRotation;
            if (rightDoor != null) _rightClosed = rightDoor.localRotation;

            _leftOpen = _leftClosed * Quaternion.Euler(0, -openAngle, 0);
            _rightOpen = _rightClosed * Quaternion.Euler(0, openAngle, 0);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (!_playerReached)
            {
                _playerReached = true;
                TryOpen();
            }
        }

        /// <summary>Проверяет шкалу и открывает либо останавливает игру.</summary>
        public void TryOpen()
        {
            float scale = ScoreManager.Instance != null ? ScoreManager.Instance.GetNormalizedScale() : 0f;

            if (scale >= requiredScale)
            {
                _isOpen = true;
                Debug.Log($"[DoorSegment] Дверь открыта! Шкала: {scale:F2}, нужно: {requiredScale:F2}");

                // Если финальная дверь — победа
                if (isFinalDoor)
                {
                    ScoreManager.Instance?.OnFinalDoorOpened();
                }
            }
            else
            {
                Debug.Log($"[DoorSegment] Недостаточно шкалы! Есть: {scale:F2}, нужно: {requiredScale:F2}");
                // Останавливаем игру
                ScoreManager.Instance?.OnDoorFailed();
            }
        }

        void Update()
        {
            if (_isOpen)
            {
                if (leftDoor != null)
                    leftDoor.localRotation = Quaternion.Lerp(leftDoor.localRotation, _leftOpen, Time.deltaTime * openSpeed);
                if (rightDoor != null)
                    rightDoor.localRotation = Quaternion.Lerp(rightDoor.localRotation, _rightOpen, Time.deltaTime * openSpeed);
            }
        }

        /// <summary>
        /// Может ли игрок пройти (дверь открыта)?
        /// </summary>
        public bool IsOpen => _isOpen;
    }
}
