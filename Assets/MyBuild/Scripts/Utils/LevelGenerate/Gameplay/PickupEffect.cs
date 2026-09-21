using UnityEngine;
using System.Collections;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    /// <summary>
    /// Партикл-эффект при сборе пикапа. Маленькая вспышка.
    /// </summary>
    public class PickupEffect : MonoBehaviour
    {
        [Tooltip("Префаб партикл-системы (из Addressables или прямой)")]
        public GameObject particlePrefab;

        [Tooltip("Длительность эффекта до удаления")]
        public float duration = 1f;

        /// <summary>Запускает эффект в точке сбора.</summary>
        public void PlayAt(Vector3 worldPos)
        {
            if (particlePrefab == null) return;

            var fx = Instantiate(particlePrefab, worldPos, Quaternion.identity);
            var ps = fx.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();

            Destroy(fx, duration);
        }
    }
}
