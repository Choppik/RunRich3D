using MyBuild.Scripts.Utils.MVP.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace MyBuild.Scripts.Game.Gameplay.Binderes
{
    public class PopupEndLevelBinder : PopupBinder
    {
        [SerializeField] private Button _btnToGoNetLevel;

        private void OnEnable()
        {
            _btnToGoNetLevel.onClick.AddListener(OnToGoNextLevelButtonClicked);
        }

        private void OnDisable()
        {
            _btnToGoNetLevel.onClick.RemoveListener(OnToGoNextLevelButtonClicked);
        }

        private void OnToGoNextLevelButtonClicked()
        {
            _toGoNextLevel.OnNext(Unit.Default);
        }

        /// <summary>
        /// Событие запроса для запусска следующего уровня.
        /// </summary>
        public Observable<Unit> ToGoNextLevel => _toGoNextLevel;
        private readonly Subject<Unit> _toGoNextLevel = new();
    }
}
