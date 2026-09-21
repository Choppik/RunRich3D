using DI;
using MyBuild.Scripts.Utils.MVP.UI;
using MyBuild.Scripts.Game.GameRoot;
using MyBuild.Scripts.Game.Settings;
using MyBuild.Scripts.Game.State.Providers;
using MyBuild.Scripts.Game.Gameplay.Presenters;

namespace MyBuild.Scripts.Game.Gameplay.Managers
{
    /// <summary>
    /// Менеджер окон сцены геймплея.
    /// </summary>
    public class GameplayUIManager : UIManager
    {
        private readonly AudioManager _audioManager;
        private readonly ISettingsProvider _settingsProvider;
        public GameplayUIManager(DIContainer container) : base(container)
        {
            _audioManager = container.Resolve<AudioManager>();
            _settingsProvider = container.Resolve<ISettingsProvider>();
        }

        /// <summary>
        /// Открытие окна меню.
        /// </summary>
        /// <returns>Презентер меню.</returns>
        public ScreenMainMenuPresenter OpenMainMenuPresenter()
        {
            var presenter = new ScreenMainMenuPresenter(this, _settingsProvider);
            var rootPresenter = Container.Resolve<UIGameplayRootPresenter>();

            rootPresenter.OpenScreen(presenter);

            return presenter;
        }

        /// <summary>
        /// Открытие окна геймплея.
        /// </summary>
        /// <returns>Презентер геймплея.</returns>
        public ScreenGameplayPresenter OpenGameplayPresenter()
        {
            var presenter = new ScreenGameplayPresenter(this, _settingsProvider);
            var rootPresenter = Container.Resolve<UIGameplayRootPresenter>();

            rootPresenter.OpenScreen(presenter);

            return presenter;
        }

        /// <summary>
        /// Открытие превью окна перед игрок.
        /// </summary>
        public PopupPreviewPresenter OpenPreviewPresenter(ScreenGameplayPresenter screenPresenter)
        {
            var presenter = new PopupPreviewPresenter(this, _settingsProvider, screenPresenter);
            var rootPresenter = Container.Resolve<UIGameplayRootPresenter>();
        
            rootPresenter.OpenPopup(presenter);
        
            return presenter;
        }

        /// <summary>
        /// Открытие стартого вспомогательного окна.
        /// </summary>
        public PopupStartPresenter OpenStartPresenter()
        {
            var presenter = new PopupStartPresenter(this, _settingsProvider);
            var rootPresenter = Container.Resolve<UIGameplayRootPresenter>();
        
            rootPresenter.OpenPopup(presenter);
        
            return presenter;
        }

        /// <summary>
        /// Запрос для закрытия попапа.
        /// </summary>
        /// <param name="lastPresenter">Презентер попапа для закрытия.</param>
        public void ClosePopup(string lastPresenter)
        {
            var rootPresenter = Container.Resolve<UIGameplayRootPresenter>();

            rootPresenter.ClosePopup(lastPresenter);
        }
    }
}
