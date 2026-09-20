using DI;
using MyBuild.Scripts.Game.State.Cmd;
using MyBuild.Scripts.Game.State.Providers;

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
            var gameStateProvider = container.Resolve<IGameStateProvider>();
            var gameState = gameStateProvider.GameState;

            var cmd = new CommandProcessor(gameStateProvider);
            container.RegisterInstance<ICommandProcessor>(cmd);

            // Регистрируем обработчики команд.

            // Регистрируем сервисы.
        }
    }
}
