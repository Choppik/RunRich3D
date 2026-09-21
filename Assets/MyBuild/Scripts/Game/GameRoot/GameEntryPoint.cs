using R3;
using DI;
using UnityEngine;
using System.Collections;
using MyBuild.Scripts.Utils;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using MyBuild.Scripts.Game.Common;
using MyBuild.Scripts.Game.Gameplay;
using MyBuild.Scripts.Game.Settings;
using MyBuild.Scripts.Game.GameRoot;
using MyBuild.Scripts.Game.State.Providers;
using ButchersGames;
using System.Threading.Tasks;

namespace MyBuild.Scripts.Game
{
    /// <summary>
    /// Точка входа в игру.
    /// </summary>
    public class GameEntryPoint
    {
        private static GameEntryPoint _instance;
        private Coroutines _coroutines;
        private readonly AddressablesSceneLoaderCoroutine _addressablesSceneLoader;
        private UIRootView _uiRoot;
        private readonly DIContainer _rootContainer = new();
        private DIContainer _cachedSceneContainer;

        // Хэндл Addressables для инстанцированного UIRoot (если нужно позднее Release)
        private AsyncOperationHandle<GameObject> _uiRootHandle;

        /// <summary>
        /// Место запуска игры.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void AutoStartGame()
        {
            Application.targetFrameRate = 60;

            _instance = new GameEntryPoint();
            _instance.RunGame();
        }

        private GameEntryPoint() 
        {
            // Создание компонета куррутины.
            _coroutines = new GameObject(name:"[COROUTINES]").AddComponent<Coroutines>();
            Object.DontDestroyOnLoad(_coroutines.gameObject);
            _rootContainer.RegisterInstance(_coroutines);

            _addressablesSceneLoader = new AddressablesSceneLoaderCoroutine();
            
            var settingsProvider = new SettingsProvider();
            _rootContainer.RegisterInstance<ISettingsProvider>(settingsProvider);

            var levelManager = new LevelManager();
            _rootContainer.RegisterInstance(levelManager);

            // Не использую, так как в данном случае используем LevelManager.
            //_rootContainer.RegisterInstance<IGameStateProvider>(new PlayerPrefsGameStateProvider());
        }

        private async void RunGame()
        {
            await _rootContainer.Resolve<ISettingsProvider>().InitializeAsync();
            await _rootContainer.Resolve<ISettingsProvider>().LoadGameSettings();

            await LoadAndInstantiateUIRootAsync();

            _coroutines.StartCoroutine(LoadAndStartGameplay());
        }

        private async Task LoadAndInstantiateUIRootAsync()
        {
            if (_uiRoot != null) return;

            string uiAddress = "UIRoot";

            // Instantiate через Addressables (автоматическая загрузка ассета и инстанс)
            _uiRootHandle = Addressables.InstantiateAsync(uiAddress);
            await _uiRootHandle.Task;

            if (_uiRootHandle.Status == AsyncOperationStatus.Succeeded && _uiRootHandle.Result != null)
            {
                var go = _uiRootHandle.Result;
                _uiRoot = go.GetComponent<UIRootView>();
                if (_uiRoot == null)
                {
                    // Если префаб не содержит компонент UIRootView — добавим/попробуем найти его дочерний
                    _uiRoot = go.GetComponentInChildren<UIRootView>();
                    if (_uiRoot == null) _uiRoot = go.AddComponent<UIRootView>();
                }

                Object.DontDestroyOnLoad(go);

                // Регистрируем UI и AudioManager в контейнере
                _rootContainer.RegisterInstance(_uiRoot);

                var audioManager = _uiRoot.GetComponentInChildren<AudioManager>();
                if (audioManager != null)
                    _rootContainer.RegisterInstance(audioManager);
                else
                    Debug.LogWarning("AudioManager not found in UIRoot after Addressables instantiation.");
            }
            else
            {
                Debug.LogError($"Failed to instantiate UIRoot from Addressables at '{uiAddress}': {_uiRootHandle.OperationException}");
            }
        }

        private IEnumerator LoadAndStartGameplay()
        {
            _cachedSceneContainer?.Dispose();
            _uiRoot.ShowLoadingScreen();

            yield return _addressablesSceneLoader.LoadSceneCoroutine(Scenes.GAMEPLAY);
            yield return new WaitForSeconds(1);

            //var isGameStateLoaded = false;
            //_rootContainer.Resolve<IGameStateProvider>().LoadGameState().Subscribe(_ => isGameStateLoaded = true);
            //yield return new WaitUntil(() => isGameStateLoaded);

            var sceneEntryPoint = Object.FindFirstObjectByType<GameplayEntryPoint>();

            var gameplayContainer = _cachedSceneContainer = new DIContainer(_rootContainer);

            sceneEntryPoint.Run(gameplayContainer);

            _uiRoot.HideLoadingScreen();
        }
    }
}
