using R3;
using System;

namespace MyBuild.Scripts.Utils.MVP.UI
{
    /// <summary>
    /// Базовый класс для всех окон в игре.
    /// </summary>
    public abstract class WindowPresenterBase : IDisposable
    {
        /// <summary>
        /// Запрос закрытия окна.
        /// </summary>
        public void RequestClose()
        {
            _closeRequested.OnNext(this);
        }

        /// <summary>
        /// Привязка окна к презентору.
        /// </summary>
        /// <param name="window">Сссылка на окно.</param>
        public virtual void BindWindow(IWindowBinder window)
        {
            Window = window;
        }

        /// <summary>
        /// Получить объект реализующий интерфейс.
        /// </summary>
        /// <typeparam name="T">Тип объекта.</typeparam>
        /// <returns>Конкретный объект.</returns>
        protected virtual T GetWindow<T>() where T : IWindowBinder
        {
            return (T)Window;
        }

        public virtual void Dispose() { }

        /// <summary>
        /// Спикок всех запросов на закрытие окна.
        /// </summary>
        public Observable<WindowPresenterBase> CloseRequested => _closeRequested;

        public abstract string Name { get; }

        /// <summary>
        /// Представление презентора.
        /// </summary>
        public IWindowBinder Window { get; private set; }

        private readonly Subject<WindowPresenterBase> _closeRequested = new();
    }
}
