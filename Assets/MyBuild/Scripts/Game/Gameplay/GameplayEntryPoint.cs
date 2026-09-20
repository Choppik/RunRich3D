using DI;
using UnityEngine;
using MyBuild.Scripts.Game.Gameplay.Binderes;
using MyBuild.Scripts.Game.Gameplay.Managers;

namespace MyBuild.Scripts.Game.Gameplay
{
    /// <summary>
    /// Точка входа в сцену Gameplay.
    /// </summary>
    public class GameplayEntryPoint : MonoBehaviour
    {
        [SerializeField] private UIGameplayRootBinder _sceneUIRootPrefab;

        public void Run(DIContainer container)
        {
            GameplayRegistrations.Register(container);
            var gameplayPresentationContainer = new DIContainer(container);
            GameplayPresentationsRegistrations.Register(gameplayPresentationContainer);

            var uiRoot = container.Resolve<UIRootView>();
            var uiScene = Instantiate(_sceneUIRootPrefab);
            uiRoot.AttachSceneUI(uiScene.gameObject);

            gameplayPresentationContainer.Resolve<WorldGameplayPresenter>(); // Вызываем, так как пока объект не создан.
            var presenter = gameplayPresentationContainer.Resolve<UIGameplayRootPresenter>();
            presenter.Bind(uiScene);

            var manager = gameplayPresentationContainer.Resolve<GameplayUIManager>();
            manager.OpenGameplayPresenter();
           // manager.OpenMainMenuPresenter();
        }
    }
}
