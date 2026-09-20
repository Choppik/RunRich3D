using R3;
using DI;
using UnityEngine;
using System.Collections;
using MyBuild.Scripts.Utils;
using UnityEngine.SceneManagement;
using MyBuild.Scripts.Game.Common;
using MyBuild.Scripts.Game.Gameplay;
using MyBuild.Scripts.Game.Settings;
using MyBuild.Scripts.Game.GameRoot;
using MyBuild.Scripts.Game.State.Providers;

namespace MyBuild.Scripts.Game
{
    /// <summary>
    /// Точка входа в игру.
    /// </summary>
    public class GameEntryPoint
    {
        private static GameEntryPoint _instance;
        private Coroutines _coroutines;
        private UIRootView _uiRoot;
        private readonly DIContainer _rootContainer = new();
        private DIContainer _cachedSceneContainer;

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

            // Загружаем из ресурсов корневой префаб.
            var prefabUIRoot = Resources.Load<UIRootView>($"{AppConsts.PathPrefabsRoot}UIRoot");

            // Создание компонета корневого префаба.
            _uiRoot = Object.Instantiate(prefabUIRoot);
            Object.DontDestroyOnLoad(_uiRoot.gameObject);
            
            var settingsProvider = new SettingsProvider();
            _rootContainer.RegisterInstance<ISettingsProvider>(settingsProvider);

            var audioManager = _uiRoot.GetComponentInChildren<AudioManager>();
            _rootContainer.RegisterInstance(audioManager);

            _rootContainer.RegisterInstance<IGameStateProvider>(new PlayerPrefsGameStateProvider());
            _rootContainer.RegisterInstance(_uiRoot);
        }

        private async void RunGame()
        {
            await _rootContainer.Resolve<ISettingsProvider>().LoadGameSettings();

            _coroutines.StartCoroutine(LoadAndStartGameplay());
        }

        private IEnumerator LoadAndStartGameplay()
        {
            _cachedSceneContainer?.Dispose();
            _uiRoot.ShowLoadingScreen();

            yield return LoadScene(Scenes.GAMEPLAY);
            yield return new WaitForSeconds(1);

            var isGameStateLoaded = false;
            _rootContainer.Resolve<IGameStateProvider>().LoadGameState().Subscribe(_ => isGameStateLoaded = true);
            yield return new WaitUntil(() => isGameStateLoaded);

            var sceneEntryPoint = Object.FindFirstObjectByType<GameplayEntryPoint>();

            var gameplayContainer = _cachedSceneContainer = new DIContainer(_rootContainer);

            //_rootContainer.Resolve<ISettingsProvider>().SetLocalization(YG2.lang);

            sceneEntryPoint.Run(gameplayContainer);

            _uiRoot.HideLoadingScreen();
        }

        private IEnumerator LoadScene(string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName);
        }
    }
}
