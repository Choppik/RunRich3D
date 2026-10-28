using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    public class PoolManager : MonoBehaviour
    {
        private static PoolManager _instance;

        [System.Serializable]
        public class PoolConfig
        {
            [Tooltip("Address из Addressables Groups")]
            [SerializeField] private string _address;
            [Tooltip("Сколько экземпляров предсоздать при загрузке")]
            [SerializeField] private int _preloadCount = 3;

            public string Address { get => _address; }
            public int PreloadCount { get => _preloadCount; }
        }

        [Header("Pool Config")]
        [SerializeField] private List<PoolConfig> _configs = new();

        // Загруженные префабы: address → prefab
        private readonly Dictionary<string, GameObject> _prefabs = new();
        // Хэндлы загрузки: address → handle (для освобождения памяти)
        private readonly Dictionary<string, AsyncOperationHandle<GameObject>> _handles = new();
        // Пулы экземпляров: address → очередь неактивных объектов
        private readonly Dictionary<string, Queue<GameObject>> _pools = new();
        // Какому пулу принадлежит объект: instanceID → address
        private readonly Dictionary<EntityId, string> _instanceToAddress = new();

        /// <summary>
        /// Признак готов ли пул.
        /// </summary>
        public bool IsReady { get; private set; } = false;

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        void OnDestroy()
        {
            Cleanup();
        }

        /// <summary>
        /// Предзагрузка: грузит все префабы через Addressables,
        /// затем создаёт preloadCount экземпляров каждого.
        /// </summary>
        public IEnumerator Initialize()
        {
            // 1. Загружаем все префабы
            foreach (var cfg in _configs)
            {
                if (string.IsNullOrEmpty(cfg.Address) || _prefabs.ContainsKey(cfg.Address))
                    continue;

                var handle = Addressables.LoadAssetAsync<GameObject>(cfg.Address);
                yield return handle;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _prefabs[cfg.Address] = handle.Result;
                    _handles[cfg.Address] = handle;
                    _pools[cfg.Address] = new Queue<GameObject>();
                }
                else
                {
                    Debug.LogError($"[Pool] Не удалось загрузить '{cfg.Address}'");
                }
            }

            // 2. Предсоздаём экземпляры
            foreach (var cfg in _configs)
            {
                if (!_prefabs.ContainsKey(cfg.Address)) continue;

                for (int i = 0; i < cfg.PreloadCount; i++)
                {
                    var obj = Instantiate(_prefabs[cfg.Address], transform);
                    obj.SetActive(false);
                    _pools[cfg.Address].Enqueue(obj);
                    var id = obj.GetEntityId();
                    _instanceToAddress[id] = cfg.Address;
                }
            }

            IsReady = true;
            Debug.Log("[Pool] Инициализация завершена.");
        }

        /// <summary>
        /// Берёт объект из пула. Если пул пуст — создаёт новый экземпляр.
        /// </summary>
        public GameObject Spawn(string address, Vector3 pos, Quaternion rot, Transform parent = null)
        {
            if (!_pools.TryGetValue(address, out var queue))
            {
                Debug.LogWarning($"[{nameof(PoolManager)}] Адрес '{address}' не зарегистрирован.");
                return null;
            }

            GameObject obj;
            if (queue.Count > 0)
            {
                obj = queue.Dequeue();
            }
            else
            {
                // Пул пуст — создаём новый экземпляр
                obj = Instantiate(_prefabs[address], parent != null ? parent : transform);
                var id = obj.GetEntityId();
                _instanceToAddress[id] = address;
                Debug.Log($"[{nameof(PoolManager)}] Расширение пула '{address}' (+1)");
            }

            if (parent != null) obj.transform.SetParent(parent, false);
            obj.transform.SetPositionAndRotation(pos, rot);
            obj.SetActive(true);

            return obj;
        }

        /// <summary>
        /// Возвращает объект в пул (деактивирует, кладёт в очередь).
        /// </summary>
        public void Despawn(GameObject obj)
        {
            if (obj == null) return;

            var id = obj.GetEntityId();
            if (!_instanceToAddress.TryGetValue(id, out var address))
            {
                Debug.LogWarning($"[{nameof(PoolManager)}] Объект {obj.name} не принадлежит пулу. Уничтожаю.");
                Destroy(obj);
                return;
            }

            obj.SetActive(false);
            obj.transform.SetParent(transform);
            _pools[address].Enqueue(obj);
        }

        /// <summary>
        /// Полная очистка: уничтожает все экземпляры и освобождает память Addressables.
        /// </summary>
        public void Cleanup()
        {
            // Уничтожаем все объекты во всех пулах
            foreach (var kvp in _pools)
            {
                while (kvp.Value.Count > 0)
                {
                    var obj = kvp.Value.Dequeue();
                    if (obj != null) Destroy(obj);
                }
            }

            _pools.Clear();
            _instanceToAddress.Clear();

            // Освобождаем загруженные ассеты
            foreach (var kvp in _handles)
            {
                Addressables.Release(kvp.Value);
            }
            _handles.Clear();
            _prefabs.Clear();

            IsReady = false;
            Debug.Log($"[{nameof(PoolManager)}] Полная очистка.");
        }

    }
}
