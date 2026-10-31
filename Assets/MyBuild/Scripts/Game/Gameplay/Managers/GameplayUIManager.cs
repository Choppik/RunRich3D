using MyBuild.Scripts.Game.Gameplay.Presenters;
using MyBuild.Scripts.Game.GameRoot;
using MyBuild.Scripts.Game.Settings;

namespace MyBuild.Scripts.Game.Gameplay.Managers
{
    /// <summary>
    /// Менеджер окон сцены геймплея.
    /// </summary>
    public class GameplayUIManager
    {
        private readonly AudioManager _audioManager;
        private readonly ISettingsProvider _settingsProvider;
        private readonly UIGameplayRootPresenter _rootPresenter;
        private readonly LevelManager _levelManager;
        private readonly ScoreManager _scoreManager;
        public GameplayUIManager(
            AudioManager audioManager,
            ISettingsProvider settingsProvider,
            UIGameplayRootPresenter rootPresenter,
            LevelManager levelManager,
            ScoreManager scoreManager)
        {
            _audioManager = audioManager;
            _settingsProvider = settingsProvider;
            _rootPresenter = rootPresenter;
            _levelManager = levelManager;
            _scoreManager = scoreManager;
        }

        /// <summary>
        /// Открытие окна меню.
        /// </summary>
        /// <returns>Презентер меню.</returns>
        public ScreenMainMenuPresenter OpenMainMenuPresenter()
        {
            var presenter = new ScreenMainMenuPresenter(this, _settingsProvider);

            _rootPresenter.OpenScreen(presenter);

            return presenter;
        }

        /// <summary>
        /// Открытие окна геймплея.
        /// </summary>
        /// <returns>Презентер геймплея.</returns>
        public ScreenGameplayPresenter OpenGameplayPresenter()
        {
            var presenter = new ScreenGameplayPresenter(this, _settingsProvider, _levelManager, _scoreManager);

            _rootPresenter.OpenScreen(presenter);

            return presenter;
        }

        /// <summary>
        /// Открытие превью окна перед игрой.
        /// </summary>
        public PopupPreviewPresenter OpenPreviewPresenter(ScreenGameplayPresenter screenPresenter)
        {
            var presenter = new PopupPreviewPresenter(this, _settingsProvider, screenPresenter);

            _rootPresenter.OpenPopup(presenter);
        
            return presenter;
        }

        /// <summary>
        /// Открытие стартого вспомогательного окна.
        /// </summary>
        public PopupStartPresenter OpenStartPresenter()
        {
            var presenter = new PopupStartPresenter(this, _settingsProvider);

            _rootPresenter.OpenPopup(presenter);
        
            return presenter;
        }

        /// <summary>
        /// Запрос для закрытия попапа.
        /// </summary>
        /// <param name="lastPresenter">Презентер попапа для закрытия.</param>
        public void ClosePopup(string lastPresenter)
        {
            _rootPresenter.ClosePopup(lastPresenter);
        }
    }
}
