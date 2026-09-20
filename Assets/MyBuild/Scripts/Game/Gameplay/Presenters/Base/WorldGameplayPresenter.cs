using R3;
using UnityEngine;
using System.Linq;
using System.Collections;
using MyBuild.Scripts.Utils;
using MyBuild.Scripts.Utils.MVP.UI;
//using MyBuild.Scripts.Game.Gameplay.Root.Presentations;

namespace MyBuild.Scripts.Game.Gameplay
{
    /// <summary>
    /// Глобальный презентер геймплея.
    /// </summary>
    /// <remarks>В целом нужен для какой-либо фоновой подгрузки объектов.</remarks>
    public class WorldGameplayPresenter : System.IDisposable
    {
        public WorldGameplayPresenter(UIGameplayRootPresenter rootPresenter)
        {
            _rootPresenter = rootPresenter;
            _compositeDisposable.Add(_rootPresenter.OpenedScreen.Where(p => p != null).Subscribe(presenter => ChangePresenter(presenter)));
        }

        /// <summary>
        /// Событие изменения презентера окна.
        /// </summary>
        /// <param name="windowPresenter">Ссылка на презентер.</param>
        private void ChangePresenter(WindowPresenterBase windowPresenter)
        {

        }

        public void Dispose()
        {
            _rootPresenter.Dispose();
            _compositeDisposable.Dispose();
        }

        private readonly UIGameplayRootPresenter _rootPresenter;
        private readonly CompositeDisposable _compositeDisposable = new();
    }
}
