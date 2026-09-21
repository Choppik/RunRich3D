using MyBuild.Scripts.Game.Gameplay.Binderes;
using MyBuild.Scripts.Game.Gameplay.Managers;
using MyBuild.Scripts.Game.Settings;
using MyBuild.Scripts.Utils.MVP.UI;
using R3;

namespace MyBuild.Scripts.Game.Gameplay.Presenters
{
    /// <summary>
    /// Презентер стартого окна.
    /// </summary>
    public class PopupPreviewPresenter : WindowPresenterBase
    {
        public PopupPreviewPresenter(GameplayUIManager manager, ISettingsProvider settingsProvider, ScreenGameplayPresenter screenPresenter)
        {
            _manager = manager;
            _settingsProvider = settingsProvider;
            _screenPresenter = screenPresenter;
        }

        public override void BindWindow(IWindowBinder window)
        {
            base.BindWindow(window);
            BindSubscriptions();
        }

        /// <summary>
        /// Подписка на события.
        /// </summary>
        private void BindSubscriptions()
        {
            _compositeDisposable.Add(Binder.ToGoRequest.Subscribe(value => RequestToGo(value)));
        }

        /// <summary>
        /// Отправить запрос для открытия окна Геймплея.
        /// </summary>
        private void RequestToGo(bool value)
        {
            _screenPresenter.Run(value);
            _manager.ClosePopup(Name);
        }

        public override void Dispose()
        {
            Window?.Close();
            _compositeDisposable.Dispose();
        }

        public override string Name => "PopupPreview";
        public PopupPreviewBinder Binder => base.GetWindow<PopupPreviewBinder>();

        private readonly GameplayUIManager _manager;
        private readonly ISettingsProvider _settingsProvider;
        private readonly ScreenGameplayPresenter _screenPresenter;

        // Все подписки.
        private readonly CompositeDisposable _compositeDisposable = new();
    }
}
