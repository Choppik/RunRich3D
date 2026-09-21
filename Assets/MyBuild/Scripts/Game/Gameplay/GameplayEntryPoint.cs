using DI;
using MyBuild.Scripts.Game.Gameplay.Binderes;
using MyBuild.Scripts.Game.Gameplay.Managers;
using MyBuild.Scripts.Utils.LevelGenerate;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MyBuild.Scripts.Game.Gameplay
{
    public class GameplayEntryPoint : MonoBehaviour
    {
        [SerializeField] private UIGameplayRootBinder _sceneUIRootPrefab;
        [SerializeField] private AssetReference _poolManagerAddressable;

        public async void Run(DIContainer container)
        {
            // 1. Загружаем PoolManager через Addressables
            var poolHolder = new PoolManagerInstanceHolder();
            poolHolder.Instance = await LoadPoolManager(_poolManagerAddressable);

            // 2. Регистрируем холдер в контейнере (до GameplayRegistrations)
            container.RegisterInstance(poolHolder);

            // 3. Регистрируем все сервисы
            GameplayRegistrations.Register(container);

            var gameplayPresentationContainer = new DIContainer(container);
            GameplayPresentationsRegistrations.Register(gameplayPresentationContainer);

            // 4. UI
            var uiRoot = container.Resolve<UIRootView>();
            var uiScene = Instantiate(_sceneUIRootPrefab);
            uiRoot.AttachSceneUI(uiScene.gameObject);

            var presenter = gameplayPresentationContainer.Resolve<UIGameplayRootPresenter>();
            presenter.Bind(uiScene);

            var manager = gameplayPresentationContainer.Resolve<GameplayUIManager>();
            manager.OpenMainMenuPresenter();

            // 5. Запускаем мир (генерация уровня)
            var worldPresenter = gameplayPresentationContainer.Resolve<WorldGameplayPresenter>();
            await worldPresenter.StartLevelGeneration();
        }

        private async Task<PoolManager> LoadPoolManager(AssetReference reference)
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(reference);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogError("[GameplayEntryPoint] PoolManager не загружен!");
                return null;
            }

            var go = Instantiate(handle.Result);
            DontDestroyOnLoad(go);

            var poolManager = go.GetComponent<PoolManager>();
            if (poolManager == null)
            {
                Debug.LogError("[GameplayEntryPoint] На префабе нет PoolManager!");
                return null;
            }

            // Ждём инициализацию пулов (Start-корутина в PoolManager)
            // Даём один кадр, чтобы Start() отработал
            await Task.Yield();

            return poolManager;
        }
    }
}
