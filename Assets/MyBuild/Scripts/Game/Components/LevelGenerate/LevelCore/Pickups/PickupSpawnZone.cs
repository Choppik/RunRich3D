using MyBuild.Scripts.Game.Common;
using System.Collections.Generic;
using UnityEngine;
using R3;

namespace MyBuild.Scripts.Game.Components.LevelGenerate
{
    public class PickupSpawnZone : MonoBehaviour
    {
        private enum Pattern { Row, Line, LineLeft, LineRight }
        private struct Group
        {
            public Pattern Pattern;
            public int Count;
            public Vector3 Center; // локальный центр группы
        }

        [Header("Пул")]
        [SerializeField] private PoolManager _poolManager;

        [Header("Правила спавна")]
        [SerializeField, Range(0, 100)] private int _positiveChance = 100;

        [Header("Масштаб пинапов")]
        [SerializeField, Min(0.01f)] private float _spawnScale = 0.5f;

        [Header("Визуализация зоны (только в редакторе)")]
        [SerializeField] private Vector3 _zoneSize = new(4f, 0.2f, 10f);

        [Header("Группы")]
        [SerializeField, Min(1)] private int _minGroups = 2;
        [SerializeField, Min(1)] private int _maxGroups = 4;
        [SerializeField, Min(1)] private int _minPerGroup = 2;
        [SerializeField, Min(1)] private int _maxPerGroup = 4;
        [SerializeField, Min(0.1f)] private float _spacing = 0.5f;      // между пинапами в группе
        [SerializeField, Min(0f)] private float _gapBetweenGroups = 2f; // зазор вдоль дороги

        [Header("Защита от наложений")]
        [SerializeField, Min(0.1f)] private float _minDistance = 0.45f; // минимум между пинапами

        [Header("Плотный режим")]
        [SerializeField, Min(1)] private int _denseRows = 4;

        [Header("Расположение (в локальных координатах сегмента)")]
        [SerializeField] private Vector3 _centerOffset = Vector3.zero;

        [Header("Разброс по ширине зоны")]
        [SerializeField, Range(0f, 1f)] private float _lineSideOffset = 0.6f;

        [Header("Превью в редакторе")]
        [SerializeField] private int _previewSeed = 12345; // фиксирует картинку гизмо
        [SerializeField] private bool _previewDense;

        private readonly List<GameObject> _instances = new();
        private bool _spawned;
        private readonly int _minValuePickup = 5;
        private readonly int _maxValuePickup = 20;

        private readonly Dictionary<PickupTrigger, System.IDisposable> _pickupsSubscriptions = new();

        private void Awake()
        {
            _poolManager = FindAnyObjectByType<PoolManager>();

            if (_poolManager == null)
            {
                Debug.Log($"Компонент [{nameof(PoolManager)}] не найден.");
                return;
            }
        }

        private void OnDrawGizmos()
        {
            // Сохраняем состояние рандома: превью стабильно и не ломает рандом игры
            var oldState = Random.state;
            Random.InitState(_previewSeed);
            var plan = _previewDense ? GenerateDensePlan() : GeneratePlan();
            Random.state = oldState;

            var oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;

            // Рамка и объём зоны
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.35f);
            Gizmos.DrawWireCube(_centerOffset, _zoneSize);

            Color fill = Gizmos.color;
            fill.a = 0.12f;
            Gizmos.color = fill;
            Gizmos.DrawCube(_centerOffset, _zoneSize);

            // Осевая линия зоны: если красный луч не идёт по центру дороги — двигай объект зоны
            Gizmos.color = Color.red;
            Vector3 axisStart = _centerOffset + Vector3.back * (_zoneSize.z * 0.5f);
            Gizmos.DrawRay(axisStart, Vector3.forward * _zoneSize.z);

            // Точки пинапов по цветам паттернов
            foreach (var group in plan)
            {
                Gizmos.color = group.Pattern switch
                {
                    Pattern.Row => Color.yellow,
                    Pattern.Line => Color.cyan,
                    Pattern.LineLeft => new Color(1f, 0.4f, 0.8f),
                    _ => new Color(1f, 0.6f, 0f),
                };

                for (int i = 0; i < group.Count; i++)
                {
                    float offset = (i - (group.Count - 1) * 0.5f) * _spacing;

                    Vector3 localPos = group.Pattern == Pattern.Row
                        ? group.Center + new Vector3(offset, 0f, 0f)
                        : group.Center + new Vector3(0f, 0f, offset);

                    Gizmos.DrawCube(localPos, Vector3.one * 0.3f * _spawnScale);
                }
            }

