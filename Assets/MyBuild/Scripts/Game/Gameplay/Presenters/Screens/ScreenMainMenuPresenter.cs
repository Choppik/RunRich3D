using R3;
using System.Collections.Generic;
using MyBuild.Scripts.Game.Common;
using MyBuild.Scripts.Utils.MVP.UI;
using MyBuild.Scripts.Game.GameRoot;
using MyBuild.Scripts.Game.Settings;
using MyBuild.Scripts.Game.Gameplay.Managers;
using MyBuild.Scripts.Game.Gameplay.Binderes;

namespace MyBuild.Scripts.Game.Gameplay.Presenters
{
    /// <summary>
    /// Презентер основного окна меню.
    /// </summary>
    public class ScreenMainMenuPresenter : WindowPresenterBase
    {
        public ScreenMainMenuPresenter(GameplayUIManager manager, ISettingsProvider settingsProvider, AudioManager audioManager)
        {
            _manager = manager;
            _settingsProvider = settingsProvider;
            _audioManager = audioManager;
        }

        public override void BindWindow(IWindowBinder window)
        {
            base.BindWindow(window);
            BindSubscriptions();

            _manager.OpenStartPresenter();
        }

        private void BindSubscriptions()
        {
            _compositeDisposable.Add(_settingsProvider.ChangeLangRequest.Subscribe(_ => UpdateTextUI()));
        }

        /// <summary>
        /// Обновление текста в UI.
        /// </summary>
        private void UpdateTextUI()
        {
            var textRu = _settingsProvider.AppSettings.LocalizationData.commonTextUIData.text_language_ru;
            var textEn = _settingsProvider.AppSettings.LocalizationData.commonTextUIData.text_language_en;
            var textDe = _settingsProvider.AppSettings.LocalizationData.commonTextUIData.text_language_de;
            var textBe = _settingsProvider.AppSettings.LocalizationData.commonTextUIData.text_language_be;
            var textTh = _settingsProvider.AppSettings.LocalizationData.commonTextUIData.text_language_tr;
            var listLanguage = new List<string>()
            {
                textRu,
                textEn,
            };

            Binder.SetTextUI(listLanguage);

        }

        public override void Dispose()
        {
            Window?.Close();
            _compositeDisposable.Dispose();
        }

        public override string Name => "ScreenMainMenu";
        public ScreenMainMenuBinder Binder => base.GetWindow<ScreenMainMenuBinder>();

        private readonly GameplayUIManager _manager;
        private readonly ISettingsProvider _settingsProvider;
        private readonly AudioManager _audioManager;

        // Все подписки.
        private readonly CompositeDisposable _compositeDisposable = new();
    }
}
