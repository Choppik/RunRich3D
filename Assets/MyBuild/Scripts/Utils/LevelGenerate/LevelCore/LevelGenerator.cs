using MyBuild.Scripts.Game.Common;
using MyBuild.Scripts.Utils.LevelGenerate;
using R3;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class LevelGenerator : MonoBehaviour
{
    [Header("Mode")]
    [SerializeField] private GenMode mode = GenMode.Fixed;

    [Header("Counts")]
    [SerializeField] private int straightCount = 6;
    [SerializeField] private int turnCount = 3;
    [SerializeField] private int doorCount = 4;

    [Header("Tiers")]
    [SerializeField] private TierType currentRoadTier = TierType.Casual;
    [SerializeField] private TierType currentDoorTier = TierType.Poor;
    [SerializeField] private TierType currentPlayerTier = TierType.Casual;

    [Header("Fixed Sequence (S=прямая, T=поворот, D=дверь)")]
    [SerializeField] private string fixedSequence = "STSTSTST";

    [Header("Streaming Window")]
    [Tooltip("Сколько сегментов держать впереди игрока")]
    [SerializeField] private int windowAhead = 4;
    [Tooltip("Сколько сегментов держать позади игрока")]
    [SerializeField] private int windowBehind = 1;

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

    // Все подписки.
    private readonly CompositeDisposable _compositeDisposable = new();

    private void Start()
    {
        if (PoolManager.Instance == null)
        {
            Debug.LogWarning("[PoolManager] не создан!");
        }
        else
        {
            _poolManager = PoolManager.Instance;
        }

        StartCoroutine(Generate());
    }

    /// <summary>
    /// Генерация уровня.
    /// </summary>
    /// <returns>Сгенерированный уровень.</returns>
    private IEnumerator Generate()
    {
        _sequence = BuildSequence();

        _nextSpawnPos = transform.position;
        _nextSpawnRot = transform.rotation;

        int initialCount = Mathf.Min(windowAhead + 1, _sequence.Count);
        for (int i = 0; i < initialCount; i++)
        {
            SpawnSegment(i);
            yield return null; // один кадр между спавнами — плавный старт
        }

        // 4. Spawn player
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

        if (mode == GenMode.Fixed && !string.IsNullOrEmpty(fixedSequence))
        {
            foreach (char c in fixedSequence.ToUpper())
            {
                if (c == 'S') seq.Add(TypeSequence.Straight);
                else if (c == 'T') seq.Add(TypeSequence.Turn);
                else if (c == 'D') seq.Add(TypeSequence.Door);
            }
            seq.Add(TypeSequence.End);
            return seq;
        }

        int sLeft = straightCount, tLeft = turnCount, dLeft = doorCount;

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
            //flip = Random.value < 0.5f;
            flip = index == 1;
        }

        string address = GetAddress(type, flip);

        // Берём из внутреннего пула или создаём новый
        var obj = _poolManager.Spawn(address, _nextSpawnPos, _nextSpawnRot, transform);
        if (obj == null) return;

        if (!obj.TryGetComponent<SegmentBase>(out var seg))
        {
            Debug.LogWarning($"[LevelGenerator] Нет SegmentBase на префабе по адресу: '{address}'");
            return;
        }

        // Сброс состояния (важно для переиспользуемых объектов)
        seg.ResetState();
        seg.SetSegmentIndex(index);
        _compositeDisposable.Add(seg.OnPlayerEnteredCallback.Subscribe(_ => HandlePlayerEntered(index)));

        // Выравнивание по точкам входа/выхода
        seg.AlignEntryTo(_nextSpawnPos, _nextSpawnRot);
        seg.GetExitData(out _nextSpawnPos, out _nextSpawnRot);

        _active.AddLast(new ActiveSeg { index = index, obj = obj, seg = seg });

        // Спавним пикапы и декор сразу при создании сегмента
        if (type == TypeSequence.Straight || type == TypeSequence.Turn)
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
        if (segmentIndex <= _playerSegmentIndex) return;
        _playerSegmentIndex = segmentIndex;
        Debug.Log($"Обработчик!!!!!!!!!!");
        // 1. Удаляем сегменты далеко позади
        //if (segmentIndex != _playerSegmentIndex)
        //{
        //
        //    _poolManager.Despawn(_active.First.Value.obj);
        //
        //    // 2. Спавним сегменты впереди
        //    int lastActiveIndex = _active.Last.Value.index;
        //    int neededUpTo = segmentIndex + windowAhead;
        //
        //    for (int i = lastActiveIndex + 1; i <= neededUpTo && i < _sequence.Count; i++)
        //    {
        //        SpawnSegment(i);
        //    }
        //}


        // 3. Если игрок вошёл в последний сегмент — ничего не делаем,
        //    EndTrigger или DoorSegment сами обработают конец
    }

    private IEnumerator SpawnPlayer()
    {
        // Ставим на первую дорогу
        var firstNode = _active.First;
        if (firstNode != null && firstNode.Value.seg is RoadSegment firstRoad)
        {
            _playerInstance = _poolManager.Spawn("Player", firstRoad.StartPoint.position, firstRoad.StartPoint.rotation);
        }

        // CameraFollow
        var cam = Camera.main;
        if (cam != null && cam.GetComponent<CameraFollow>() == null)
        {
            cam.gameObject.AddComponent<CameraFollow>().target = _playerInstance.transform;
        }

        yield break;
    }
    
    /// <summary>
    /// Получение адреса сегмента.
    /// </summary>
    /// <param name="type">Тип сегмента.</param>
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
            TypeSequence.Door => currentDoorTier switch
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

    //public void ApplyTiers(int roadTier, int doorTier)
    //{
    //    currentRoadTier = Mathf.Clamp(roadTier, 0, 2);
    //    currentDoorTier = Mathf.Clamp(doorTier, 0, 2);
    //
    //    // Меняем тир только для еще не созданных сегментов
    //    // (уже созданные остаются как есть до возврата в пул)
    //}

    // --- Cleanup ---

    //public void ClearLevel()
    //{
    //    // Возвращаем все активные сегменты в пул
    //    while (_active.Count > 0)
    //    {
    //        var seg = _active.First.Value;
    //        _active.RemoveFirst();
    //
    //        var road = seg.seg as RoadSegment;
    //        if (road != null)
    //        {
    //            road.GetComponent<PickupSpawner>()?.ClearPickups();
    //            road.GetComponent<DecorationPlacer>()?.ClearDecorations();
    //        }
    //
    //        ReturnToSegPool(seg.obj);
    //    }
    //
    //    // Уничтожаем все объекты из внутреннего пула
    //    foreach (var kvp in _segPool)
    //    {
    //        foreach (var obj in kvp.Value)
    //            if (obj != null) Destroy(obj);
    //    }
    //    _segPool.Clear();
    //
    //    // Уничтожаем игрока
    //    if (_playerInstance != null)
    //    {
    //        Destroy(_playerInstance);
    //        _playerInstance = null;
    //    }
    //
    //    // Релизим Addressables хендлы
    //    foreach (var h in _handles)
    //        if (h.IsValid()) Addressables.Release(h);
    //    _handles.Clear();
    //    _prefabs.Clear();
    //}

    void OnDestroy()
    {
        //ClearLevel();
    }
}

