using R3;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using ObservableCollections;

namespace MyBuild.Scripts.Utils.MVP.UI
{
    /// <summary>
    /// Презентер корневого UI сцены.
    /// </summary>
    public abstract class UIRootPresenter : IDisposable
    {
        /// <summary>
        /// Привязка биндера к презентору.
        /// </summary>
        /// <param name="binder">Биндер.</param>
        public void Bind(UIRootBinder binder)
        {
            _rootBinder = binder;
        }

        /// <summary>
        /// Открытие основного окна на сцене.
        /// </summary>
        /// <param name="presenter">Презентер окна.</param>
        public async void OpenScreen(WindowPresenterBase presenter)
        {
            if (!CheckPresenter(presenter))
            {
                return;
            }

            _openedScreen.Value?.Dispose();

            var path = $"{presenter.Name}";
            var binder = await _rootBinder.OpenScreenAsync(path);
            _openedScreen.OnNext(presenter);
            presenter.BindWindow(binder);
        }

        /// <summary>
        /// Открытие вспомогательного окна на сцене.
        /// </summary>
        /// <param name="presenter">Презентер окна.</param>
        public async void OpenPopup(WindowPresenterBase presenter)
        {
            if (!CheckPresenter(presenter))
            {
                return;
            }

            if (_openedPopups.Contains(presenter))
            {
                return;
            }

            var path = $"{presenter.Name}";
            var binder = await _rootBinder.OpenPopupAsync(path);
            var subscription = presenter.CloseRequested.Subscribe(ClosePopup);
            _popupsSubscriptions.Add(presenter, subscription);
            _openedPopups.Add(presenter);
            presenter.BindWindow(binder);
        }

        /// <summary>
        /// Закрытие вспомогательного окна.
        /// </summary>
        /// <param name="presenter">Презентер окна.</param>
        public void ClosePopup(WindowPresenterBase presenter)
        {
            if (_openedPopups.Contains(presenter))
            {
                presenter.Dispose();
                _popupsSubscriptions[presenter]?.Dispose();
                _popupsSubscriptions.Remove(presenter);
                _openedPopups.Remove(presenter);
            }
        }

        /// <summary>
        /// Закрытие вспомогательного окна по имени.
        /// </summary>
        /// <param name="name">Название окна.</param>
        public void ClosePopup(string name)
        {
            var presenter = _openedPopups.FirstOrDefault(p => p.Name == name);
            ClosePopup(presenter);
        }

        /// <summary>
        /// Закрытие всех вспомогательных окон.
        /// </summary>
        public void CloseAllPopups()
        {
            _openedPopups.ForEach(p =>
            {
                p.Dispose();
                _popupsSubscriptions[p]?.Dispose();
                _popupsSubscriptions.Remove(p);
            });

            _openedPopups.Clear();
        }

        /// <summary>
        /// Проверка презентера на корректность.
        /// </summary>
        /// <param name="presenter">Презентер окна.</param>
        /// <returns>Истина, если все хорошо.</returns>
        private static bool CheckPresenter(WindowPresenterBase presenter)
        {
            if (null == presenter)
            {
                throw new ArgumentNullException(nameof(presenter));
            }

            if (string.IsNullOrEmpty(presenter.Name))
            {
                Debug.Log($"Префаб не найден: {presenter.Name}.");
                return false;
            }

            return true;
        }

        public virtual void Dispose()
        {
            CloseAllPopups();
            _openedScreen.Value?.Dispose();
        }

        /// <summary>
        /// Путь к префабам сцены.
        /// </summary>
        public virtual string PathPrefabs { get; }

        public ReadOnlyReactiveProperty<WindowPresenterBase> OpenedScreen => _openedScreen;
        public IObservableCollection<WindowPresenterBase> OpenedPopups => _openedPopups;

        private readonly ReactiveProperty<WindowPresenterBase> _openedScreen = new();
        private readonly ObservableList<WindowPresenterBase> _openedPopups = new();
        private readonly Dictionary<WindowPresenterBase, IDisposable> _popupsSubscriptions = new();

        protected UIRootBinder _rootBinder;
    }
}
