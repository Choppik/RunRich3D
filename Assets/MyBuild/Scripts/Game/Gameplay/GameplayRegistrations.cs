using DI;
using MyBuild.Scripts.Game.Gameplay.Managers;
using MyBuild.Scripts.Utils.LevelGenerate;
using UnityEngine;

namespace MyBuild.Scripts.Game.Gameplay
{
    /// <summary>
    /// Регистрация всех необходимых компонентов (сервисы, команды и т. д.) для сцены геймплея.
    /// </summary>
    public static class GameplayRegistrations
    {
        /// <summary>
        /// Регистрация компонентов.
        /// </summary>
        /// <param name="container">Контейнер, содержащий компоненты уровня приложения.</param>
        public static void Register(DIContainer container)
        {
            container.RegisterFactory(c =>
            {
                var go = new GameObject($"[{nameof(LevelGenerator)}]");
                var levelGen = go.AddComponent<LevelGenerator>();
                return levelGen;
            }).AsSingle();

            // ScoreManager — синглтон
            container.RegisterType<ScoreManager>();

            // QualityManager — синглтон
            container.RegisterType<QualityManager>();

            // LevelManager — синглтон
            container.RegisterType<LevelManager>();
        }
    }
}
