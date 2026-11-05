using MyBuild.Scripts.Game.Gameplay.Binderes;
using MyBuild.Scripts.Game.Gameplay.Managers;
using MyBuild.Scripts.Game.Settings;
using MyBuild.Scripts.Game.State.Providers;
using MyBuild.Scripts.Utils.MVP.UI;
using R3;

namespace MyBuild.Scripts.Game.Gameplay.Presenters
{
    /// <summary>
    /// Презентер окна конца уровня.
    /// </summary>
    public class PopupEndLevelPresenter : WindowPresenterBase
    {
        public PopupEndLevelPresenter(
            GameplayUIManager manager, 
            ISettingsProvider settingsProvider,
            IGameStateProvider gameStateProvider,
            ScreenGameplayPresenter screenPresenter)
        {
            _manager = manager;
            _settingsProvider = settingsProvider;
            _gameState = gameStateProvider;
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
            _compositeDisposable.Add(Binder.ToGoNextLevel.Subscribe(_ => RequestToGoNextLevel()));
        }

        /// <summary>
        /// Отправить запрос для запуска следующего уровня.
        /// </summary>
        private void RequestToGoNextLevel()
        {
            _manager.ClosePopup(Name);

            var map = _gameState.GameState.GetGurrentMap();
            int nextLevel = 1;
            if (map != null)
            {
                var data = map.Progress;

                if (data != null)
                {
                    nextLevel += data.CurrentLevel.CurrentValue;
                }
            }

            _screenPresenter.RunNextLevel(nextLevel);
        }

        public override void Dispose()
        {
            Window?.Close();
            _compositeDisposable.Dispose();
        }

        public override string Name => "PopupEndLevel";
        public PopupEndLevelBinder Binder => base.GetWindow<PopupEndLevelBinder>();

        private readonly GameplayUIManager _manager;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IGameStateProvider _gameState;
        private readonly ScreenGameplayPresenter _screenPresenter;
        private readonly CompositeDisposable _compositeDisposable = new();
    }
}
