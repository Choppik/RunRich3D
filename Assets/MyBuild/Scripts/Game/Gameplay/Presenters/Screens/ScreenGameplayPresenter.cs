using R3;
using System.Linq;
using MyBuild.Scripts.Game.Common;
using MyBuild.Scripts.Utils.MVP.UI;
using MyBuild.Scripts.Game.GameRoot;
using MyBuild.Scripts.Game.Settings;
using MyBuild.Scripts.Game.State.Providers;
using MyBuild.Scripts.Game.Gameplay.Managers;
using MyBuild.Scripts.Game.Gameplay.Binderes;

namespace MyBuild.Scripts.Game.Gameplay.Presenters
{
    /// <summary>
    /// Презентер основного окна геймплея.
    /// </summary>
    public class ScreenGameplayPresenter : WindowPresenterBase
    {
        public ScreenGameplayPresenter(
            GameplayUIManager manager, 
            ISettingsProvider settingsProvider, 
            IGameStateProvider gameStateProvider,
            AudioManager audioManager)
        {
            _manager = manager;
            _settingsProvider = settingsProvider;
            _gameStateProvider = gameStateProvider;
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
            //_compositeDisposable.Add(Binder.PauseGameRequest.Subscribe(_ => RequestOpenPauseGame()));
            //_compositeDisposable.Add(Binder.LoseRequest.Subscribe(_ => RequestOpenLose()));
        }

        /// <summary>
        /// Обновление текста в UI.
        /// </summary>
        private void UpdateTextUI()
        {
            var textLevel = _settingsProvider.AppSettings.LocalizationData.commonTextUIData.text_level;
            var textStage = _settingsProvider.AppSettings.LocalizationData.commonTextUIData.text_stage;

            Binder.SetTextUI(textLevel, textStage);
        }


        /// <summary>
        /// Отправить запрос для открытия вспомогательного окна "Пауза".
        /// </summary>
        private void RequestOpenPauseGame()
        {
           
            _audioManager.Stop(AppConsts.Background);
            _audioManager.Play(AppConsts.Click);
            //_manager.OpenPauseGamePresenter(this);
        }

        public override void Dispose()
        {
            Window?.Close();
            _compositeDisposable.Dispose();
        }

        public override string Name => "ScreenGameplay";

        public ScreenGameplayBinder Binder => base.GetWindow<ScreenGameplayBinder>();


        private readonly GameplayUIManager _manager;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly AudioManager _audioManager;

        // Все подписки.
        private readonly CompositeDisposable _compositeDisposable = new();
    }
}
