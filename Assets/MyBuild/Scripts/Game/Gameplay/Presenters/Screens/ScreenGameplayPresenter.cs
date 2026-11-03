using MyBuild.Scripts.Game.Common;
using MyBuild.Scripts.Game.Components.LevelGenerate;
using MyBuild.Scripts.Game.Gameplay.Binderes;
using MyBuild.Scripts.Game.Gameplay.Managers;
using MyBuild.Scripts.Game.GameRoot;
using MyBuild.Scripts.Game.Settings;
using MyBuild.Scripts.Game.State.Providers;
using MyBuild.Scripts.Utils.MVP.UI;
using R3;
using System.Linq;

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
            LevelManager levelManager,
            ScoreManager scoreManager)
        {
            _manager = manager;
            _settingsProvider = settingsProvider;
            _levelManager = levelManager;
            _scoreManager = scoreManager;
        }

        public override void BindWindow(IWindowBinder window)
        {
            base.BindWindow(window);
            BindSubscriptions();

            _manager.OpenPreviewPresenter(this);
        }

        /// <summary>
        /// Подписка на события.
        /// </summary>
        private void BindSubscriptions()
        {
            _scoreManager.OnScoreChanged.Subscribe(OnScoreChange).AddTo(_compositeDisposable);
            EndTrigger.OnEndLevel.Subscribe(_ => LevelEnd()).AddTo(_compositeDisposable);
        }

        public void RunCurrenLevel(bool value)
        {
            if (value)
            {
                if (_levelManager.IsLevelGenerate)
                {
                    _levelManager.Run();
                }
            }
        }

        public void RunNextLevel(int nextLevel)
        {
            _levelManager.StartLevel(nextLevel);
            Binder.SetCurrentLevel(nextLevel.ToString());
            _manager.OpenPreviewPresenter(this);
        }

        private void OnScoreChange(int value)
        {
            Binder.UpdateCurrentScore(value.ToString());
        }

        private void LevelEnd()
        {
            // TODO: Запуспк анимации, звуков и окна конца
            _levelManager.Stop();
            _manager.OpenEndLevelPresenter(this);
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
        private readonly LevelManager _levelManager;
        private readonly ScoreManager _scoreManager;
        private PopupPreviewPresenter _popupPreviewPresenter;

        // Все подписки.
        private readonly CompositeDisposable _compositeDisposable = new();
    }
}
