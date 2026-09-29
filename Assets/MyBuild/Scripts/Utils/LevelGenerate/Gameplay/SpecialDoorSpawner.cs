using MyBuild.Scripts.Game.Common;
using R3;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    public class SpecialDoorSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [Range(0f, 1f)]
        [Tooltip("Шанс появления спецдверей на сегменте")]
        [SerializeField] private float spawnChance = 0.3f;

        [Tooltip("Максимум таких элементов на сегменте (по умолчанию 1 = одна пара)")]
        [SerializeField] private int maxPairs = 1;

        [Header("Position (local to road)")]
        [Tooltip("Позиция по Z (вдоль дороги), где ставятся двери")]
        [SerializeField] private float spawnZ = 0f;

        [Tooltip("Смещение левой двери по X")]
        [SerializeField] private float leftDoorX = -1.5f;

        [Tooltip("Смещение правой двери по X")]
        [SerializeField] private float rightDoorX = 1.5f;

        [Tooltip("Высота дверей")]
        [SerializeField] private float spawnY = 0f;

        [Header("Addressable Keys")]
        [SerializeField] private string leftDoorAddress = "doors/special_left";
        [SerializeField] private string rightDoorAddress = "doors/special_right";

        [Header("Trigger Settings")]
        [Tooltip("Размер триггера двери (ширина, высота, глубина)")]
        [SerializeField] private Vector3 triggerSize = new(2f, 3f, 0.5f);

        // --- State ---
        private List<GameObject> _activeDoors = new();
        private int _pairsSpawned = 0;

        private RoadSegment _road;

        // Событие: игрок прошёл через одну из дверей
        public Observable<SpecialDoorType> OnDoorPassed => _doorPassed;

        private readonly Subject<SpecialDoorType> _doorPassed = new();

        void Awake()
        {
            _road = GetComponent<RoadSegment>();
        }

        /// <summary>Вызывается LevelGenerator при создании сегмента.</summary>
        public async void SpawnRandom()
        {
            if (_road == null || _road.GetRoadShape() != RoadShape.Straight)
                return;

            if (Random.value > spawnChance)
                return;

            for (int i = 0; i < maxPairs; i++)
            {
                await SpawnPair();
            }
        }

        private async Task SpawnPair()
        {
            // Случайный Z в пределах зоны (если maxPairs > 1, разносим по Z)
            float z = spawnZ;
            if (maxPairs > 1)
                z = spawnZ + (_pairsSpawned * 3f); // шаг 3 метра между парами

            // Левая дверь
            var leftDoor = await SpawnSingleDoor(leftDoorAddress, leftDoorX, z, SpecialDoorType.Left);
            // Правая дверь
            var rightDoor = await SpawnSingleDoor(rightDoorAddress, rightDoorX, z, SpecialDoorType.Right);

            if (leftDoor != null) _activeDoors.Add(leftDoor);
            if (rightDoor != null) _activeDoors.Add(rightDoor);

            _pairsSpawned++;
        }

        private async Task<GameObject> SpawnSingleDoor(
            string address, float localX, float localZ, SpecialDoorType type)
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(address);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogWarning($"[SpecialDoorSpawner] Не удалось загрузить '{address}'");
                return null;
            }

            Vector3 localPos = new(localX, spawnY, localZ);
            Vector3 worldPos = transform.TransformPoint(localPos);

            var door = Instantiate(handle.Result, worldPos, transform.rotation, transform);
            door.name = $"SpecialDoor_{type}_{_pairsSpawned}";

            // Добавляем триггер для отслеживания прохода игрока
            var tracker = door.GetComponent<SpecialDoorTrigger>();
            if (tracker == null)
                tracker = door.AddComponent<SpecialDoorTrigger>();

            tracker.Init(type, triggerSize, this);

            return door;
        }

        /// <summary>
        /// Вызывается из SpecialDoorTrigger при входе игрока.
        /// </summary>
        public void HandleDoorPassed(SpecialDoorType type)
        {
            _doorPassed.OnNext(type);
            Debug.Log($"[SpecialDoorSpawner] Игрок прошёл через {type} дверь.");
        }

        /// <summary>
        /// Уничтожает все спецдвери при Despawn сегмента.
        /// </summary>
        public void ClearDoors()
        {
            foreach (var d in _activeDoors)
            {
                if (d != null) Destroy(d);
            }
            _activeDoors.Clear();
            _pairsSpawned = 0;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            // Левая дверь
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
            Gizmos.DrawCube(
                new Vector3(leftDoorX, spawnY, spawnZ),
                triggerSize
            );
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(
                new Vector3(leftDoorX, spawnY, spawnZ),
                triggerSize
            );

            // Правая дверь
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawCube(
                new Vector3(rightDoorX, spawnY, spawnZ),
                triggerSize
            );
            Gizmos.color = new Color(1f, 0.4f, 0f);
            Gizmos.DrawWireCube(
                new Vector3(rightDoorX, spawnY, spawnZ),
                triggerSize
            );
        }
    }
}
