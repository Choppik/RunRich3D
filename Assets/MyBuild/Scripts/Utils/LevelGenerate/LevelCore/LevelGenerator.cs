using MyBuild.Scripts.Game.Common;
using MyBuild.Scripts.Utils;
using MyBuild.Scripts.Utils.LevelGenerate;
using R3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Mode")]
    [SerializeField] private GenMode _mode = GenMode.Fixed;

    [Header("Counts")]
    [SerializeField] private int _doorCount = 4;
    [SerializeField] private int _chanceDoor = 7;

    [Header("Tiers")]
    [SerializeField] private TierType _currentRoadTier = TierType.Casual;
    [SerializeField] private TierType _currentDoorTier = TierType.Poor;
    [SerializeField] private TierType _currentPlayerTier = TierType.Casual;

    [Header("Fixed Sequence (S=прямая, T=поворот, D=дверь)")]
    [SerializeField] private string _fixedSequence = "SDTSTSTSTD";

    [Header("Streaming Window")]
    [Tooltip("Сколько сегментов держать впереди игрока")]
    [SerializeField] private int _windowAhead = 5;
    [Tooltip("Сколько сегментов держать позади игрока")]
    [SerializeField] private int _windowBehind = 3;

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
    private HashSet<int> _activeIndices = new(); // гарантия уникальности
    private Vector3 _nextSpawnPos;
    private Quaternion _nextSpawnRot;
    private int _playerSegmentIndex = -1;
    private GameObject _playerInstance;
    private readonly Dictionary<SegmentBase, System.IDisposable> _segmentSubscriptions = new();

    private readonly CompositeDisposable _compositeDisposable = new();

    private bool _isUpdatingWindow;

    private void Awake()
    {
#if !DEBUG
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

    public void SetMovement(bool value)
    {
        if (_playerInstance == null) return;

        if (_playerInstance.TryGetComponent(out PlayerController controller))
        {
            if (value)
            {
                controller.StartMovement();
            }
            else
            {
                controller.StopMovement();
            }
        }
    }

    /// <summary>
    /// Установка параметров уровня.
    /// </summary>
    public void SetLevelParams(int doorCount, int chanceDoor)
    {
        _doorCount = doorCount;
        _chanceDoor = chanceDoor;
    }

    /// <summary>
    /// Генерация уровня.
    /// </summary>
    public IEnumerator GenerateLevel()
    {
        _sequence = BuildSequence();

        _nextSpawnPos = transform.position;
        _nextSpawnRot = transform.rotation;

        int initialCount = Mathf.Min(_windowAhead + _windowBehind, _sequence.Count);
        for (int i = 0; i < initialCount; i++)
        {
            SpawnSegment(i);
            yield return null; // один кадр между спавнами — плавный старт
        }

        yield return SpawnPlayer();

        IsGenerationComplete = true;
        Debug.Log($"[{nameof(LevelGenerator)}] Стриминг запущен: {_sequence.Count} сегментов в очереди, {_active.Count} активно.");
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

        int dLeft = _doorCount;
        
        // Первая - прямая
        seq.Add(TypeSequence.Straight);

        while (true)
        {
            TypeSequence last = seq[^1];

            // Если уже набрали нужное количество дверей — выходим из цикла генерации, дальше только End
            if (dLeft == 0)
                break;

            var opts = new List<TypeSequence>();

            // ГЛАВНОЕ ПРАВИЛО: после Straight обязательно идёт Door или Turn и наоборот.

            // Добавляем Дверь и Поворот, если лимит позволяет и мы на прямой
            if (dLeft > 0 && last == TypeSequence.Straight)
            {
                opts.Add(TypeSequence.Door);
            }
            
            opts.Add(TypeSequence.Turn);

            if (opts.Count == 0)
            {
                // Тупик: нельзя поставить ни дверь, ни поворот (например, кончились двери, а повороты запрещены).
                // Выходим, дальше сработает блок доводки дверей.
                break;
            }

            TypeSequence pick;

            if (dLeft > 0 && opts.Contains(TypeSequence.Door))
            {
                // Шанс выбора двери
                pick = Random.Range(0, 10) < _chanceDoor ? TypeSequence.Door : TypeSequence.Turn;
            }
            else
            {
                pick = TypeSequence.Straight;
            }

            seq.Add(pick);

            if (pick == TypeSequence.Door)
            {
                dLeft--;
            }
        }

        // БЛОК ДОВОДКИ ДВЕРЕЙ: Гарантируем ровно 4 двери.
        // Если цикл закончился, а дверей меньше 4 — добиваем их принудительно.
        while (dLeft != 0)
        {
            TypeSequence last = seq[^1];

            // По правилу: дверь ставится только после прямой.
            if (last != TypeSequence.Straight)
            {
                seq.Add(TypeSequence.Straight);
            }

            // Теперь последний точно Straight — ставим дверь
            if (seq[^1] == TypeSequence.Straight)
            {
                seq.Add(TypeSequence.Door);
                dLeft--;
            }
            else
            {
                break; // Защита от зацикливания
            }
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

        if (!_activeIndices.Add(index))
        {
            Debug.LogWarning($"[{nameof(SpawnSegment)}] Сегмент {index} уже активен. Пропуск.");
            return;
        }

        TypeSequence type = _sequence[index];
        var flip = (type == TypeSequence.Turn) && (Random.value < 0.5f);
        string address = GetAddress(type, flip);

        // Берём из внутреннего пула или создаём новый
        var obj = _poolManager.Spawn(address, _nextSpawnPos, _nextSpawnRot, transform);
        if (obj == null) return;

        if (!obj.TryGetComponent<SegmentBase>(out var seg))
        {
            Debug.LogWarning($"[{nameof(LevelGenerator)}] Нет SegmentBase на префабе по адресу: '{address}'");
            return;
        }

        if (obj == null || seg == null)
        {
            Debug.LogError($"[{nameof(SpawnSegment)}] Не удалось создать сегмент {index} ('{address}'). Откат резерва.");
            _activeIndices.Remove(index);       // <-- откат
            if (obj != null) ReturnToPool(obj); // мусор не оставляем в сцене
            return;
        }

        try
        {
            seg.ResetState();
            seg.SetSegmentIndex(index);

            if (_segmentSubscriptions.TryGetValue(seg, out var oldSub))
                oldSub.Dispose();

            _segmentSubscriptions[seg] = seg.OnPlayerEnteredCallback.Subscribe(i => HandlePlayerEntered(i));

            seg.AlignEntryTo(_nextSpawnPos, _nextSpawnRot);
            seg.GetExitData(out _nextSpawnPos, out _nextSpawnRot);

            _active.AddLast(new ActiveSeg { index = index, obj = obj, seg = seg });
        }
        catch (System.Exception e)
        {
            // --- ОТКАТ: возвращаем всё в исходное состояние ---
            Debug.LogError($"[{nameof(SpawnSegment)}] Исключение при инициализации сегмента {index}: {e}");

            _activeIndices.Remove(index); // снимаем резерв

            // Удаляем узел из списка, если он успел туда попасть
            var node = _active.First;
            while (node != null)
            {
                var next = node.Next;
                if (node.Value.index == index)
                    _active.Remove(node);
                node = next;
            }

            ReturnToPool(obj);
            return;
        }
    }

    /// <summary>
    /// Обработчик события активации сегмента игроком.
    /// </summary>
    /// <param name="segmentIndex">Индекс сегмента.</param>
    private void HandlePlayerEntered(int segmentIndex)
    {
        Debug.Log($"Игрок на сегменте {segmentIndex}");
        var minTotalSegments = _windowAhead + _windowBehind;
        // Защита от откатов и повторных вызовов с тем же индексом
        if (segmentIndex <= _playerSegmentIndex)
            return;

        // Защита от вложенных вызовов (событие срабатывает во время спавна)
        if (_isUpdatingWindow)
            return;

        _playerSegmentIndex = segmentIndex;
        _isUpdatingWindow = true;

        try
        {
            // 1. Границы окна видимости
            int last = _playerSegmentIndex + _windowAhead;
            if (last >= _sequence.Count)
                last = _sequence.Count - 1;

            int first = last - minTotalSegments + 1;
            if (first < 0)
                first = 0;

            // 2. Удаляем сегменты, вышедшие за левую границу
            var node = _active.First;
            while (node != null)
            {
                var next = node.Next;

                if (node.Value.index < first)
                {
                    _activeIndices.Remove(node.Value.index);
                    _active.Remove(node);
                    ReturnToPool(node.Value.obj);
                }

                node = next;
            }

            // 3. Спавним недостающие — по возрастанию, чтобы цепочка позиций строилась верно
            for (int i = first; i <= last; i++)
            {
                if (!_activeIndices.Contains(i))
                    SpawnSegment(i);
            }
        }
        finally
        {
            _isUpdatingWindow = false;
        }
    }

    /// <summary>
    /// Возвращение объекта в пул + отписка.
    /// </summary>
    private void ReturnToPool(GameObject obj)
    {
        if (obj == null) return;

        if (obj.TryGetComponent<SegmentBase>(out var seg))
        {
            if (_segmentSubscriptions.Remove(seg, out var sub))
            {
                sub.Dispose();
            }
        }

        _poolManager.Despawn(obj);
    }

    /// <summary>
    /// Спавн игрока.
    /// </summary>
    private IEnumerator SpawnPlayer()
    {
        // Ставим на первую дорогу
        var firstNode = _active.First;
        if (firstNode != null && firstNode.Value.seg is RoadSegment firstRoad)
        {
            _playerInstance = _poolManager.Spawn("Player", firstRoad.ConnectPoint.position, firstRoad.ConnectPoint.rotation);

            if (_playerInstance.TryGetComponent(out PlayerController controller))
            {
                controller.SetStartRoad(firstRoad.gameObject.GetComponent<RoadSegmentSpline>());
            }
        }

        // CameraFollow
        var cam = Camera.main;
        var cameraF = cam.GetComponent<CameraFollow>();

        if (cam != null && cameraF == null)
        {
            cameraF = cam.gameObject.AddComponent<CameraFollow>();
        }

        cameraF.SetTarget(_playerInstance.transform);

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

            ReturnToPool(seg.obj);
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

