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
            var presenter = new ScreenMainMenuPresenter(this, _settingsProvider, _audioManager);
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
            var gameProvider = Container.Resolve<IGameStateProvider>();
            var presenter = new ScreenGameplayPresenter(this, _settingsProvider, gameProvider, _audioManager);
            var rootPresenter = Container.Resolve<UIGameplayRootPresenter>();

            rootPresenter.OpenScreen(presenter);

            return presenter;
        }

        /// <summary>
        /// Открытие стартого вспомогательного окна.
        /// </summary>
        public PopupStartPresenter OpenStartPresenter()
        {
            var presenter = new PopupStartPresenter(this, _settingsProvider, _audioManager);
            var rootPresenter = Container.Resolve<UIGameplayRootPresenter>();
        
            rootPresenter.OpenPopup(presenter);
        
            return presenter;
        }
        //
        ///// <summary>
        ///// Открытие вспомогательного окна Режимы.
        ///// </summary>
        //public PopupModsPresenter OpenModsPresenter()
        //{
        //    YG2.GameplayStop();
        //
        //    var presenter = new PopupModsPresenter(this, _settingsProvider, _audioManager);
        //    var rootPresenter = Container.Resolve<UIGameplayRootPresenter>();
        //    
        //    rootPresenter.OpenPopup(presenter);
        //
        //    ShowAdv();
        //
        //    return presenter;
        //}
        //
        ///// <summary>
        ///// Открытие вспомогательного окна Дополнительно.
        ///// </summary>
        //public PopupOtherPresenter OpenOtherPresenter()
        //{
        //    YG2.GameplayStop();
        //
        //    var presenter = new PopupOtherPresenter(this, _settingsProvider, _audioManager);
        //    var rootPresenter = Container.Resolve<UIGameplayRootPresenter>();
        //    
        //    rootPresenter.OpenPopup(presenter);
        //
        //    ShowAdv();
        //
        //    return presenter;
        //}
        //
        ///// <summary>
        ///// Открытие вспомогательного окна Правила режима.
        ///// </summary>
        //public PopupInfoModPresenter OpenInfoModPresenter()
        //{
        //    YG2.GameplayStop();
        //
        //    var presenter = new PopupInfoModPresenter(this, _settingsProvider, _audioManager);
        //    var rootPresenter = Container.Resolve<UIGameplayRootPresenter>();
        //    
        //    rootPresenter.OpenPopup(presenter);
        //
        //    ShowAdv();
        //
        //    return presenter;
        //}
        //
        ///// <summary>
        ///// Открытие вспомогательного окна Пауза.
        ///// </summary>
        //public PopupPauseGamePresenter OpenPauseGamePresenter(ScreenGameplayPresenter gameplayPresenter)
        //{
        //    YG2.GameplayStop();
        //
        //    var presenter = new PopupPauseGamePresenter(this, _settingsProvider, gameplayPresenter, _audioManager);
        //    var rootPresenter = Container.Resolve<UIGameplayRootPresenter>();
        //    
        //    rootPresenter.OpenPopup(presenter);
        //
        //    ShowAdv();
        //
        //    return presenter;
        //}
        //
        ///// <summary>
        ///// Открытие вспомогательного окна Победа.
        ///// </summary>
        //public PopupLosePresenter OpenLosePresenter(ScreenGameplayPresenter gameplayPresenter)
        //{
        //    YG2.GameplayStop();
        //
        //    var presenter = new PopupLosePresenter(this, _settingsProvider, gameplayPresenter, _audioManager);
        //    var rootPresenter = Container.Resolve<UIGameplayRootPresenter>();
        //    
        //    rootPresenter.OpenPopup(presenter);
        //
        //    return presenter;
        //}
        //
        ///// <summary>
        ///// Открытие вспомогательного окна Таблица лидеров.
        ///// </summary>
        //public PopupLeaderBoardPresenter OpenLeaderBoardPresenter(ScreenGameplayPresenter gameplayPresenter)
        //{
        //    YG2.GameplayStop();
        //
        //    var gameProvider = Container.Resolve<IGameStateProvider>();
        //    var presenter = new PopupLeaderBoardPresenter(this, _settingsProvider, gameplayPresenter, _audioManager, gameProvider);
        //    var rootPresenter = Container.Resolve<UIGameplayRootPresenter>();
        //    
        //    rootPresenter.OpenPopup(presenter);
        //
        //    ShowAdv();
        //
        //    return presenter;
        //}

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
