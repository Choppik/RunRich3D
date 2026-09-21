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
            ISettingsProvider settingsProvider)
        {
            _manager = manager;
            _settingsProvider = settingsProvider;
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

        public override void Dispose()
        {
            Window?.Close();
            _compositeDisposable.Dispose();
        }

        public void Run(bool value)
        {

        }

        public override string Name => "ScreenGameplay";

        public ScreenGameplayBinder Binder => base.GetWindow<ScreenGameplayBinder>();


        private readonly GameplayUIManager _manager;
        private readonly ISettingsProvider _settingsProvider;
        private PopupPreviewPresenter _popupPreviewPresenter;

        // Все подписки.
        private readonly CompositeDisposable _compositeDisposable = new();
    }
}
