using UnityEngine;
using UnityEngine.UI;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Управляет счётом, шкалой прогресса и UI. Синглтон.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [Header("Score")]
        public int score = 0;
        public int maxScore = 100;

        [Header("Scale (для дверей и мешей)")]
        [Range(0f, 1f)] public float scale = 0f;

        [Header("UI References (заполнять программно или в инспекторе)")]
        public Text scoreText;
        public Slider scaleSlider;

        [Header("Thresholds (для QualityManager и MeshSwapper)")]
        public float[] meshThresholds = { 0f, 0.3f, 0.6f, 0.9f }; // 4 порога = 4 меша
        public float[] qualityThresholds = { 0f, 0.4f, 0.7f };    // 3 тира

        // События
        public System.Action<int> OnScoreChanged;
        public System.Action<float> OnScaleChanged;
        public System.Action<int> OnQualityTierChanged;
        public System.Action<int> OnMeshTierChanged;

        private int _currentQualityTier = 0;
        private int _currentMeshTier = 0;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            UpdateUI();
        }

        public void AddScore(int amount)
        {
            score += amount;
            score = Mathf.Max(0, score);
            UpdateScale();
            UpdateUI();
            CheckTiers();
            OnScoreChanged?.Invoke(score);
        }

        public void CollectPositive(int value = 1)
        {
            AddScore(value);
            Debug.Log($"[ScoreManager] +{value}. Счёт: {score}, шкала: {scale:F2}");
        }

        public void CollectNegative(int value = 1)
        {
            AddScore(-value);
            Debug.Log($"[ScoreManager] -{value}. Счёт: {score}, шкала: {scale:F2}");
        }

        void UpdateScale()
        {
            scale = Mathf.Clamp01((float)score / maxScore);
            OnScaleChanged?.Invoke(scale);
        }

        void CheckTiers()
        {
            // Quality tier
            int newQuality = 0;
            for (int i = qualityThresholds.Length - 1; i >= 0; i--)
            {
                if (scale >= qualityThresholds[i]) { newQuality = i; break; }
            }
            if (newQuality != _currentQualityTier)
            {
                _currentQualityTier = newQuality;
                OnQualityTierChanged?.Invoke(newQuality);
                QualityManager.Instance?.OnTierChanged(newQuality);
            }

            // Mesh tier
            int newMesh = 0;
            for (int i = meshThresholds.Length - 1; i >= 0; i--)
            {
                if (scale >= meshThresholds[i]) { newMesh = i; break; }
            }
            if (newMesh != _currentMeshTier)
            {
                _currentMeshTier = newMesh;
                OnMeshTierChanged?.Invoke(newMesh);
                var swapper = FindFirstObjectByType<MeshSwapper>();
                swapper?.SetMeshTier(newMesh);
            }
        }

        public float GetNormalizedScale() => scale;

        void UpdateUI()
        {
            if (scoreText != null) scoreText.text = score.ToString();
            if (scaleSlider != null) scaleSlider.value = scale;
        }

        // --- Концовки ---

        public void OnFinalDoorOpened()
        {
            var player = FindFirstObjectByType<PlayerController>();
            if (player != null) player.StopMovement();
            Debug.Log("[ScoreManager] Победа! Финальная дверь открыта.");
            // Здесь вызываешь свой UI-презентер победы
        }

        public void OnDoorFailed()
        {
            var player = FindFirstObjectByType<PlayerController>();
            if (player != null) player.StopMovement();
            Debug.Log("[ScoreManager] Поражение. Недостаточно шкалы для двери.");
            // Здесь вызываешь свой UI-презентер поражения
        }

        public void OnVictory()
        {
            Debug.Log("[ScoreManager] Победа — достигнут конец уровня.");
            // UI победы
        }

        public void OnDefeat()
        {
            Debug.Log("[ScoreManager] Поражение.");
            // UI поражения
        }
    }
}
