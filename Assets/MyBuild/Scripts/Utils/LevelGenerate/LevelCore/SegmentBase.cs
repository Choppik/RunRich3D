using UnityEngine;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    /// <summary>
    /// База для дороги и двери. Хранит точки входа/выхода и тип сегмента.
    /// </summary>
    public abstract class SegmentBase : MonoBehaviour
    {
        public enum SegmentType { Road, Door, End }

        [Header("Connection Points")]
        [Tooltip("Пустой объект в начале сегмента (куда цепляется предыдущий)")]
        public Transform entryPoint;

        [Tooltip("Пустой объект в конце сегмента (откуда начинается следующий)")]
        public Transform exitPoint;

        [Header("Type")]
        public SegmentType type = SegmentType.Road;

        /// <summary>
        /// Позиция и поворот, чтобы выровнять entry этого сегмента с worldPos.
        /// </summary>
        public void AlignEntryTo(Vector3 worldPos, Quaternion worldRot)
        {
            // Смещаем весь объект так, чтобы entryPoint оказался в worldPos с worldRot
            Transform t = transform;
            Vector3 delta = worldPos - entryPoint.position;
            t.position += delta;

            // Поворот: разница между желаемым и текущим поворотом entry
            Quaternion rotDelta = worldRot * Quaternion.Inverse(entryPoint.rotation);
            t.rotation = rotDelta * t.rotation;
        }

        /// <summary>
        /// Данные для позиционирования следующего сегмента.
        /// </summary>
        public void GetExitData(out Vector3 pos, out Quaternion rot)
        {
            pos = exitPoint.position;
            rot = exitPoint.rotation;
        }
    }
}
