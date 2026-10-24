using MyBuild.Scripts.Game.Common;
using MyBuild.Scripts.Utils.LevelGenerate;
using R3;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Mode")]
    [SerializeField] private GenMode _mode = GenMode.Fixed;

    [Header("Counts")]
    [SerializeField] private int _straightCount = 6;
    [SerializeField] private int _turnCount = 3;
    [SerializeField] private int _doorCount = 4;

    [Header("Tiers")]
    [SerializeField] private TierType _currentRoadTier = TierType.Casual;
    [SerializeField] private TierType _currentDoorTier = TierType.Poor;
    [SerializeField] private TierType _currentPlayerTier = TierType.Casual;

    [Header("Fixed Sequence (S=прямая, T=поворот, D=дверь)")]
    [SerializeField] private string _fixedSequence = "STSTSTST";

    [Header("Streaming Window")]
    [Tooltip("Сколько сегментов держать впереди игрока")]
    [SerializeField] private int _windowAhead = 5;
    [Tooltip("Сколько сегментов держать позади игрока")]
    [SerializeField] private int _windowBehind = 5;

    public bool IsGenerationComplete { get; private set; }

    private struct ActiveSeg
    {
        public int index;
        public GameObject obj;
        public SegmentBase seg;
    }

    private PoolManager _poolManager;
    private List<TypeSequence> _sequence;
    private readonly LinkedList<ActiveSeg> _active = new();
    private Vector3 _nextSpawnPos;
    private Quaternion _nextSpawnRot;
    private int _playerSegmentIndex = -1;
    private GameObject _playerInstance;
    private PlayerController _playerController;

    private readonly Dictionary<SegmentBase, System.IDisposable> _segmentSubscriptions = new();

    private readonly CompositeDisposable _compositeDisposable = new();

    private void Awake()
    {
#if DEBUG
        _mode = GenMode.Fixed;
#else
        _mode = GenMode.Random;
#endif
        _poolManager = FindAnyObjectByType<PoolManager>();

        if (_poolManager == null)
        {
            Debug.Log($"Компонент [{nameof(PoolManager)}] не найден.");
            return;
        }
    }

    private void OnDestroy()
    {
        ClearLevel();
        _compositeDisposable.Dispose();
    }

    public void SetMove(bool value)
    {
        if (_playerController == null) return;

        if (value)
        {
            _playerController.StartMovement();
        }
        else
        {
            _playerController.StopMovement();
        }
    }

    /// <summary>
    /// Установка параметров уровня.
    /// </summary>
    public void SetLevelParams(int straightCount, int turnCount, int doorCount)
    {
        _straightCount = straightCount;
        _turnCount = turnCount;
        _doorCount = doorCount;
    }

    /// <summary>
    /// Генерация уровня.
    /// </summary>
    public IEnumerator GenerateLevel()
    {
        _sequence = BuildSequence();

        _nextSpawnPos = transform.position;
        _nextSpawnRot = transform.rotation;

        int initialCount = Mathf.Min(_windowAhead + 1, _sequence.Count);
        for (int i = 0; i < initialCount; i++)
        {
            SpawnSegment(i);
            yield return null; // один кадр между спавнами — плавный старт
        }

        yield return SpawnPlayer();

        IsGenerationComplete = true;
        Debug.Log($"[LevelGenerator] Стриминг запущен: {_sequence.Count} сегментов в очереди, {_active.Count} активно.");
    }

    /// <summary>
    /// Сборка последовательности уровня.
    /// </summary>
    /// <returns>Последовательность уровня.</returns>
    private List<TypeSequence> BuildSequence()
    {
        var seq = new List<TypeSequence>();

        if (_mode == GenMode.Fixed && !string.IsNullOrEmpty(_fixedSequence))
        {
            foreach (char c in _fixedSequence.ToUpper())
            {
                if (c == 'S') seq.Add(TypeSequence.Straight);
                else if (c == 'T') seq.Add(TypeSequence.Turn);
                else if (c == 'D') seq.Add(TypeSequence.Door);
            }
            seq.Add(TypeSequence.End);
            return seq;
        }

        int sLeft = _straightCount, tLeft = _turnCount, dLeft = _doorCount;

        // Первая — всегда прямая
        if (sLeft > 0) { seq.Add(TypeSequence.Straight); sLeft--; }

        while (sLeft > 0 || tLeft > 0 || dLeft > 0)
        {
            TypeSequence last = seq[^1];
            var opts = new List<TypeSequence>();

            if (sLeft > 0) opts.Add(TypeSequence.Straight);
            if (tLeft > 0 && last != TypeSequence.Turn && last != TypeSequence.Door) opts.Add(TypeSequence.Turn);
            if (dLeft > 0 && last != TypeSequence.Door && last != TypeSequence.Turn) opts.Add(TypeSequence.Door);

            if (opts.Count == 0) break;

            TypeSequence pick = opts[Random.Range(0, opts.Count)];
            seq.Add(pick);

            if (pick == TypeSequence.Straight) sLeft--;
            else if (pick == TypeSequence.Turn) tLeft--;
            else if (pick == TypeSequence.Door) dLeft--;
        }

        // Дверь в конце + EndTrigger
        if (seq.Count > 0 && seq[^1] != TypeSequence.Door)
        {
            if (sLeft > 0) seq.Add(TypeSequence.Straight);
            seq.Add(TypeSequence.Door);
        }
        seq.Add(TypeSequence.End);

        return seq;
    }

    /// <summary>
    /// Спавн сегментов.
    /// </summary>
    /// <param name="index">Индекс сегмента.</param>
    private void SpawnSegment(int index)
    {
        if (index < 0 || index >= _sequence.Count) return;

        TypeSequence type = _sequence[index];

        var flip = false;

        if (type == TypeSequence.Turn)
        {
            flip = Random.value < 0.5f;
        }

        string address = GetAddress(type, flip);

        // Берём из внутреннего пула или создаём новый
        var obj = _poolManager.Spawn(address, _nextSpawnPos, _nextSpawnRot, transform);
        if (obj == null) return;

        if (!obj.TryGetComponent<SegmentBase>(out var seg))
        {
            Debug.LogWarning($"[{nameof(LevelGenerator)}] Нет SegmentBase на префабе по адресу: '{address}'");
            return;
        }

        // Сброс состояния (важно для переиспользуемых объектов)
        seg.ResetState();
        seg.SetSegmentIndex(index);

        if (_segmentSubscriptions.TryGetValue(seg, out var oldSub))
        {
            oldSub.Dispose();
        }

        var newSub = seg.OnPlayerEnteredCallback.Subscribe(i => HandlePlayerEntered(i));

        _compositeDisposable.Add(newSub);

        _segmentSubscriptions[seg] = newSub;

        // Выравнивание по точкам входа/выхода
        seg.AlignEntryTo(_nextSpawnPos, _nextSpawnRot);
        seg.GetExitData(out _nextSpawnPos, out _nextSpawnRot);

        _active.AddLast(new ActiveSeg { index = index, obj = obj, seg = seg });

        // Спавним пикапы и декор сразу при создании сегмента
        if (type == TypeSequence.Straight)
        {
            var road = seg as RoadSegment;
            if (road != null)
            {
               // var spawner = road.GetComponent<PickupSpawner>();
               // if (spawner == null)
               // {
               //     spawner = road.gameObject.AddComponent<PickupSpawner>();
               //     //spawner.preset = (PickupSpawner.Preset)Random.Range(0, 3);
               // }
               // spawner.SpawnRandom();
               //
               // var decor = road.GetComponent<DecorationPlacer>();
               // if (decor == null)
               //     decor = road.gameObject.AddComponent<DecorationPlacer>();
               // decor.PlaceDecorations();
            }
        }

        // Привязываем дверь к предыдущей дороге
        if (type == TypeSequence.Door)
        {
            //if (obj.TryGetComponent<DoorSegment>(out var door))
            //{
            //    // Находим предыдущую дорогу в активном списке
            //    var node = _active.Last;
            //    while (node != null)
            //    {
            //        if (node.Value.seg is RoadSegment prevRoad)
            //        {
            //            prevRoad.nextDoor = door;
            //            // Считаем, финальная ли дверь (последняя в последовательности перед End)
            //            //door.IsFinalDoor = (index == _sequence.Count - 2); // -1 = End
            //            break;
            //        }
            //        node = node.Previous;
            //    }
            //}
        }

        // End trigger
        if (type == TypeSequence.End)
        {
            //var endTrig = obj.GetComponent<EndTrigger>();
            //if (endTrig != null && _active.Count > 0)
            //{
            //    // Привязываем к последней дороге
            //    var node = _active.Last;
            //    while (node != null)
            //    {
            //        if (node.Value.seg is RoadSegment prevRoad)
            //        {
            //            prevRoad.isLastBeforeEnd = true;
            //            prevRoad.endTrigger = endTrig;
            //            break;
            //        }
            //        node = node.Previous;
            //    }
            //}
        }
    }

    /// <summary>
    /// Обработчик события активации сегмента игроком.
    /// </summary>
    /// <param name="segmentIndex">Индекс сегмента.</param>
    private void HandlePlayerEntered(int segmentIndex)
    {
        Debug.Log($"[{nameof(HandlePlayerEntered)}] Игрок на сегменте {segmentIndex}");

        // Если игрок не продвинулся вперёд — ничего не делаем
        if (segmentIndex <= _playerSegmentIndex)
            return;

        _playerSegmentIndex = segmentIndex;

        // --- 1. Вычисляем индекс сегмента, который нужно УДАЛИТЬ ---
        int segmentToRemoveIndex = _playerSegmentIndex - _windowBehind;

        // Ищем в _active сегмент с этим индексом и удаляем его
        var node = _active.First;
        while (node != null)
        {
            if (node.Value.index == segmentToRemoveIndex)
            {
                var segToRemove = node.Value.seg;

                // Сначала чистим подписку, потом убираем из списка, потом в пул
                ReturnToPool(segToRemove);
                _active.Remove(node); // Удаляем именно этот узел

                Debug.Log($"[HandlePlayerEntered] Удалён сегмент {segmentToRemoveIndex} (осталось активных: {_active.Count})");
                break; // Удалили один — выходим
            }
            node = node.Next;
        }

        // --- 2. Спавним ОДИН новый сегмент впереди ---
        int nextIndexToSpawn = _playerSegmentIndex + _windowAhead;

        if (nextIndexToSpawn < _sequence.Count)
        {
            // Проверяем, не заспавнен ли он уже (на случай рассинхрона)
            bool alreadySpawned = _active.Any(a => a.index == nextIndexToSpawn);
            if (!alreadySpawned)
            {
                Debug.Log($"[HandlePlayerEntered] Спавним сегмент {nextIndexToSpawn}");
                SpawnSegment(nextIndexToSpawn);
            }
        }
    }

    private IEnumerator SpawnPlayer()
    {
        // Ставим на первую дорогу
        var firstNode = _active.First;
        if (firstNode != null && firstNode.Value.seg is RoadSegment firstRoad)
        {
            _playerInstance = _poolManager.Spawn("Player", firstRoad.ConnectPoint.position, firstRoad.ConnectPoint.rotation);

            if (_playerInstance.TryGetComponent(out PlayerController controller))
            {
                _playerController = controller;
                _playerController.SetRoad(firstRoad, true);
            }
        }

        // CameraFollow
        var cam = Camera.main;
        var cameraF = cam.GetComponent<CameraFollow>();
        if (cam != null && cameraF == null)
        {
            cameraF = cam.gameObject.AddComponent<CameraFollow>();
            cameraF.SetTarget(_playerInstance.transform);
        }

        yield break;
    }

    /// <summary>
    /// Получение адреса сегмента.
    /// </summary>
    /// <param name="type">Тип сегмента.</param>
    /// <param name="flip">Поворот сегмента поворота.</param>
    /// <returns>Адрес.</returns>
    private string GetAddress(TypeSequence type, bool flip = false)
    {
        return type switch
        {
            TypeSequence.Straight => "Ground",
            TypeSequence.Turn => flip switch
            {
                true => "GroundL",
                false => "GroundR",
            },
            TypeSequence.Door => _currentDoorTier switch
            {
                TierType.Poor => "Door1",
                TierType.Casual => "Door2",
                TierType.Rich => "Door3",
                TierType.Millionaire => "Door4",
                _ => null,
            },
            TypeSequence.End => "GroundEND",
            _ => null,
        };
    }

    private void ReturnToPool(SegmentBase seg)
    {
        if (_segmentSubscriptions.Remove(seg, out var sub))
        {
            sub.Dispose();
        }
        _poolManager.Despawn(seg.gameObject);
    }

    public void ApplyTiers(TierType roadTier, TierType doorTier, TierType playerTire)
    {
        if (roadTier == TierType.Poor)
        {

        }
        //_currentRoadTier = Mathf.Clamp(roadTier, 0, 2);
        //_currentDoorTier = Mathf.Clamp(doorTier, 0, 2);
        //_currentPlayerTier = Mathf.Clamp(doorTier, 0, 2);
    
        // Меняем тир только для еще не созданных сегментов
        // (уже созданные остаются как есть до возврата в пул)
    }

    /// <summary>
    /// Очистка уровня.
    /// </summary>
    public void ClearLevel()
    {
        while (_active.Count > 0)
        {
            var seg = _active.First.Value;
            _active.RemoveFirst();
       
            var road = seg.seg as RoadSegment;
            if (road != null)
            {
                road.GetComponent<PickupSpawner>()?.ClearPickups();
                //road.GetComponent<DecorationPlacer>()?.ClearDecorations();
            }

            ReturnToPool(seg.seg);
            //_poolManager.Despawn(seg.obj);
        }
       
        // Уничтожаем игрока
        if (_playerInstance != null)
        {
            _poolManager.Despawn(_playerInstance);
            _playerInstance = null;
        }

        IsGenerationComplete = false;
    }
}

