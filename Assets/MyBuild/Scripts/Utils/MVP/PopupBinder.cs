using R3;
using UnityEngine;
using UnityEngine.UI;

namespace MyBuild.Scripts.Utils.MVP.UI
{
    /// <summary>
    /// Базовый класс для привязки вспомогательного окна к конкретному презентору.
    /// </summary>
    /// <remarks>Модульное окно или всплывающая подсказка и т. д..</remarks>
    public abstract class PopupBinder : WindowBinder
    {
        // У каждого подобного окна должна быть возможность полностью закрыть окно,
        // как с помощью отдельной кнопки (пример: окно с обучением), так и с помощью клика
        // по зоне вне окна (пример: окно со всплывающей подсказкой).
        [SerializeField] private Button _btnClose;
        [SerializeField] private Button _btnCloseAlt;

        protected virtual void Start()
        {
            _btnClose?.onClick.AddListener(OnCloseButtonClick);
            _btnCloseAlt?.onClick.AddListener(OnCloseButtonClick);
        }

        protected virtual void OnDestroy()
        {
            _btnClose?.onClick.RemoveListener(OnCloseButtonClick);
            _btnCloseAlt?.onClick.RemoveListener(OnCloseButtonClick);
        }

        /// <summary>
        /// Отправка запроса на закрытие окна.
        /// </summary>
        protected virtual void OnCloseButtonClick()
        {
            _closeRequest.OnNext(Unit.Default);
        }

        /// <summary>
        /// Событие запроса для закрытия окна.
        /// </summary>
        /// <remarks>Аналог - Action</remarks>
        public Observable<Unit> CloseRequest => _closeRequest;

        // Запрос открытия окна с выбором уровня.
        private readonly Subject<Unit> _closeRequest = new();
    }
}
