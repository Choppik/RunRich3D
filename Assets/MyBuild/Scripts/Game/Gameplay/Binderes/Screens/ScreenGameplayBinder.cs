using MyBuild.Scripts.Utils.MVP.UI;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MyBuild.Scripts.Game.Gameplay.Binderes
{
    /// <summary>
    /// Биндер окна геймплея.
    /// </summary>
    public class ScreenGameplayBinder : WindowBinder
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 300f;
        //[SerializeField] private float rotationSmooth = 20f;      // для плавного поворота (если захочешь Slerp)
        [SerializeField] private float deadzone = 0.15f;
        //[SerializeField] private float mouseSensitivity = 0.002f;  // тут маленький множитель, т.к. delta в пикселях

        [SerializeField] private Vector3 _initialOffset;  // смещение, которое вы подобрали в редакторе

        private Camera _cam;
        private Vector3 _storedRotation; // сохраняем ваш «красивый» поворот из редактора

        private CharacterController _controller;
        private bool _isPaused;
        private GameObject _player;
        private int _groundLayerMask;

        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                _isPaused = value;
                if (_isPaused)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    LockCursorIfNeeded();
                }
            }
        }

        void Awake()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            _cam = Camera.main.GetComponent<Camera>();
            _cam.orthographic = true;

            // Запоминаем, как вы повернули камеру в редакторе
            _storedRotation = Camera.main.transform.eulerAngles;

            // Вычисляем смещение относительно персонажа на основе текущей позиции камеры
            _initialOffset = Camera.main.transform.position - _player.transform.position;

            _controller = _player.GetComponent<CharacterController>();

            // Пока на один слой ориентируется вращение.
            _groundLayerMask = LayerMask.GetMask("Ground");
        }

        void Start()
        {
            LockCursorIfNeeded();
        }

        void LockCursorIfNeeded()
        {
            bool isPc = Application.platform == RuntimePlatform.WindowsPlayer || 
                        Application.platform == RuntimePlatform.WindowsEditor ||
                        Application.platform == RuntimePlatform.OSXPlayer;

            if (isPc)
            {
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void LateUpdate()
        {
            if (_player == null) return;

            // Двигаем камеру вслед за персонажем, сохраняя смещение
            Camera.main.transform.position = _player.transform.position + _initialOffset;

            // Жёстко возвращаем ваш ракурс (чтобы не сбился поворот)
            Camera.main.transform.eulerAngles = _storedRotation;
        }

        void Update()
        {
            if (_isPaused) return;

            HandleRotation();   // Только поворот за курсором
            HandleMovement();   // Только движение по кнопкам
        }

        // --- ПОВОРОТ ЗА КУРСОРОМ (работает на любой высоте и с уклонами) ---
        void HandleRotation()
        {
            var mouse = Mouse.current;
            bool isPc = Application.platform is RuntimePlatform.WindowsPlayer or
                                  RuntimePlatform.OSXPlayer or
                                  RuntimePlatform.WindowsEditor;

            if (!isPc || mouse == null || Camera.main == null) return;

            Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());

            // Physics.Raycast бьёт в реальные коллайдеры, а не в воображаемую плоскость Y=0
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _groundLayerMask))
            {
                Vector3 pointOnGround = hit.point;
                Vector3 lookDirection = (pointOnGround - _player.transform.position).normalized;

                if (lookDirection.magnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    // Оставляем только поворот вокруг Y (чтобы не заваливался вверх/вниз)
                    _player.transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
                }
            }
        }

        // --- ДВИЖЕНИЕ ОТНОСИТЕЛЬНО ВЗГЛЯДА (и стены не дают пройти) ---
        void HandleMovement()
        {
            Vector2 moveInput = Vector2.zero;

            // --- Сбор ввода (без изменений) ---
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                float h = 0f, v = 0f;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) v += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) v -= 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) h -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) h += 1f;
                moveInput += new Vector2(h, v).normalized;
            }

            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Vector2 stickInput = gamepad.leftStick.ReadValue();
                if (Mathf.Abs(stickInput.x) < deadzone) stickInput.x = 0f;
                if (Mathf.Abs(stickInput.y) < deadzone) stickInput.y = 0f;
                if (stickInput.magnitude >= 0.1f)
                    moveInput += stickInput.normalized;
            }

            moveInput = moveInput.normalized;
            if (moveInput.magnitude < 0.01f) return;

            // --- Подготовка направления движения (без изменений) ---
            Vector3 forward = _player.transform.forward;
            Vector3 right = _player.transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;

            Vector3 sphereOrigin = _player.transform.position - _controller.radius * 0.9f * Vector3.up;

            if (Physics.SphereCast(sphereOrigin, _controller.radius, Vector3.down, out RaycastHit groundHit, Mathf.Infinity, _groundLayerMask))
            {
                float dropDistance = _player.transform.position.y - groundHit.point.y;
                if (dropDistance > 0.01f) // Двигаем только если есть реальный зазор
                {
                    _controller.Move(new Vector3(0f, -dropDistance, 0f));
                }
            }

            // --- ОСНОВНОЕ ДВИЖЕНИЕ ---
            _controller.Move(moveSpeed * Time.deltaTime * moveDirection);
        }

        /// <summary>
        /// Установка текста в интерфейс.
        /// </summary>
        /// <param name="textLevel">Текст уровня.</param>
        /// <param name="textStage">Текст стадии.</param>
        public void SetTextUI(string textLevel, string textStage)
        {
        }


        private void OnEnable()
        {
        }

        private void OnDisable()
        {

        }

        /// <summary>
        /// Обработчик события нажатия на кнопку "Пауза".
        /// </summary>
        private void OnCloseRequestButtonClicked()
        {
            _pauseGameRequest?.OnNext(Unit.Default);
        }



        /// <summary>
        /// Событие запроса для открытия окна "Пауза".
        /// </summary>
        public Observable<Unit> PauseGameRequest => _pauseGameRequest;

        private readonly Subject<Unit> _pauseGameRequest = new();

        /// <summary>
        /// Событие запроса для открытия окна "Поражение".
        /// </summary>
        public Observable<Unit> LoseRequest => _loseRequest;

        private readonly Subject<Unit> _loseRequest = new();

        /// <summary>
        /// Событие клика по элементу.
        /// </summary>
        public Observable<Unit> ClickElementRequest => _clickElementRequest;

        private readonly Subject<Unit> _clickElementRequest = new();

        /// <summary>
        /// Событие выйгрыша.
        /// </summary>
        public Observable<int> LevelUPRequest => _levelUPRequest;
        
        private readonly Subject<int> _levelUPRequest = new();
    }
}
