using R3;
using TMPro;
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
        [SerializeField] private Button _btnToGoMods;
        [SerializeField] private Button _btnToGoOther;

        /// <summary>
        /// Установка текста в интерфейс.
        /// </summary>
        /// <param name="textToGoMods">Текст кнопки Режимы.</param>
        /// <param name="textToGoOther">Текст кнопки Дополнительно.</param>
        public void SetTextUI(string textToGoMods, string textToGoOther)
        {
            if (!string.IsNullOrEmpty(textToGoMods))
            {
                _btnToGoMods.GetComponentInChildren<TextMeshProUGUI>().text = textToGoMods;
            }

            if (!string.IsNullOrEmpty(textToGoOther))
            {
                _btnToGoOther.GetComponentInChildren<TextMeshProUGUI>().text = textToGoOther;
            }
        }

        private void OnEnable()
        {
            //_btnToGoMods.onClick.AddListener(OnToGoModsButtonClicked);
            //_btnToGoOther.onClick.AddListener(OnToGoOtherButtonClicked);
        }

        private void OnDisable()
        {
            //_btnToGoMods.onClick.RemoveListener(OnToGoModsButtonClicked);
            //_btnToGoOther.onClick.RemoveListener(OnToGoOtherButtonClicked);
        }

        private void OnToGoModsButtonClicked()
        {
            _toGoModsRequest.OnNext(Unit.Default);
        }

        private void OnToGoOtherButtonClicked()
        {
            _toGoOtherRequest.OnNext(Unit.Default);
        }

        /// <summary>
        /// Событие запроса для открытия вспомогательного окна Режимы.
        /// </summary>
        public Observable<Unit> ToGoModsRequest => _toGoModsRequest;

        private readonly Subject<Unit> _toGoModsRequest = new();

        /// <summary>
        /// Событие запроса для открытия вспомогательного окна Дополнительно.
        /// </summary>
        public Observable<Unit> ToGoOtherRequest => _toGoOtherRequest;

        private readonly Subject<Unit> _toGoOtherRequest = new();
    }
}
