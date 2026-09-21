using DI;
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
            // Регистрируем PoolManager как синглтон
            // Он будет создан через Addressables в EntryPoint
            container.RegisterFactory<PoolManager>(resolver =>
            {
                // Возвращаем уже созданный экземпляр (см. EntryPoint ниже)
                return resolver.Resolve<PoolManagerInstanceHolder>().Instance;
            }).AsSingle();

            // ScoreManager — синглтон
            container.RegisterFactory<ScoreManager>(resolver =>
            {
                var go = new GameObject("[ScoreManager]");
                var sm = go.AddComponent<ScoreManager>();
                Object.DontDestroyOnLoad(go);
                return sm;
            }).AsSingle();

            // QualityManager — синглтон
            container.RegisterFactory<QualityManager>(resolver =>
            {
                var go = new GameObject("[QualityManager]");
                var qm = go.AddComponent<QualityManager>();
                Object.DontDestroyOnLoad(go);
                return qm;
            }).AsSingle();
        }
    }
}
