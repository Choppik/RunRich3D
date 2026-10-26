using UnityEngine;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Настройки")]
        [SerializeField] private Transform _target;

        // Смещение: 0 по X, 5 по Y, -7 по Z (за спиной)
        [SerializeField] private Vector3 _offset = new Vector3(0f, 5f, -7f);

        // Скорость движения камеры. 
        // Для раннеров лучше использовать Lerp вместо SmoothDamp, чтобы не было инерции при резких остановках/поворотах.
        // Значение 10-15 дает очень плотное следование без рывков.
        [Range(0.1f, 20f)]
        [SerializeField] private float _followSpeed = 10f;

        // Чувствительность вращения. 
        // ВАЖНО: При резком повороте игрока (5000 speed) этот параметр нужно либо уменьшить, 
        // либо (лучше) убрать зависимость от Time.deltaTime в пользу экспоненциального сглаживания.
        [Range(0.1f, 10f)]
        [SerializeField] private float _rotationSmooth = 10f;

        private void LateUpdate()
        {
            if (_target == null) return;

            // 1. Вычисляем идеальную позицию ЗА СПИНОЙ игрока
            Vector3 desiredPosition = _target.position + _target.TransformDirection(_offset);

            // 2. ИСПРАВЛЕНИЕ ПОЗИЦИИ:
            // Заменяем SmoothDamp на Lerp. SmoothDamp создает инерцию (пружину), которая дергается 
            // при резкой смене направления. Lerp дает предсказуемое, линейное сглаживание.
            // Формула: current + (target - current) * speed
            transform.position = Vector3.Lerp(transform.position, desiredPosition, _followSpeed * Time.deltaTime);

            // 3. ИСПРАВЛЕНИЕ ВРАЩЕНИЯ:
            // Убираем прямой Slerp с Time.deltaTime. Вместо этого используем подход, 
            // который не зависит от скачков deltaTime и дает более естественное "прилипание".

            Vector3 lookDirection = _target.position - transform.position;

            // Защита от деления на ноль, если камера и игрок в одной точке
            if (lookDirection.sqrMagnitude < 0.001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, _target.up);

            // Используем Quaternion.RotateTowards вместо Slerp для вращения.
            // RotateTowards ограничивает угол поворота за кадр, что предотвращает "перелет" 
            // и дергание при резких изменениях вектора lookDirection.
            float maxAngleThisFrame = _rotationSmooth * Time.deltaTime * 10f; // Коэффициент подобран для плавности
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxAngleThisFrame);
        }

        public void SetTarget(Transform target) => _target = target;
    }
}

