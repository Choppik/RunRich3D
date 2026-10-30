using System;
using UnityEngine;

namespace MyBuild.Scripts.Game.Components.LevelGenerate.LevelCore
{
    public class PickupView : MonoBehaviour
    {
        [Header("Вращение модели")]
        [SerializeField] private Transform _model;
        [SerializeField, Range(0f, 720f)] private float _spinSpeed = 180f; // градусов в секунду

        [Header("Канвасы (биллборд только по горизонтали)")]
        [SerializeField] private Transform _backgroundCanvas;
        [SerializeField] private Transform _labelCanvas;

        private Camera _camera;

        private void Awake()
        {
            // Кэшируем камеру один раз — Camera.main в Update это дорогой поиск
            _camera = Camera.main;
        }

        private void LateUpdate()
        {
            SpinModel();
            FaceCanvases();
        }

        private void SpinModel()
        {
            if (_model == null) return;

            // Крутим только вокруг вертикальной оси, локально — не трогаем наклон префаба
            _model.Rotate(0f, _spinSpeed * Time.deltaTime, 0f, Space.Self);
        }

        private void FaceCanvases()
        {
            if (_camera == null)
            {
                // Камера могла появиться позже (например, спавнится с игроком)
                _camera = Camera.main;
                if (_camera == null) return;
            }

            if (_backgroundCanvas != null)
                FaceCameraHorizontal(_backgroundCanvas);

            if (_labelCanvas != null)
                FaceCameraHorizontal(_labelCanvas);
        }

        /// <summary>
        /// Поворачивает трансформ на камеру, но только вокруг оси Y:
        /// наклон вперёд/назад (X) и крен (Z) остаются нулевыми.
        /// </summary>
        private void FaceCameraHorizontal(Transform target, float yawOffset = 0f)
        {
            Vector3 dir = _camera.transform.position - target.position;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.0001f)
                return;

            // Инвертируем: лицевая сторона канваса — это -Z, она должна смотреть на камеру
            Quaternion look = Quaternion.LookRotation(-dir.normalized, Vector3.up);

            if (Mathf.Abs(yawOffset) > 0.01f)
                look *= Quaternion.Euler(0f, yawOffset, 0f);

            target.rotation = look;
        }
    }
}
