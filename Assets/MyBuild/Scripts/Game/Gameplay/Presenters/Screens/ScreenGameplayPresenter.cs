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
            LevelManager levelManager)
        {
            _manager = manager;
            _settingsProvider = settingsProvider;
            _levelManager = levelManager;
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
            
        }

        public void RunCurrenLevel(bool value)
        {
            if (value)
            {
                // TODO: Запуск контроллера плеера
                if (_levelManager.IsLevelGenerate)
                {
                    _levelManager.Run();
                }
            }
        }

        public void RunNextLevel(bool value)
        {
            if (value)
            {
                // TODO: Запуск контроллера плеера + генерация следующего уровня.
                //_levelManager.StartCurrentLevel();
            }
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
        private PopupPreviewPresenter _popupPreviewPresenter;

        // Все подписки.
        private readonly CompositeDisposable _compositeDisposable = new();
    }
}
