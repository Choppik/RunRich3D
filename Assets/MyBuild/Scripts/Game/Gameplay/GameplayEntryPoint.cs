using DI;
using MyBuild.Scripts.Game.Gameplay.Binderes;
using MyBuild.Scripts.Game.Gameplay.Managers;
using MyBuild.Scripts.Utils.LevelGenerate;
using System.Collections;
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

        public IEnumerator Run(DIContainer container)
        {
            // 1. Сначала загружаем префаб PoolManager через Addressables
            var handle = Addressables.LoadAssetAsync<GameObject>(_poolManagerAddressable);
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogError("[GameplayEntryPoint] Не удалось загрузить префаб PoolManager!");
                yield break;
            }

            var go = Instantiate(handle.Result);
            DontDestroyOnLoad(go);
            
            // 2. Получаем компонент PoolManager (он должен иметь статический Instance)
            if (!go.TryGetComponent<PoolManager>(out var poolManager))
            {
                Debug.LogError("[GameplayEntryPoint] На префабе нет компонента PoolManager!");
                yield break;
            }

            yield return null;

            // 3. Регистрируем в DI не сам объект, а ссылку на Instance
            container.RegisterInstance(PoolManager.Instance);

            // 4. Регистраация сервисов
            GameplayRegistrations.Register(container);

            var gameplayPresentationContainer = new DIContainer(container);
            GameplayPresentationsRegistrations.Register(gameplayPresentationContainer);

            var uiRoot = container.Resolve<UIRootView>();
            var uiScene = Instantiate(_sceneUIRootPrefab);
            uiRoot.AttachSceneUI(uiScene.gameObject);

            var presenter = gameplayPresentationContainer.Resolve<UIGameplayRootPresenter>();
            presenter.Bind(uiScene);

            var manager = gameplayPresentationContainer.Resolve<GameplayUIManager>();
            manager.OpenMainMenuPresenter();

            // 5. Запускаем мир
            var worldPresenter = gameplayPresentationContainer.Resolve<WorldGameplayPresenter>();
            yield return worldPresenter.StartLevelGeneration();
        }
    }
}
