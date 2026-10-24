using MyBuild.Scripts.Game.Common;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    /// <summary>
    /// Управляет тирами дороги, двери и игрока. Реагирует на изменение счета.
    /// </summary>
    public class QualityManager
    {
        private TierType _roadTier = TierType.Casual;
        private TierType _doorTier = TierType.Poor;
        private TierType _playerTier = TierType.Casual;

        private LevelGenerator _levelGen;

        public void OnLevelGenerated(LevelGenerator gen)
        {
            _levelGen = gen;
            ApplyTiers();
        }

        public void OnTierChanged(TierType newTier)
        {
            // Тир дороги = качеству, тир двери = тоже
            _roadTier = _doorTier = _playerTier = newTier;
            ApplyTiers();
        }

        private void ApplyTiers()
        {
            if (_levelGen != null)
                _levelGen.ApplyTiers(_roadTier, _doorTier, _playerTier);
        }
    }
}
