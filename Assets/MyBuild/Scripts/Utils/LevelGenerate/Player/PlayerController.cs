using R3;
using UnityEngine;
using UnityEngine.InputSystem;          // основное API новой системы ввода
using UnityEngine.InputSystem.EnhancedTouch; // расширенная работа с касаниями

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    /// <summary>
    /// Управление персонажем: движение активируется через StartMovement(),
    /// едет вдоль направления дороги, смещается вбок только в пределах её ширины.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _forwardSpeed = 3f;
        [SerializeField] private float _maxSteerOffset = 0.7f; // макс. смещение от центра (в метрах)
        [SerializeField] private float _steerSmoothTime = 0.15f; // время сглаживания руления (сек)
        [SerializeField] private float _turnSpeed = 5000f;       // град/сек

        [Header("Input")]
        [SerializeField] private bool _useMouse = true;
        [SerializeField] private float _mouseSensitivity = 0.01f;

        private float _targetSteer = 0f; // от -1 до 1
        private float _currentSteer = 0f; // от -1 до 1
        private bool _isMoving = false;
        private bool _isInputActive = false;
        private Vector2 _lastInputPos;
        private float _currentSteerVelocity = 0f;
        private RoadSegmentSpline _currentSpline;
        private RoadSegmentSpline _nextSpline;
        private float _distanceAlongSpline = 0f;
        private float _splineLength = 0f;
        private readonly CompositeDisposable _compositeDisposable = new();

        // Для тач-ввода: храним «последнее активное касание», чтобы
        // вести его от начала жеста до конца, а не брать случайный палец
        private int _activeFingerId = -1;

        private void Awake()
        {
            // EnhancedTouch нужно явно включить — без этого касания не будут приходить
            EnhancedTouchSupport.Enable();
        }

        private void OnTriggerEnter(Collider other)
        {
            var road = other.GetComponent<RoadSegmentSpline>();
            if (road != null && road != _currentSpline)
            {
                _nextSpline = road;
            }
        }

        private void OnDestroy()
        {
            _compositeDisposable.Dispose();
            // Симметрично выключаем, чтобы не оставлять подсистему включённой
            // после уничтожения единственного потребителя
            EnhancedTouchSupport.Disable();
        }

        private void Update()
        {
            if (!_isMoving) return;

            HandleInput();
            Move();
        }

        /// <summary>
        /// Обработчик нажатия по экрану.
        /// </summary>
        private void HandleInput()
        {
            if (!_isMoving) return;

            float inputDelta = 0f;
            bool hasInput = false;

            if (_useMouse)
            {
                var mouse = Mouse.current;
                if (mouse != null && mouse.leftButton.isPressed)
                {
                    hasInput = true;
                    if (!_isInputActive)
                    {
                        _isInputActive = true;
                        _lastInputPos = mouse.position.ReadValue();
                    }

                    Vector2 position = mouse.position.ReadValue();
                    Vector2 delta = position - _lastInputPos;
                    inputDelta = delta.x * _mouseSensitivity;
                    _lastInputPos = position;
                }
                else
                {
                    _isInputActive = false;
                }
            }

            foreach (var touch in Touch.activeTouches)
            {
                if (_activeFingerId == -1)
                {
                    _activeFingerId = touch.finger.index;
                }

                if (touch.finger.index != _activeFingerId) continue;

                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
                {
                    hasInput = true;
                    inputDelta = touch.delta.x * _mouseSensitivity;
                }
                else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                         touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    _activeFingerId = -1;
                }

                break;
            }

            // Сдвигаем целевую позицию, но сразу жёстко ограничиваем по ширине дороги
            if (hasInput)
            {
                _targetSteer += inputDelta;
                _targetSteer = Mathf.Clamp(_targetSteer, -_maxSteerOffset, _maxSteerOffset);
            }
        }

        /// <summary>
        /// Движение игрока.
        /// </summary>
        private void Move()
        {
            if (_currentSpline == null) return;

            if (_splineLength <= 0f)
            {
                _splineLength = _currentSpline.GetLength();
                if (_splineLength <= 0f) return;
            }

            float t = _distanceAlongSpline / _splineLength;

            if (_distanceAlongSpline >= _splineLength)
            {
                if (_nextSpline != null)
                {
                    _distanceAlongSpline -= _splineLength;
                    _currentSpline = _nextSpline;
                    _nextSpline = null;
                    _splineLength = _currentSpline.GetLength();
                    if (_splineLength <= 0f) return;

                    t = _distanceAlongSpline / _splineLength;
                }
                else
                {
                    _isMoving = false;
                    return;
                }
            }

            Vector3 posOnSpline = _currentSpline.GetPoint(t);
            Vector3 tangent = _currentSpline.GetTangent(t).normalized;

            // Инерция: плавно следуем за целевой позицией
            _currentSteer = Mathf.SmoothDamp(_currentSteer, _targetSteer, ref _currentSteerVelocity, _steerSmoothTime);

            // ВАЖНО: жёстко ограничиваем итоговое смещение, чтобы инерция не вынесла за край
            _currentSteer = Mathf.Clamp(_currentSteer, -_maxSteerOffset, _maxSteerOffset);

            Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;
            Vector3 finalPos = posOnSpline + right * _currentSteer;

            float moveDist = _forwardSpeed * Time.deltaTime;
            _distanceAlongSpline += moveDist;

            Quaternion targetRot = Quaternion.LookRotation(tangent, Vector3.up);
            float maxRad = _turnSpeed * Mathf.Deg2Rad * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, maxRad);

            transform.position = finalPos;
        }

        public void SetStartRoad(RoadSegmentSpline segmentSpline)
        {
            _currentSpline = segmentSpline;
        }

        /// <summary>
        /// Запускает движение.
        /// </summary>
        public void StartMovement()
        {
            _isMoving = true;
        }

        /// <summary>
        /// Останавливает движение.
        /// </summary>
        public void StopMovement()
        {
            _isMoving = false;
        }
    }
}
