using R3;
using UnityEngine;
using UnityEngine.UI;
using MyBuild.Scripts.Utils.MVP.UI;

namespace MyBuild.Scripts.Game.Gameplay.Binderes
{
    /// <summary>
    /// Биндер стартого окна.
    /// </summary>
    public class PopupStartBinder : PopupBinder
    {
        [SerializeField] private Button _btnToGo;

        private void OnEnable()
        {
            _btnToGo.onClick.AddListener(OnToGoButtonClicked);
        }

        private void OnDisable()
        {
            _btnToGo.onClick.RemoveListener(OnToGoButtonClicked);
        }

        private void OnToGoButtonClicked()
        {
            _toGoRequest.OnNext(Unit.Default);
        }

        /// <summary>
        /// Событие запроса для открытия вспомогательного окна Геймплея.
        /// </summary>
        public Observable<Unit> ToGoRequest => _toGoRequest;

        private readonly Subject<Unit> _toGoRequest = new();
    }
}
