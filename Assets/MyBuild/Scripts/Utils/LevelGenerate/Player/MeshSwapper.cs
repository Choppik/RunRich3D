using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using System.Collections.Generic;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    /// <summary>
    /// Меняет меши персонажа по шкале. Меши грузятся из Addressables.
    /// </summary>
    public class MeshSwapper : MonoBehaviour
    {
        [Header("Mesh Addresses (по тирам)")]
        public string[] meshAddresses = { "player/mesh_0", "player/mesh_1", "player/mesh_2", "player/mesh_3" };

        [Header("References")]
        [Tooltip("Объект, у которого меняем SkinnedMeshRenderer или MeshFilter")]
        public Transform meshHolder;

        private readonly List<AsyncOperationHandle<GameObject>> _handles = new();
        private GameObject _currentMeshObj;
        private int _currentTier = -1;

        void Start()
        {
            if (meshHolder == null) meshHolder = transform;
            SetMeshTier(0);
        }

        public void SetMeshTier(int tier)
        {
            if (tier == _currentTier) return;
            if (tier < 0 || tier >= meshAddresses.Length) return;

            StartCoroutine(SwapMeshCoroutine(tier));
        }

        IEnumerator SwapMeshCoroutine(int tier)
        {
            string addr = meshAddresses[tier];

            var handle = Addressables.LoadAssetAsync<GameObject>(addr);
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogWarning($"[MeshSwapper] Меш '{addr}' не загружен.");
                yield break;
            }

            _handles.Add(handle);

            // Удаляем старый
            if (_currentMeshObj != null)
                Destroy(_currentMeshObj);

            // Спавним новый
            _currentMeshObj = Instantiate(handle.Result, meshHolder);
            _currentMeshObj.transform.localPosition = Vector3.zero;
            _currentMeshObj.transform.localRotation = Quaternion.identity;
            _currentMeshObj.transform.localScale = Vector3.one;

            _currentTier = tier;
            Debug.Log($"[MeshSwapper] Меш сменён на тир {tier}.");
        }

        void OnDestroy()
        {
            foreach (var h in _handles)
                if (h.IsValid()) Addressables.Release(h);
        }
    }
}
