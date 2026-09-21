using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MyBuild.Scripts.Utils
{
    /// <summary>
    /// Провайдер drag-ввода.
    /// - Предназначен для двух режимов:
    ///   1) UI-режим: внешние UI-компоненты (например, DragStartButton или EventTrigger) вызывают BeginDrag/Drag/EndDrag.
    ///   2) Автодетект режим: если enableAutoDetect = true, провайдер сам отслеживает касания/мышь и генерирует события.
    /// 
    /// Подписки:
    ///   DragInputProvider.Instance.OnBeginDrag += ...
    ///   DragInputProvider.Instance.OnDrag += ...
    ///   DragInputProvider.Instance.OnEndDrag += ...
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class DragInputProvider : MonoBehaviour
    {
        public static DragInputProvider Instance { get; private set; }

        /// <summary>События в экранных координатах (пиксели)</summary>
        public event Action<Vector2> OnBeginDrag;
        public event Action<Vector2> OnDrag;
        public event Action<Vector2> OnEndDrag;

        [Header("Auto detect (touch / mouse)")]
        [Tooltip("Если true — провайдер сам будет отслеживать touch/mouse и генерировать события")]
        public bool enableAutoDetect = true;

        // внутренняя логика детекции
        private bool _isDragging;
        private int _touchId = -1;
        private Vector2 _lastPos;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            if (!enableAutoDetect) return;

            // Touch на мобильных устройствах
            if (Input.touchSupported && Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0); // используем первый палец
                var pos = touch.position;

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        _isDragging = true;
                        _touchId = touch.fingerId;
                        _lastPos = pos;
                        OnBeginDrag?.Invoke(pos);
                        break;
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (_isDragging && touch.fingerId == _touchId)
                        {
                            OnDrag?.Invoke(pos);
                            _lastPos = pos;
                        }
                        break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (_isDragging && touch.fingerId == _touchId)
                        {
                            OnEndDrag?.Invoke(pos);
                            _isDragging = false;
                            _touchId = -1;
                        }
                        break;
                }
                return;
            }

            // Мышь в редакторе / standalone
            if (Input.mousePresent)
            {
                var mpos = (Vector2)Input.mousePosition;
                if (Input.GetMouseButtonDown(0))
                {
                    // guard: если курсор над UI элементы и вы хотите игнорировать, проверяйте EventSystem.current.IsPointerOverGameObject()
                    _isDragging = true;
                    _lastPos = mpos;
                    OnBeginDrag?.Invoke(mpos);
                }
                else if (Input.GetMouseButton(0))
                {
                    if (_isDragging)
                    {
                        OnDrag?.Invoke(mpos);
                        _lastPos = mpos;
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    if (_isDragging)
                    {
                        OnEndDrag?.Invoke(mpos);
                        _isDragging = false;
                    }
                }
            }
        }

        // --- Методы для вызова из UI (DragStartButton и др.) ---

        /// <summary>Вызывать при PointerDown / TouchBegin из UI</summary>
        public void BeginDrag(Vector2 screenPosition)
        {
            _isDragging = true;
            _lastPos = screenPosition;
            OnBeginDrag?.Invoke(screenPosition);
        }

        /// <summary>Вызывать при PointerMove / Drag из UI</summary>
        public void Drag(Vector2 screenPosition)
        {
            if (!_isDragging) return;
            _lastPos = screenPosition;
            OnDrag?.Invoke(screenPosition);
        }

        /// <summary>Вызывать при PointerUp / TouchEnd из UI</summary>
        public void EndDrag(Vector2 screenPosition)
        {
            if (!_isDragging) return;
            _isDragging = false;
            OnEndDrag?.Invoke(screenPosition);
        }
    }
}
