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
        public ScreenMainMenuPresenter(GameplayUIManager manager, ISettingsProvider settingsProvider)
        {
            _manager = manager;
            _settingsProvider = settingsProvider;
        }

        public override void BindWindow(IWindowBinder window)
        {
            base.BindWindow(window);
            BindSubscriptions();

            _manager.OpenStartPresenter();
        }

        private void BindSubscriptions()
        {

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

        // Все подписки.
        private readonly CompositeDisposable _compositeDisposable = new();
    }
}
