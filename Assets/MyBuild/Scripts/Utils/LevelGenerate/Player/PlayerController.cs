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
        [SerializeField] private float _forwardSpeed = 0.5f; // скорость движения вперед
        [SerializeField] private float _steerSpeed = 10f; // скорость движения в бок
        [SerializeField] private float _maxSteerOffset = 0.18f; // макс. отклонение от центра дороги
        [SerializeField] private float _turnSpeed = 5000f;      // скорость доворота в направлении дороги = 2f

        [Header("Настройки движения")]
        [Tooltip("Смещение границ дороги. 0 = полная ширина, 0.2 = игрок двигается в пределах 80% ширины")]
        [Range(0f, 1f)]
        [SerializeField] private float _roadWidthOffset = 0.2f;

        [Header("Input")]
        [SerializeField] private bool _useMouse = true;
        [SerializeField] private float _mouseSensitivity = 0.01f;

        private float _currentSteer = 0f;
        private float _targetSteer = 0f;
        private bool _isMoving = false;
        private bool _isInputActive = false;
        private Vector2 _lastInputPos;
        private RoadSegment _currentRoad;
        private RoadSegment _lastRoad;
        public Observable<RoadSegment> RoadSet => _roadSet;

        private readonly Subject<RoadSegment> _roadSet = new();
        private readonly CompositeDisposable _compositeDisposable = new();

        // Для тач-ввода: храним «последнее активное касание», чтобы
        // вести его от начала жеста до конца, а не брать случайный палец
        private int _activeFingerId = -1;

        private void Awake()
        {
            // EnhancedTouch нужно явно включить — без этого касания не будут приходить
            EnhancedTouchSupport.Enable();
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

        private void OnTriggerEnter(Collider other)
        {
            var road = other.GetComponentInParent<RoadSegment>();
            if (road != null && road != _currentRoad)
            {
                SetRoad(road, false);
            }
        }   

        /// <summary>
        /// Обработчик нажатия по экрану.
        /// </summary>
        private void HandleInput()
        {
            if (_useMouse)
            {
                // Мышь в Input System: текущий указатель есть всегда (в редакторе и на ПК),
                // проверяем на null на всякий случай (например, консоль без мыши)
                var mouse = Mouse.current;
                if (mouse != null)
                {
                    // Аналог Input.GetMouseButton(0) — левая кнопка удерживается
                    if (mouse.leftButton.isPressed)
                    {
                        if (!_isInputActive)
                        {
                            // Жест только начался — запоминаем стартовую позицию,
                            // чтобы первый кадр не давал рывок руления
                            _isInputActive = true;
                            _lastInputPos = mouse.position.ReadValue();
                        }

                        // position — это InputAction, поэтому читаем значение через ReadValue()
                        Vector2 position = mouse.position.ReadValue();
                        Vector2 delta = position - _lastInputPos;

                        _targetSteer += delta.x * _mouseSensitivity;
                        _targetSteer = Mathf.Clamp(_targetSteer, -_maxSteerOffset, _maxSteerOffset);
                        _lastInputPos = position;
                    }
                    else
                    {
                        // Кнопку отпустили — перестаём вилять и сбрасываем состояние жеста
                        _isInputActive = false;
                        _targetSteer = 0f;
                    }
                }
            }

            // Тач: в Input System касания приходят через EnhancedTouch
            foreach (var touch in Touch.activeTouches)
            {
                if (_activeFingerId == -1)
                {
                    // Свободного жеста нет — начинаем отслеживать этот палец
                    _activeFingerId = touch.finger.index;
                }

                if (touch.finger.index != _activeFingerId)
                {
                    // Этот палец не наш — пропускаем (второй палец не рулит)
                    continue;
                }

                // Фазы в новой системе: Began → Moved → Ended/Canceled
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
                {
                    _targetSteer += touch.delta.x * _mouseSensitivity;
                    _targetSteer = Mathf.Clamp(_targetSteer, -_maxSteerOffset, _maxSteerOffset);
                }
                else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                         touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    _targetSteer = 0f;
                    _activeFingerId = -1; // жест завершён — готовы к новому касанию
                }

                // Обработали наш палец — дальше по коллекции не идём
                break;
            }
        }

        /// <summary>
        /// Движение игрока.
        /// </summary>
        private void Move()
        {
            if (_currentRoad == null) return;

            // 1. Руление
            _currentSteer = Mathf.Lerp(_currentSteer, _targetSteer, Time.deltaTime * _steerSpeed);

            Vector3 roadForward = GetRoadForward();
            if (roadForward.sqrMagnitude < 0.001f) return;

            // Синий: куда должна смотреть дорога (цель)
            Debug.DrawRay(transform.position, roadForward.normalized * 6f, Color.blue, 0.1f);
            // Зеленый: куда реально смотрит игрок (текущее состояние)
            Debug.DrawRay(transform.position, transform.forward * 6f, Color.green, 0.1f);

            Vector3 roadRight = Vector3.Cross(Vector3.up, roadForward);

            // 2. Целевая ротация (куда должна смотреть дорога)
            Quaternion targetRotation = Quaternion.LookRotation(roadForward, Vector3.up);

            // 3. ПЛАВНЫЙ ПОВОРОТ (Работает всегда: и на стыке, и внутри сегмента)
            // Игрок сохраняет инерцию текущего поворота и плавно стремится к targetRotation.
            float maxRadiansThisFrame = _turnSpeed * Mathf.Deg2Rad * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxRadiansThisFrame);

            // 4. Движение
            transform.position += roadForward * (_forwardSpeed * Time.deltaTime);
            transform.position += roadRight * (_currentSteer * _steerSpeed * Time.deltaTime);

            // 5. Границы
            Vector3 localPos = _currentRoad.transform.InverseTransformPoint(transform.position);
            localPos.x = Mathf.Clamp(localPos.x, _currentRoad.GetLeftBound(), _currentRoad.GetRightBound());
            transform.position = _currentRoad.transform.TransformPoint(localPos);

            // Обновляем _lastRoad ТОЛЬКО для отладки или других механик, 
            // но НЕ для управления вращением!
            _lastRoad = _currentRoad;
        }

        /// <summary>
        /// Направление движения дороги. Ось сегментов направлена против движения
        /// генерации (так настроена генерация), поэтому forward инвертируется.
        /// </summary>
        private Vector3 GetRoadForward()
        {
            return _currentRoad.GetMoveDirectionAt(transform.position);
        }

        public void AlignToRoad()
        {
            if (_currentRoad == null) return;

            Vector3 roadForward = GetRoadForward();

            if (roadForward.sqrMagnitude < 0.001f) return;

            transform.rotation = Quaternion.LookRotation(roadForward, Vector3.up);
        }

        /// <summary>
        /// Устанавливает текущий сегмент дороги. Также можно вызвать извне,
        /// чтобы задать дорогу до старта движения.
        /// </summary>
        /// <param name="road">Сегмент дороги.</param>
        /// <param name="snapToRoadDirection">Мгновенно развернуть игрока вдоль дороги.</param>
        public void SetRoad(RoadSegment road, bool snapToRoadDirection = false)
        {
            _currentRoad = road;

            if (snapToRoadDirection)
            {
                AlignToRoad();
            }

            _roadSet.OnNext(road);
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
