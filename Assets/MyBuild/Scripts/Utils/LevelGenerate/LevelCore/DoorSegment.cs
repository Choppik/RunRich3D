using UnityEngine;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    /// <summary>
    /// Дверь между сегментами дороги. Открывается только если шкала достаточна.
    /// </summary>
    public class DoorSegment : SegmentBase
    {
        [Header("Door")]
        public int tier = 0;

        [Tooltip("Минимальная шкала (0..1), чтобы дверь открылась")]
        public float requiredScale = 0.3f;

        [Tooltip("Аниматор двери (или Transform для вращения/сдвига)")]
        public Transform leftDoor;
        public Transform rightDoor;

        [Tooltip("Триггер перед дверью")]
        public Collider doorTrigger;

        [Header("Visuals")]
        public float openAngle = 90f;
        public float openSpeed = 2f;

        private bool _isOpen = false;
        private bool _playerReached = false;
        private Quaternion _leftClosed;
        private Quaternion _rightClosed;
        private Quaternion _leftOpen;
        private Quaternion _rightOpen;

        [HideInInspector] public bool IsFinalDoor = false;

        void Awake()
        {
            type = SegmentType.Door;
        }

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
                if (IsFinalDoor)
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

        public void SetTier(int newTier)
        {
            tier = Mathf.Clamp(newTier, 0, 2);
            // Смена визуала двери по тиру
        }

        /// <summary>Может ли игрок пройти (дверь открыта)?</summary>
        public bool IsOpen => _isOpen;
    }
}
