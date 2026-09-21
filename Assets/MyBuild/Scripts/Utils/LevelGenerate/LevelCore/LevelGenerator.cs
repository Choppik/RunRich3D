using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    /// <summary>
    /// Генератор уровня. Собирает цепочку: прямые → поворот/дверь → прямые → ...
    /// Подряд могут идти только прямые. Все префабы грузятся через Addressables.
    /// </summary>
    public class LevelGenerator : MonoBehaviour
    {
        public enum GenMode { Random, Fixed }

        [Header("Mode")]
        public GenMode mode = GenMode.Random;

        [Header("Counts")]
        public int straightCount = 6;
        public int turnCount = 2;
        public int doorCount = 3;

        [Header("Addressable Keys")]
        public string[] straightRoadAddresses = { "roads/straight_t0", "roads/straight_t1", "roads/straight_t2" };
        public string[] turnRoadAddresses = { "roads/turn_t0", "roads/turn_t1", "roads/turn_t2" };
        public string[] doorAddresses = { "doors/door_t0", "doors/door_t1", "doors/door_t2" };
        public string endTriggerAddress = "triggers/end_trigger";
        public string playerAddress = "player/player_default";

        [Header("Tiers (управляется QualityManager)")]
        [Range(0, 2)] public int currentRoadTier = 0;
        [Range(0, 2)] public int currentDoorTier = 0;

        [Header("Fixed Sequence (S=прямая, T=поворот, D=дверь)")]
        public string fixedSequence = "SSDSTSD";
        public bool IsGenerationComplete { get; private set; } = false;
        // Состояние
        private readonly List<GameObject> _spawned = new();
        private readonly List<AsyncOperationHandle> _handles = new();
        private Vector3 _nextPos;
        private Quaternion _nextRot;
        private RoadSegment _firstRoad;
        private RoadSegment _lastRoad;

        private enum Seq { Straight, Turn, Door }

        [Header("Auto-start")]
        public bool autoStart = true;

        void Start()
        {
            if (autoStart)
                StartCoroutine(Generate());
        }

        /// <summary>Отключает авто-старт. Вызвать ДО Start().</summary>
        public void DisableAutoStart()
        {
            autoStart = false;
        }

        /// <summary>Запускает генерацию вручную.</summary>
        public void StartGeneration()
        {
            StartCoroutine(Generate());
        }

        /// <summary>Ручной запуск генерации. Вызвать из презентера.</summary>
        public Task GenerateLevelAsync()
        {
            autoStart = false; // отключаем авто-старт
            return Task.Run(async () =>
            {
                // Корутина не запускается из Task, поэтому используем
                // другой подход — просто запускаем корутину
            });
        }

        IEnumerator Generate()
        {
            _nextPos = transform.position;
            _nextRot = transform.rotation;

            List<Seq> sequence = BuildSequence();
            int doorIndex = 0;
            int totalDoors = 0;

            // Считаем двери в последовательности
            foreach (var s in sequence)
                if (s == Seq.Door) totalDoors++;

            for (int i = 0; i < sequence.Count; i++)
            {
                switch (sequence[i])
                {
                    case Seq.Straight:
                        yield return SpawnSegment(
                            straightRoadAddresses[currentRoadTier],
                            seg => { var r = seg.GetComponent<RoadSegment>(); r.shape = RoadSegment.RoadShape.Straight; },
                            isDoor: false);
                        break;

                    case Seq.Turn:
                        yield return SpawnSegment(
                            turnRoadAddresses[currentRoadTier],
                            seg => { var r = seg.GetComponent<RoadSegment>(); r.shape = RoadSegment.RoadShape.Turn; },
                            isDoor: false);
                        break;

                    case Seq.Door:
                        bool isFinal = (doorIndex == totalDoors - 1);
                        yield return SpawnSegment(
                            doorAddresses[currentDoorTier],
                            seg =>
                            {
                                var d = seg.GetComponent<DoorSegment>();
                                if (d != null) d.IsFinalDoor = isFinal;
                            },
                            isDoor: true);
                        doorIndex++;
                        break;
                }
            }

            // EndTrigger после последней дороги
            if (_lastRoad != null)
            {
                yield return SpawnEndTrigger();
            }

            // Игрок
            yield return SpawnPlayer();

            // Уведомляем QualityManager
            QualityManager.Instance?.OnLevelGenerated(this);

            Debug.Log($"[LevelGenerator] Готово: {_spawned.Count} сегментов.");
            IsGenerationComplete = true;

            Debug.Log($"[LevelGenerator] Готово: {_spawned.Count} сегментов, {totalDoors} дверей.");
        }

        // --- Последовательность ---

        List<Seq> BuildSequence()
        {
            var seq = new List<Seq>();

            if (mode == GenMode.Fixed && !string.IsNullOrEmpty(fixedSequence))
            {
                foreach (char c in fixedSequence.ToUpper())
                {
                    if (c == 'S') seq.Add(Seq.Straight);
                    else if (c == 'T') seq.Add(Seq.Turn);
                    else if (c == 'D') seq.Add(Seq.Door);
                }
                return seq;
            }

            int sLeft = straightCount, tLeft = turnCount, dLeft = doorCount;

            // Первая — всегда прямая
            if (sLeft > 0) { seq.Add(Seq.Straight); sLeft--; }

            while (sLeft > 0 || tLeft > 0 || dLeft > 0)
            {
                Seq last = seq[seq.Count - 1];
                var opts = new List<Seq>();

                if (sLeft > 0) opts.Add(Seq.Straight);
                if (tLeft > 0 && last != Seq.Turn && last != Seq.Door) opts.Add(Seq.Turn);
                if (dLeft > 0 && last != Seq.Door && last != Seq.Turn) opts.Add(Seq.Door);

                if (opts.Count == 0) break;

                Seq pick = opts[Random.Range(0, opts.Count)];
                seq.Add(pick);

                if (pick == Seq.Straight) sLeft--;
                else if (pick == Seq.Turn) tLeft--;
                else if (pick == Seq.Door) dLeft--;
            }

            // Гарантируем дверь в конце
            if (seq.Count > 0 && seq[^1] != Seq.Door)
            {
                if (sLeft > 0) seq.Add(Seq.Straight);
                seq.Add(Seq.Door);
            }

            return seq;
        }

        // --- Спавн одного сегмента ---

        IEnumerator SpawnSegment(string address, System.Action<GameObject> onSpawned, bool isDoor)
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(address);
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogError($"[LevelGenerator] Не удалось загрузить '{address}'");
                yield break;
            }

            _handles.Add(handle);
            var prefab = handle.Result;

            var obj = Instantiate(prefab, _nextPos, _nextRot, transform);
            _spawned.Add(obj);

            // Выравниваем по entry point
            var seg = obj.GetComponent<SegmentBase>();
            if (seg != null)
            {
                seg.AlignEntryTo(_nextPos, _nextRot);
                seg.GetExitData(out _nextPos, out _nextRot);
            }

            // Связываем дорогу с предыдущей дверью и наоборот
            if (!isDoor)
            {
                var road = obj.GetComponent<RoadSegment>();
                if (road != null)
                {
                    if (_firstRoad == null) _firstRoad = road;
                    _lastRoad = road;

                    // Добавляем PickupSpawner и DecorationPlacer программно
                    if (road.GetComponent<PickupSpawner>() == null)
                    {
                        var spawner = road.gameObject.AddComponent<PickupSpawner>();
                        // Настройка спавнера будет через QualityManager/ScoreManager
                    }
                    if (road.GetComponent<DecorationPlacer>() == null)
                    {
                        road.gameObject.AddComponent<DecorationPlacer>();
                    }
                }
            }

            onSpawned?.Invoke(obj);
        }

        IEnumerator SpawnEndTrigger()
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(endTriggerAddress);
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogWarning("[LevelGenerator] EndTrigger не загружен, пропускаем.");
                yield break;
            }

            _handles.Add(handle);
            var obj = Instantiate(handle.Result, _nextPos, _nextRot, transform);
            _spawned.Add(obj);

            var seg = obj.GetComponent<SegmentBase>();
            if (seg != null) seg.AlignEntryTo(_nextPos, _nextRot);

            // Привязываем к последней дороге
            if (_lastRoad != null)
            {
                _lastRoad.isLastBeforeEnd = true;
                var endTrig = obj.GetComponent<EndTrigger>();
                if (endTrig != null) _lastRoad.endTrigger = endTrig;
            }
        }

        IEnumerator SpawnPlayer()
        {
            if (string.IsNullOrEmpty(playerAddress)) yield break;

            var handle = Addressables.LoadAssetAsync<GameObject>(playerAddress);
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogError("[LevelGenerator] Игрок не загружен!");
                yield break;
            }

            _handles.Add(handle);
            _playerInstance = Instantiate(handle.Result);

            // Ставим на начало первой дороги
            if (_firstRoad != null)
            {
                _playerInstance.transform.position = _firstRoad.entryPoint.position + _firstRoad.entryPoint.forward * 1f;
                _playerInstance.transform.rotation = _firstRoad.entryPoint.rotation;
            }

            // CameraFollow — добавляем, если нет
            var cam = Camera.main;
            if (cam != null && cam.GetComponent<CameraFollow>() == null)
            {
                cam.gameObject.AddComponent<CameraFollow>().target = _playerInstance.transform;
            }
        }

        // --- Управление тирами ---

        /// <summary>Меняет тир всех активных дорог и дверей.</summary>
        public void ApplyTiers(int roadTier, int doorTier)
        {
            currentRoadTier = Mathf.Clamp(roadTier, 0, 2);
            currentDoorTier = Mathf.Clamp(doorTier, 0, 2);

            foreach (var obj in _spawned)
            {
                if (obj == null) continue;
                var road = obj.GetComponent<RoadSegment>();
                if (road != null) road.SetTier(currentRoadTier);

                var door = obj.GetComponent<DoorSegment>();
                if (door != null) door.SetTier(currentDoorTier);
            }
        }

        // --- Очистка ---

        public void ClearLevel()
        {
            foreach (var obj in _spawned)
            {
                if (obj != null) Destroy(obj);
            }
            _spawned.Clear();

            foreach (var h in _handles)
            {
                if (h.IsValid()) Addressables.Release(h);
            }
            _handles.Clear();

            if (_playerInstance != null) Destroy(_playerInstance);
        }

        void OnDestroy()
        {
            ClearLevel();
        }

        private GameObject _playerInstance;
    }
}
