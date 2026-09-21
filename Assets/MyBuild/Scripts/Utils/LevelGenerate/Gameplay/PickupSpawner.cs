using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    /// <summary>
    /// Спавн пикапов на дороге. Паттерны: линия, сетка, полоса.
    /// Работает с PoolManager. Вешается на RoadSegment программно.
    /// </summary>
    public class PickupSpawner : MonoBehaviour
    {
        public enum Preset { None, Line, Grid, Stripe }

        public Preset preset = Preset.Line;
        public int count = 5;
        public float spacing = 1.2f;
        public Vector2 gridSize = new(3, 2);

        [Tooltip("Префабы положительных пикапов (с PickupInitializer)")]
        public GameObject[] positivePrefabs;
        [Tooltip("Префабы отрицательных пикапов")]
        public GameObject[] negativePrefabs;

        [Tooltip("Префаб партикл-эффекта при сборе")]
        public GameObject particleEffectPrefab;

        private readonly List<GameObject> _active = new();
        private PickupEffect _effect;

        void Awake()
        {
            _effect = gameObject.AddComponent<PickupEffect>();
            _effect.particlePrefab = particleEffectPrefab;

            // Случайный пресет
            var presets = new[] { Preset.Line, Preset.Grid, Preset.Stripe };
            preset = presets[Random.Range(0, presets.Length)];
        }

        public void SpawnRandom()
        {
            switch (preset)
            {
                case Preset.Line: SpawnLine(); break;
                case Preset.Grid: SpawnGrid(); break;
                case Preset.Stripe: SpawnStripe(); break;
            }
        }

        void SpawnLine()
        {
            for (int i = 0; i < count; i++)
            {
                var prefab = ChooseRandomPrefab();
                if (prefab == null) continue;

                var obj = PoolManager.Instance.Spawn(prefab);
                PoolManager.Instance.Despawn(obj);
                if (obj == null) continue;

                obj.transform.SetParent(transform, true);
                obj.transform.localPosition = new Vector3(0, 0.5f, i * spacing + 2f);
                obj.SetActive(true);
                _active.Add(obj);
            }
        }

        void SpawnGrid()
        {
            int idx = 0;
            for (int y = 0; y < (int)gridSize.y; y++)
            {
                for (int x = 0; x < (int)gridSize.x; x++)
                {
                    if (idx++ >= count) break;
                    var prefab = ChooseRandomPrefab();
                    if (prefab == null) continue;

                    var obj = PoolManager.Instance.Spawn(prefab);
                    if (obj == null) continue;

                    obj.transform.SetParent(transform, true);
                    obj.transform.localPosition = new Vector3(
                        (x - gridSize.x / 2f) * spacing, 0.5f, y * spacing + 2f);
                    obj.SetActive(true);
                    _active.Add(obj);
                }
            }
        }

        void SpawnStripe()
        {
            for (int i = 0; i < count; i++)
            {
                var prefab = ChooseRandomPrefab();
                if (prefab == null) continue;

                var obj = PoolManager.Instance.Spawn(prefab);
                if (obj == null) continue;

                obj.transform.SetParent(transform, true);
                obj.transform.localPosition = new Vector3(
                    (i % 2 == 0 ? -1f : 1f) * spacing, 0.5f, i * spacing + 2f);
                obj.SetActive(true);
                _active.Add(obj);
            }
        }

        GameObject ChooseRandomPrefab()
        {
            // 70% positive, 30% negative
            if (Random.value < 0.7f && positivePrefabs != null && positivePrefabs.Length > 0)
                return positivePrefabs[Random.Range(0, positivePrefabs.Length)];
            if (negativePrefabs != null && negativePrefabs.Length > 0)
                return negativePrefabs[Random.Range(0, negativePrefabs.Length)];
            return (positivePrefabs != null && positivePrefabs.Length > 0) ? positivePrefabs[0] : null;
        }

        /// <summary>Очищает все пикапы (возвращает в пул).</summary>
        public void ClearPickups()
        {
            foreach (var p in _active)
            {
                if (p == null) continue;
                p.SetActive(false);
                var obj = PoolManager.Instance.Spawn(p);
                PoolManager.Instance.Despawn(obj);
            }
            _active.Clear();
        }
    }
}
