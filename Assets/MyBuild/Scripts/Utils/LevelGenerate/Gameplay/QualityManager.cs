using UnityEngine;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    /// <summary>
    /// Управляет тирами дороги и двери. Реагирует на изменение шкалы.
    /// </summary>
    public class QualityManager : MonoBehaviour
    {
        public static QualityManager Instance { get; private set; }

        [Range(0, 2)] public int roadTier = 0;
        [Range(0, 2)] public int doorTier = 0;

        private LevelGenerator _levelGen;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void OnLevelGenerated(LevelGenerator gen)
        {
            _levelGen = gen;
            ApplyTiers();
        }

        public void OnTierChanged(int newTier)
        {
            // Тир дороги = качеству, тир двери = тоже
            roadTier = Mathf.Clamp(newTier, 0, 2);
            doorTier = Mathf.Clamp(newTier, 0, 2);
            ApplyTiers();
        }

        void ApplyTiers()
        {
            if (_levelGen != null)
                _levelGen.ApplyTiers(roadTier, doorTier);
        }
    }
}
