using UnityEngine;

namespace MyBuild.Scripts.Game.Components.LevelGenerate
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Настройки")]
        [SerializeField] private Transform _target;

        // Смещение позиции камеры относительно игрока (сзади и чуть выше)
        [SerializeField] private Vector3 _positionOffset = new(0f, 1.5f, -2f);

        // Смещение ТОЧКИ ВЗГЛЯДА относительно игрока (куда именно смотрит камера)
        [SerializeField] private Vector3 _lookOffset = new(0f, 1f, -1f);

        // Скорость следования камеры за целью
        [SerializeField] private float _rotationSmooth = 100f;

        private void LateUpdate()
        {
            if (_target == null) return;

            // 1. Позиция камеры: считаем жестко
            // Камера стоит позади и выше игрока
            Vector3 desiredPosition = _target.position + _target.TransformDirection(_positionOffset);
            transform.position = desiredPosition;

            // 2. Точка, куда должна смотреть камера
            // Мы берем центр игрока и поднимаем точку взгляда на _lookOffset.Y
            Vector3 lookTarget = _target.position + _target.TransformDirection(_lookOffset);

            // 3. Расчет вращения
            Vector3 lookDirection = lookTarget - transform.position;

            if (lookDirection.sqrMagnitude < 0.001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, _target.up);

            // Плавный поворот (чтобы не было резких скачков при повороте дороги)
            float maxAngleThisFrame = _rotationSmooth * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxAngleThisFrame);
        }

        public void SetTarget(Transform target)
        {
            _target = target;

            Vector3 immediatePosition = _target.position + _target.TransformDirection(_positionOffset);
            transform.position = immediatePosition;

            // Сразу считаем точку взгляда
            Vector3 immediateLookTarget = _target.position + _target.TransformDirection(_lookOffset);
            Vector3 immediateLookDirection = immediateLookTarget - transform.position;

            if (immediateLookDirection.sqrMagnitude > 0.001f)
            {
                // Сразу ставим правильный поворот (без сглаживания)
                Quaternion immediateRotation = Quaternion.LookRotation(immediateLookDirection, _target.up);
                transform.rotation = immediateRotation;
            }
        }
    }
}