            Gizmos.matrix = oldMatrix;
        }

        /// <summary>
        /// Генерация плана спавна пинапов.
        /// </summary>
        private List<Group> GeneratePlan()
        {
            var plan = new List<Group>();

            float halfDepth = _zoneSize.z * 0.5f;
            float halfWidth = _zoneSize.x * 0.5f;

            // Реальная полезная длина: зона минус зазор с обоих торцов
            float usableDepth = _zoneSize.z - _gapBetweenGroups * 2f;
            if (usableDepth <= 0f)
            {
                Debug.LogWarning($"[{nameof(PickupSpawnZone)}] '{name}': зона короче зазоров, спавн отменён.");
                return plan;
            }

            int groupCount = Random.Range(_minGroups, _maxGroups + 1);
            float z = -usableDepth * 0.5f;

            for (int g = 0; g < groupCount && z < usableDepth * 0.5f; g++)
            {
                int count = Random.Range(_minPerGroup, _maxPerGroup + 1);
                float alongLength = (count - 1) * _spacing;

                // Группа не влезает целиком — не размещаем её вообще
                if (z + alongLength > usableDepth * 0.5f)
                    break;

                float groupZ = z + alongLength * 0.5f;
                var pattern = (Pattern)Random.Range(0, 4);

                float sideX = pattern switch
                {
                    Pattern.Row => 0,
                    Pattern.Line => (Random.value < 0.5f ? -1f : 1f) * halfWidth * _lineSideOffset,
                    Pattern.LineLeft => -halfWidth * _lineSideOffset,
                    _ => halfWidth * _lineSideOffset,
                };

                plan.Add(new Group
                {
                    Pattern = pattern,
                    Count = count,
                    Center = _centerOffset + new Vector3(sideX, 0f, groupZ),
                });

                // Строгий минимальный шаг вперёд: длина группы + зазор, но не меньше minDistance
                z += Mathf.Max(alongLength + _gapBetweenGroups, _minDistance);
            }

            return plan;
        }

        /// <summary>
        /// Плотный план: равномерная сетка Row-рядов на всю полезную глубину зоны.
        /// </summary>
        private List<Group> GenerateDensePlan()
        {
            var plan = new List<Group>();

            float usableDepth = _zoneSize.z - _gapBetweenGroups * 2f;
            if (usableDepth <= 0f)
            {
                Debug.LogWarning($"[{nameof(PickupSpawnZone)}] '{name}': зона короче зазоров, плотный спавн отменён.");
                return plan;
            }

            // Как в случайном режиме: максимум на группу
            int perRow = Mathf.Max(1, _maxPerGroup);

            // Но зона шире себя не растянет: если Max Per Group не влезает — режем и предупреждаем
            int fitsInWidth = Mathf.FloorToInt(_zoneSize.x / _spacing + 0.0001f);
            if (perRow > fitsInWidth)
            {
                Debug.LogWarning($"[{nameof(PickupSpawnZone)}] '{name}': " +
                                 $"Max Per Group ({perRow}) не влезает в зону шириной {_zoneSize.x} при Spacing {_spacing} " +
                                 $"(максимум {fitsInWidth}). Ряд обрезан до {fitsInWidth}.");
                perRow = Mathf.Max(1, fitsInWidth);
            }

            // Защита от слишком частой сетки: шаг между рядами не меньше _minDistance
            int rows = Mathf.Max(1, Mathf.Min(_denseRows, Mathf.FloorToInt(usableDepth / _minDistance + 0.0001f)));
            float stepZ = usableDepth / rows;

            for (int r = 0; r < rows; r++)
            {
                plan.Add(new Group
                {
                    Pattern = Pattern.Row,
                    Count = perRow,
                    Center = _centerOffset + new Vector3(0f, 0f, -usableDepth * 0.5f + stepZ * (r + 0.5f)),
                });
            }

            return plan;
        }

        /// <summary>
        /// Вызывается генератором при активации сегмента.
        /// Спавнит пинапы ровно один раз за жизнь сегмента.
        /// </summary>
        public void SpawnContent()
        {
            DespawnAll();
            SpawnAll(GeneratePlan(), denseMode: false);
            _spawned = true;    
        }

        /// <summary>
        /// Плотный спавн: только Row-ряды, только позитивные, зона заполнена по максимуму.
        /// </summary>
        public void SpawnDenseContent()
        {
            DespawnAll();
            SpawnAll(GenerateDensePlan(), denseMode: true);
            _spawned = true;
        }

