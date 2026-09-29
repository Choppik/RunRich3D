using MyBuild.Scripts.Game.Common;
using MyBuild.Scripts.Utils.LevelGenerate;
using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    [Header("Zone (relative to road)")]
    [SerializeField] private float zoneStartZ = -4f;
    [SerializeField] float zoneEndZ = 4f;
    [SerializeField] float zoneHalfWidth = 2f;
    [SerializeField] float spawnHeightY = 0.5f;

    [Header("Spawn Settings")]
    [SerializeField] SpawnPattern pattern = SpawnPattern.HorizontalLine;

    [Tooltip("Шанс спавна вообще (0..1)")]
    [Range(0f, 1f)] public float spawnChance = 0.8f;

    [Tooltip("Какой процент от спавнов — позитивные (остальные — негативные)")]
    [Range(0f, 1f)] public float positiveRatio = 0.7f;

    [Tooltip("Шаг между пикапами в линии")]
    [SerializeField] float spacing = 1f;

    [Tooltip("Пул-ключи")]
    [SerializeField] private string positivePoolKey = "pickup_positive";
    [SerializeField] string negativePoolKey = "pickup_negative";

    [Tooltip("Максимум пикапов на одном сегменте (оба типа в сумме)")]
    [SerializeField] int maxPickupsPerSegment = 12;

    // Занятые точки (общие для обоих типов)
    private HashSet<long> _occupiedPoints = new();
    private List<GameObject> _activePickups = new();

    private PoolManager _pool;
    private RoadSegment _road;

    void Awake()
    {
        _road = GetComponent<RoadSegment>();
    }

    /// <summary>Вызывается LevelGenerator при создании сегмента.</summary>
    public void SpawnRandom()
    {
        if (_road == null || _road.GetRoadShape() != RoadShape.Straight)
            return;

        if (Random.value > spawnChance)
            return;

        _pool = PoolManager.Instance;
        if (_pool == null)
        {
            Debug.LogWarning("[PickupSpawner] PoolManager не найден.");
            return;
        }

        _occupiedPoints.Clear();

        // Случайно выбираем паттерн
        SpawnPattern chosen = (SpawnPattern)Random.Range(0, 3);

        switch (chosen)
        {
            case SpawnPattern.HorizontalLine:
                SpawnHorizontalLine();
                break;
            case SpawnPattern.VerticalLine:
                SpawnVerticalLine();
                break;
            default:
                SpawnSingle();
                break;
        }
    }

    // --- Горизонтальная линия (4 поперёк) ---

    private void SpawnHorizontalLine()
    {
        float z = Random.Range(zoneStartZ, zoneEndZ);

        // Каждый пикап в линии — независимый выбор типа
        for (int i = 0; i < 4; i++)
        {
            float x = -zoneHalfWidth + spacing * 0.5f + i * spacing;
            TrySpawnAt(x, z);
        }
    }

    // --- Вертикальная линия (4 вдоль) ---

    private void SpawnVerticalLine()
    {
        float x = Random.Range(-zoneHalfWidth + 0.5f, zoneHalfWidth - 0.5f);
        float zStart = Random.Range(zoneStartZ, zoneEndZ - spacing * 3);

        for (int i = 0; i < 4; i++)
        {
            float z = zStart + i * spacing;
            if (z > zoneEndZ) break;
            TrySpawnAt(x, z);
        }
    }

    // --- Одиночный ---

    private void SpawnSingle()
    {
        float x = Random.Range(-zoneHalfWidth + 0.5f, zoneHalfWidth - 0.5f);
        float z = Random.Range(zoneStartZ, zoneEndZ);
        TrySpawnAt(x, z);
    }

    // --- Попытка спавна в точке ---

    private void TrySpawnAt(float localX, float localZ)
    {
        if (_activePickups.Count >= maxPickupsPerSegment) return;

        long hash = PackPoint(localX, localZ);
        if (_occupiedPoints.Contains(hash))
            return; // точка занята — пропускаем

        // Выбор типа: позитив или негатив
        PickupType type = Random.value < positiveRatio
            ? PickupType.Positive
            : PickupType.Negative;

        Vector3 localPos = new(localX, spawnHeightY, localZ);
        Vector3 worldPos = transform.TransformPoint(localPos);

        string poolKey = type == PickupType.Positive
            ? positivePoolKey
            : negativePoolKey;

        //var obj = _pool.SpawnByKey(poolKey);
        //if (obj == null)
        //{
        //    Debug.LogWarning($"[PickupSpawner] Пул '{poolKey}' пуст.");
        //    return;
        //}
        //
        //obj.transform.position = worldPos;
        //obj.transform.rotation = transform.rotation;
        //obj.transform.SetParent(transform, true);

        //_occupiedPoints.Add(hash);
        //_activePickups.Add(obj);
    }

    private long PackPoint(float x, float z)
    {
        int ix = Mathf.RoundToInt(x * 10f);
        int iz = Mathf.RoundToInt(z * 10f);
        return ((long)ix << 32) | (uint)iz;
    }

    /// <summary>Возвращает все пикапы в пул.</summary>
    public void ClearPickups()
    {
        if (_pool == null) _pool = PoolManager.Instance;

        foreach (var p in _activePickups)
        {
            if (p != null && _pool != null)
                _pool.Despawn(p);
        }

        _activePickups.Clear();
        _occupiedPoints.Clear();
    }

    void OnDrawGizmosSelected()
    {
        if (_road == null) _road = GetComponent<RoadSegment>();

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(
            new Vector3(0, spawnHeightY, (zoneStartZ + zoneEndZ) * 0.5f),
            new Vector3(zoneHalfWidth * 2, 0.2f, zoneEndZ - zoneStartZ)
        );
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            new Vector3(0, spawnHeightY, (zoneStartZ + zoneEndZ) * 0.5f),
            new Vector3(zoneHalfWidth * 2, 0.2f, zoneEndZ - zoneStartZ)
        );
        Gizmos.matrix = oldMatrix;
    }
}
