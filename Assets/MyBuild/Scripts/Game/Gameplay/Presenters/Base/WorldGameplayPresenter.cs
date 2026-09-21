using DI;
using MyBuild.Scripts.Utils.LevelGenerate;
using System.Threading.Tasks;
using UnityEngine;

namespace MyBuild.Scripts.Game.Gameplay.Managers
{
    /// <summary>
    /// Презентер мира: управляет генерацией уровня и связывает сервисы.
    /// </summary>
    public class WorldGameplayPresenter
    {
        private readonly DIContainer _container;

        public WorldGameplayPresenter(DIContainer container)
        {
            _container = container;
        }

        public async Task StartLevelGeneration()
        {
            var poolManager = _container.Resolve<PoolManager>();
            var scoreManager = _container.Resolve<ScoreManager>();
            var qualityManager = _container.Resolve<QualityManager>();

            // Создаём LevelGenerator программно
            var go = new GameObject("[LevelGenerator]");
            var levelGen = go.AddComponent<LevelGenerator>();
            levelGen.DisableAutoStart(); // запрещаем авто-старт

            // Настраиваем (адреса и параметры можно задавать через конфиг)
            levelGen.mode = LevelGenerator.GenMode.Random;
            levelGen.straightCount = 6;
            levelGen.turnCount = 2;
            levelGen.doorCount = 3;
            // Адреса Addressables — задай свои
            levelGen.straightRoadAddresses = new[] { "roads/straight_t0", "roads/straight_t1", "roads/straight_t2" };
            levelGen.turnRoadAddresses = new[] { "roads/turn_t0", "roads/turn_t1", "roads/turn_t2" };
            levelGen.doorAddresses = new[] { "doors/door_t0", "doors/door_t1", "doors/door_t2" };
            levelGen.endTriggerAddress = "triggers/end_trigger";
            levelGen.playerAddress = "player/player_default";

            levelGen.StartGeneration();
            while (!levelGen.IsGenerationComplete)
                await Task.Yield();
            // LevelGenerator сам запустится в Start(), но если хочешь управлять —
            // можно отключить авто-старт и запускать вручную:
            //
            // await levelGen.GenerateLevelAsync();

            // Уведомляем QualityManager
            qualityManager.OnLevelGenerated(levelGen);

            // ScoreManager уже инициализирован, подписываемся на события
            scoreManager.OnQualityTierChanged += tier =>
            {
                qualityManager.OnTierChanged(tier);
            };

            Debug.Log("[WorldGameplayPresenter] Генерация уровня запущена.");
        }
    }
}
