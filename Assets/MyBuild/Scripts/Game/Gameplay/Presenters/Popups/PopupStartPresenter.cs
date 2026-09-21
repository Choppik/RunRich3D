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
    public class PopupStartPresenter : WindowPresenterBase
    {
        public PopupStartPresenter(GameplayUIManager manager, ISettingsProvider settingsProvider)
        {
            _manager = manager;
            _settingsProvider = settingsProvider;
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
            _compositeDisposable.Add(Binder.ToGoRequest.Subscribe(_ => RequestToGo()));
        }

        /// <summary>
        /// Отправить запрос для открытия окна Геймплея.
        /// </summary>
        private void RequestToGo()
        {
            _manager.ClosePopup(Name);
            _manager.OpenGameplayPresenter();
        }

        public override void Dispose()
        {
            Window?.Close();
            _compositeDisposable.Dispose();
        }

        public override string Name => "PopupStart";
        public PopupStartBinder Binder => base.GetWindow<PopupStartBinder>();

        private readonly GameplayUIManager _manager;
        private readonly ISettingsProvider _settingsProvider;

        // Все подписки.
        private readonly CompositeDisposable _compositeDisposable = new();
    }
}
