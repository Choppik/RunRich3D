using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    /// <summary>
    /// Расставляет декор на пустых точках дороги. Случайно или всегда.
    /// </summary>
    public class DecorationPlacer : MonoBehaviour
    {
        [Tooltip("Адреса декораций в Addressables")]
        public string[] decorationAddresses = { "decor/bush", "decor/carpet", "decor/lantern" };

        [Tooltip("Вероятность размещения на каждой точке (0..1)")]
        public float placeChance = 0.5f;

        [Tooltip("Случайный декор или по порядку")]
        public bool randomDecor = true;

        private readonly List<GameObject> _placed = new();
        private readonly List<AsyncOperationHandle<GameObject>> _handles = new();

        public void PlaceDecorations()
        {
            StartCoroutine(PlaceCoroutine());
        }

        IEnumerator PlaceCoroutine()
        {
            var road = GetComponent<RoadSegment>();
            if (road == null || road.decorationPoints == null) yield break;

            for (int i = 0; i < road.decorationPoints.Length; i++)
            {
                if (Random.value > placeChance) continue;

                string addr = randomDecor
                    ? decorationAddresses[Random.Range(0, decorationAddresses.Length)]
                    : decorationAddresses[i % decorationAddresses.Length];

                var handle = Addressables.LoadAssetAsync<GameObject>(addr);
                yield return handle;

                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                {
                    _handles.Add(handle);
                    var obj = Instantiate(handle.Result, road.decorationPoints[i]);
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localRotation = Quaternion.identity;
                    _placed.Add(obj);
                }
            }
        }

        public void ClearDecorations()
        {
            foreach (var obj in _placed)
                if (obj != null) Destroy(obj);
            _placed.Clear();

            foreach (var h in _handles)
                if (h.IsValid()) Addressables.Release(h);
            _handles.Clear();
        }

        void OnDestroy()
        {
            ClearDecorations();
        }
    }
}
