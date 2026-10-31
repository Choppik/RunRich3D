using MyBuild.Scripts.Game.Common;
using UnityEngine;

namespace MyBuild.Scripts.Game.Components.LevelGenerate
{
    /// <summary>
    /// Смена меша персонажа по тиру.
    /// </summary>
    public class MeshSwapper : MonoBehaviour
    {
        [System.Serializable]
        public struct TierMeshEntry
        {
            [SerializeField] private TierType _tier;          // Выпадающий список в инспекторе
            [SerializeField] private GameObject _meshObject;  // Сюда перетаскиваем дочерний объект с мешем

            public readonly TierType Tier => _tier;
            public readonly GameObject MeshObject => _meshObject;
        }

        [Header("Настройки мешей по тирам")]
        [SerializeField] private TierMeshEntry[] _tierMeshes;

        private void Start()
        {
            UpdateMeshVisibility(TierType.Poor);
        }

        /// <summary>
        /// Смена тира.
        /// </summary>
        public void SetTier(TierType targetTier)
        {
            UpdateMeshVisibility(targetTier);
        }

        /// <summary>
        /// Обновление меша.
        /// </summary>
        private void UpdateMeshVisibility(TierType targetTier)
        {
            bool found = false;

            foreach (var entry in _tierMeshes)
            {
                if (entry.MeshObject == null) continue;

                if (entry.Tier == targetTier)
                {
                    entry.MeshObject.SetActive(true);
                    found = true;
                }
                else
                {
                    entry.MeshObject.SetActive(false);
                }
            }

            if (!found)
            {
                Debug.LogWarning($"Не найден объект для тира: {targetTier}. Все меши выключены.");
            }
        }
    }
}
