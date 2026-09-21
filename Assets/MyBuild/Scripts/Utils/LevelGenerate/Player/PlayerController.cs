using UnityEngine;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    /// <summary>
    /// Управление персонажем. Автоматическое движение вперёд + руление удержанием/мышью.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float forwardSpeed = 5f;
        public float steerSpeed = 3f;
        public float maxSteerOffset = 2f; // макс. отклонение от центра дороги

        [Header("Input")]
        public bool useMouse = true;
        public float mouseSensitivity = 0.01f;

        private float _currentSteer = 0f;
        private float _targetSteer = 0f;
        private bool _isMoving = true;
        private bool _isInputActive = false;
        private Vector2 _lastInputPos;
        private RoadSegment _currentRoad;

        void Update()
        {
            if (!_isMoving) return;

            HandleInput();
            Move();
        }

        void HandleInput()
        {
            if (useMouse)
            {
                if (Input.GetMouseButton(0))
                {
                    if (!_isInputActive)
                    {
                        _isInputActive = true;
                        _lastInputPos = Input.mousePosition;
                    }

                    Vector2 delta = (Vector2)Input.mousePosition - _lastInputPos;
                    _targetSteer += delta.x * mouseSensitivity;
                    _targetSteer = Mathf.Clamp(_targetSteer, -maxSteerOffset, maxSteerOffset);
                    _lastInputPos = Input.mousePosition;
                }
                else
                {
                    _isInputActive = false;
                    _targetSteer = 0f; // отпустил — перестаёт вилять
                }
            }

            // Тач (если нужна поддержка мобилок)
            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Moved)
                {
                    _targetSteer += t.deltaPosition.x * mouseSensitivity;
                    _targetSteer = Mathf.Clamp(_targetSteer, -maxSteerOffset, maxSteerOffset);
                }
                else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    _targetSteer = 0f;
                }
            }
        }

        void Move()
        {
            // Плавное руление
            _currentSteer = Mathf.Lerp(_currentSteer, _targetSteer, Time.deltaTime * steerSpeed);

            // Движение вперёд (по локальной оси Z)
            transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime, Space.Self);

            // Руление вбок (по локальной оси X)
            transform.Translate(Vector3.right * _currentSteer * Time.deltaTime * steerSpeed, Space.Self);

            // Ограничение по границам дороги
            if (_currentRoad != null)
            {
                Vector3 localPos = _currentRoad.transform.InverseTransformPoint(transform.position);
                localPos.x = Mathf.Clamp(localPos.x, _currentRoad.GetLeftBound(), _currentRoad.GetRightBound());
                transform.position = _currentRoad.transform.TransformPoint(localPos);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            var road = other.GetComponentInParent<RoadSegment>();
            if (road != null)
            {
                _currentRoad = road;
            }
        }

        /// <summary>Останавливает движение (для финала / поражения).</summary>
        public void StopMovement()
        {
            _isMoving = false;
        }

        /// <summary>Возобновляет движение.</summary>
        public void ResumeMovement()
        {
            _isMoving = true;
        }
    }
}
