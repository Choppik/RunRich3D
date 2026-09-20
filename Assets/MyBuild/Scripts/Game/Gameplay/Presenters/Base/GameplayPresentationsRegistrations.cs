using DI;
using MyBuild.Scripts.Game.Gameplay.Managers;

namespace MyBuild.Scripts.Game.Gameplay
{
    /// <summary>
    /// Класс-регистратор презенторов для сцены Gameplay.
    /// </summary>
    public static class GameplayPresentationsRegistrations
    {
        public static void Register(DIContainer container)
        {
            container.RegisterFactory(c => new UIGameplayRootPresenter()).AsSingle();
            container.RegisterFactory(c => new WorldGameplayPresenter(container.Resolve<UIGameplayRootPresenter>())).AsSingle();
            container.RegisterFactory(c => new GameplayUIManager(container)).AsSingle();
        }
    }
}
