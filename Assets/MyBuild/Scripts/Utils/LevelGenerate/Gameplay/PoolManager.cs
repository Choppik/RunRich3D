using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    /// <summary>
    /// Пул объектов с поддержкой Addressables.
    /// Использует PooledItem для надёжного сопоставления объект → ключ пула.
    /// Доступен как синглтон: PoolManager.Instance.Spawn(prefab).
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [System.Serializable]
        public class PoolEntry
        {
            [Tooltip("Ключ пула. Если пустой — используется имя префаба.")]
            public string key;

            [Tooltip("Addressable адрес префаба")]
            public string address;

            [Tooltip("Запасной префаб, если Addressables не настроен")]
            public GameObject fallbackPrefab;

            [Tooltip("Сколько объектов создать заранее")]
            public int initial = 5;
        }

        [Header("Pool Entries")]
        public PoolEntry[] entries;

        // Ключ → очередь свободных объектов
        private readonly Dictionary<string, Queue<GameObject>> _pools = new();

        // Ключ → исходный префаб (для создания новых, когда пул пуст)
        private readonly Dictionary<string, GameObject> _prefabByKey = new();

        // Загруженные через Addressables хендлы (для освобождения)
        private readonly List<AsyncOperationHandle<GameObject>> _handles = new();

        // Множество объектов, которые сейчас "в пуле" (для защиты от двойного Despawn)
        private readonly HashSet<int> _inPoolHashes = new();

        // Объекты, которые сейчас активны (для отладки и массовой очистки)
        private readonly HashSet<GameObject> _activeObjects = new();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        IEnumerator Start()
        {
            foreach (var e in entries)
            {
                GameObject prefab = null;
                string key = null;

                // --- Загрузка префаба ---

                if (!string.IsNullOrEmpty(e.address))
                {
                    var handle = Addressables.LoadAssetAsync<GameObject>(e.address);
                    yield return handle;

                    if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                    {
                        prefab = handle.Result;
                        _handles.Add(handle);
                    }
                    else
                    {
                        Debug.LogWarning($"[PoolManager] Не удалось загрузить '{e.address}'. Используем fallback.");
                        Addressables.Release(handle);
                        prefab = e.fallbackPrefab;
                    }
                }
                else
                {
                    prefab = e.fallbackPrefab;
                }

                if (prefab == null)
                {
                    Debug.LogWarning("[PoolManager] Префаб не найден ни в Addressables, ни в fallback. Пропуск.");
                    continue;
                }

                // --- Определяем ключ ---
                key = string.IsNullOrEmpty(e.key) ? prefab.name : e.key;

                _prefabByKey[key] = prefab;

                if (!_pools.ContainsKey(key))
                    _pools[key] = new Queue<GameObject>();

                // --- Создаём начальные объекты ---
                for (int i = 0; i < e.initial; i++)
                {
                    var obj = CreatePooledInstance(prefab, key);
                    _pools[key].Enqueue(obj);
                    _inPoolHashes.Add(obj.GetInstanceID());
                }

                Debug.Log($"[PoolManager] Пул '{key}' готов: {e.initial} объектов.");
            }
        }

        // --- Создание экземпляра с PooledItem ---

        private GameObject CreatePooledInstance(GameObject prefab, string key)
        {
            var obj = Instantiate(prefab, transform);
            obj.name = prefab.name;
            obj.SetActive(false);

            // Добавляем PooledItem, если нет
            var item = obj.GetComponent<PooledItem>();
            if (item == null)
                item = obj.AddComponent<PooledItem>();
            item.poolKey = key;

            return obj;
        }

        // --- Spawn ---

        /// <summary>
        /// Берёт объект из пула по референсу на префаб. Ключ = prefab.name.
        /// </summary>
        public GameObject Spawn(GameObject prefab)
        {
            if (prefab == null) return null;
            return SpawnByKey(prefab.name, prefab);
        }

        /// <summary>
        /// Берёт объект из пула по ключу.
        /// </summary>
        public GameObject SpawnByKey(string key)
        {
            return SpawnByKey(key, null);
        }

        private GameObject SpawnByKey(string key, GameObject fallbackPrefab)
        {
            if (string.IsNullOrEmpty(key)) return null;

            if (!_pools.ContainsKey(key))
                _pools[key] = new Queue<GameObject>();

            var pool = _pools[key];

            if (pool.Count > 0)
            {
                var obj = pool.Dequeue();
                _inPoolHashes.Remove(obj.GetInstanceID());
                obj.SetActive(true);
                _activeObjects.Add(obj);
                return obj;
            }

            // Пул пуст — создаём новый
            GameObject sourcePrefab = null;
            _prefabByKey.TryGetValue(key, out sourcePrefab);

            if (sourcePrefab == null)
                sourcePrefab = fallbackPrefab;

            if (sourcePrefab != null)
            {
                var obj = CreatePooledInstance(sourcePrefab, key);
                obj.SetActive(true);
                _activeObjects.Add(obj);
                return obj;
            }

            Debug.LogWarning($"[PoolManager] Нет префаба для ключа '{key}'.");
            return null;
        }

        /// <summary>Совместимость со старым API.</summary>
        public GameObject SpawnFallback(GameObject prefab) => Spawn(prefab);

        // --- Despawn ---

        /// <summary>
        /// Возвращает объект в пул. Читает ключ из PooledItem.
        /// Защищён от двойного вызова.
        /// </summary>
        public void Despawn(GameObject go)
        {
            if (go == null) return;

            int hash = go.GetInstanceID();

            // Защита от двойного Despawn
            if (_inPoolHashes.Contains(hash))
                return; // уже в пуле

            var item = go.GetComponent<PooledItem>();
            string key = item != null ? item.poolKey : go.name;

            go.SetActive(false);
            go.transform.SetParent(transform, false);

            if (!_pools.ContainsKey(key))
                _pools[key] = new Queue<GameObject>();

            _pools[key].Enqueue(go);
            _inPoolHashes.Add(hash);
            _activeObjects.Remove(go);
        }

        /// <summary>
        /// Возвращает объект в пул по конкретному ключу (если имя/PooledItem не подходят).
        /// </summary>
        public void DespawnByKey(string key, GameObject go)
        {
            if (go == null) return;

            int hash = go.GetInstanceID();
            if (_inPoolHashes.Contains(hash)) return;

            var item = go.GetComponent<PooledItem>();
            if (item != null) item.poolKey = key;

            go.SetActive(false);
            go.transform.SetParent(transform, false);

            if (!_pools.ContainsKey(key))
                _pools[key] = new Queue<GameObject>();

            _pools[key].Enqueue(go);
            _inPoolHashes.Add(hash);
            _activeObjects.Remove(go);
        }

        // --- Массовые операции ---

        /// <summary>
        /// Возвращает все активные объекты в пул.
        /// </summary>
        public void DespawnAll()
        {
            var toReturn = new List<GameObject>(_activeObjects);
            foreach (var go in toReturn)
                Despawn(go);
            _activeObjects.Clear();
        }

        /// <summary>
        /// Возвращает все активные объекты конкретного пула.
        /// </summary>
        public void DespawnAllByKey(string key)
        {
            var toReturn = new List<GameObject>();
            foreach (var go in _activeObjects)
            {
                var item = go.GetComponent<PooledItem>();
                if (item != null && item.poolKey == key)
                    toReturn.Add(go);
            }
            foreach (var go in toReturn)
                Despawn(go);
        }

        // --- Информация ---

        public int GetPoolCount(string key)
        {
            return _pools.TryGetValue(key, out var q) ? q.Count : 0;
        }

        public int GetActiveCount()
        {
            return _activeObjects.Count;
        }

        public bool IsRegistered(string key)
        {
            return _prefabByKey.ContainsKey(key);
        }

        // --- Регистрация во время игры (для префабов, не заданных в entries) ---

        /// <summary>
        /// Регистрирует новый префаб в пуле во время игры.
        /// </summary>
        public void RegisterPrefab(string key, GameObject prefab, int initialCount = 0)
        {
            if (prefab == null) return;
            if (string.IsNullOrEmpty(key)) key = prefab.name;

            if (!_prefabByKey.ContainsKey(key))
                _prefabByKey[key] = prefab;

            if (!_pools.ContainsKey(key))
                _pools[key] = new Queue<GameObject>();

            for (int i = 0; i < initialCount; i++)
            {
                var obj = CreatePooledInstance(prefab, key);
                _pools[key].Enqueue(obj);
                _inPoolHashes.Add(obj.GetInstanceID());
            }
        }

        // --- Очистка ---

        void OnDestroy()
        {
            foreach (var h in _handles)
                if (h.IsValid()) Addressables.Release(h);
            _handles.Clear();

            _pools.Clear();
            _prefabByKey.Clear();
            _inPoolHashes.Clear();
            _activeObjects.Clear();

            if (Instance == this)
                Instance = null;
        }
    }

}