        /// <summary>
        /// Возвращает все пинапы зоны в пул. Вызывать при деспавне сегмента.
        /// </summary>
        public void DespawnAll()
        {
            foreach (var sub in _pickupsSubscriptions.Values)
                sub.Dispose();

            _pickupsSubscriptions.Clear();

            for (int i = _instances.Count - 1; i >= 0; i--)
            {
                if (_instances[i] == null)
                {
                    _instances.RemoveAt(i);
                    continue;
                }

                _poolManager.Despawn(_instances[i]);
            }

            _instances.Clear();
            _spawned = false;
        }

        /// <summary>
        /// Спавн пинапов по плану.
        /// </summary>
        private void SpawnAll(List<Group> plan, bool denseMode)
        {
            if (_poolManager == null || !_poolManager.IsReady)
            {
                Debug.LogWarning($"[{nameof(PickupSpawnZone)}] '{name}': пул не готов, спавн пропущен.");
                return;
            }

            if (_spawned)
                return;

            var taken = new List<Vector3>(); // все занятые локальные точки

            foreach (var group in plan)
            {
                bool isRow = group.Pattern == Pattern.Row;
                var groupPoints = new List<Vector3>();

                for (int i = 0; i < group.Count; i++)
                {
                    float offset = (i - (group.Count - 1) * 0.5f) * _spacing;

                    Vector3 localPos = isRow
                        ? group.Center + new Vector3(offset, 0f, 0f)
                        : group.Center + new Vector3(0f, 0f, offset);

                    // Внутри группы легально (шаг spacing); сдвигаем только против чужих групп
                    localPos = ResolveOverlap(localPos, taken, allowXShift: isRow);
                    groupPoints.Add(localPos);

                    bool isPositive = denseMode || Random.Range(0, 100) < _positiveChance;
                    string address = isPositive ? AppConsts.PickupPositive : AppConsts.PickupNegative;

                    var obj = _poolManager.Spawn(
                        address,
                        transform.TransformPoint(localPos),
                        transform.rotation,
                        transform);

                    if (obj == null)
                    {
                        Debug.LogError($"[{nameof(PickupSpawnZone)}] '{name}': пул вернул null для '{address}'.");
                        continue;
                    }

                    var value = Random.Range(_minValuePickup, 1 + _maxValuePickup);

                    if (obj.TryGetComponent<PickupView>(out var view))
                    {
                        string s = string.Empty;
                        if (isPositive)
                            s = "+";
                        else
                            s = "-";

                        view.SetPickupValue($"{s}{value}");
                    }

                    if (obj.TryGetComponent<PickupTrigger>(out var trigger))
                    {
                        trigger.Init(isPositive, _poolManager, value);
                        _pickupsSubscriptions[trigger] = trigger.CollectedAndReturned.Subscribe(OnPickupCollected);
                    }

                    if (Mathf.Abs(_spawnScale - 1f) > 0.0001f)
                        obj.transform.localScale = obj.transform.localScale * _spawnScale;

                    _instances.Add(obj);
                }

                taken.AddRange(groupPoints);
            }
        }

        /// <summary>
        /// Если точка слишком близко к уже занятой — сдвигаем по Z,
        /// пока не найдём свободное место (с ограничением попыток).
        /// </summary>
        private Vector3 ResolveOverlap(Vector3 pos, List<Vector3> taken, bool allowXShift)
        {
            const int maxAttempts = 8;

            float minX = _centerOffset.x - _zoneSize.x * 0.5f;
            float maxX = _centerOffset.x + _zoneSize.x * 0.5f;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                bool conflicts = false;

                for (int i = 0; i < taken.Count; i++)
                {
                    if ((taken[i] - pos).sqrMagnitude < _minDistance * _minDistance)
                    {
                        conflicts = true;
                        break;
                    }
                }

                if (!conflicts)
                    return pos;

                // Кольцо сдвигов с растущим шагом
                float shift = _minDistance * (attempt / 2 + 1);

                if (allowXShift && attempt % 2 == 0)
                    pos.x += shift;
                else
                    pos.z += shift;

                pos.x = Mathf.Clamp(pos.x, minX, maxX);
            }

            return pos; // свободного места не нашли — ставим как есть
        }

        private void OnPickupCollected(PickupTrigger pickup)
        {
            _instances.Remove(pickup.gameObject);
            _pickupsSubscriptions.Remove(pickup); // подписка завершила свою работу
        }
    }
}
