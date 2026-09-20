using R3;
using MyBuild.Scripts.Game.Common;
using MyBuild.Scripts.Utils.MVP.UI;
using MyBuild.Scripts.Game.GameRoot;
using MyBuild.Scripts.Game.Settings;
using MyBuild.Scripts.Game.Gameplay.Binderes;
using MyBuild.Scripts.Game.Gameplay.Managers;

namespace MyBuild.Scripts.Game.Gameplay.Presenters
{
    /// <summary>
    /// Презентер стартого окна.
    /// </summary>
    public class PopupStartPresenter : WindowPresenterBase
    {
        public PopupStartPresenter(GameplayUIManager manager, ISettingsProvider settingsProvider, AudioManager audioManager)
        {
            _manager = manager;
            _settingsProvider = settingsProvider;
            _audioManager = audioManager;
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
            _compositeDisposable.Add(_settingsProvider.ChangeLangRequest.Subscribe(_ => UpdateTextUI()));
            _compositeDisposable.Add(Binder.ToGoModsRequest.Subscribe(_ => RequestToGoMods()));
            _compositeDisposable.Add(Binder.ToGoOtherRequest.Subscribe(_ => RequestToGoOther()));
        }


        /// <summary>
        /// Обновление текста в UI.
        /// </summary>
        private void UpdateTextUI()
        {
            var textToGoMods = _settingsProvider.AppSettings.LocalizationData.buttonUIData.button_modes;
            var textOther = _settingsProvider.AppSettings.LocalizationData.buttonUIData.button_other;

            Binder.SetTextUI(textToGoMods, textOther);
        }

        /// <summary>
        /// Отправить запрос для открытия окна Режимы.
        /// </summary>
        private void RequestToGoMods()
        {
            _audioManager.Play(AppConsts.Click);
            _manager.ClosePopup(Name);
            //_manager.OpenModsPresenter();
        }

        /// <summary>
        /// Отправить запрос для открытия окна Дополнительно.
        /// </summary>
        private void RequestToGoOther()
        {
            _audioManager.Play(AppConsts.Click);
            _manager.ClosePopup(Name);
            //_manager.OpenOtherPresenter();
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
        private readonly AudioManager _audioManager;

        // Все подписки.
        private readonly CompositeDisposable _compositeDisposable = new();
    }
}
