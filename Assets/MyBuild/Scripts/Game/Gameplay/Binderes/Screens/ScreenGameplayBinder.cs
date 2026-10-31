using MyBuild.Scripts.Utils.MVP.UI;
using R3;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace MyBuild.Scripts.Game.Gameplay.Binderes
{
    /// <summary>
    /// Биндер окна геймплея.
    /// </summary>
    public class ScreenGameplayBinder : WindowBinder
    {
        [SerializeField] private GameObject _objBack;
        [SerializeField] private Image _imageDollars;
        [SerializeField] private Image _imageKeys;
        [SerializeField] private Image _imageBack;
        [SerializeField] private Image[] _imagesBackGroundBlack;
        [SerializeField] private TextMeshProUGUI _labelCurrentScore;

        private void Awake()
        {
            _objBack.SetActive(false);
            // Запустить загрузку атласа и назначение спрайтов
            StartCoroutine(LoadAtlasAndAssign());
        }

        public void UpdateCurrentScore(string value)
        {
            if (!string.IsNullOrEmpty(value)) _labelCurrentScore.text = value;
        }

        private IEnumerator LoadAtlasAndAssign()
        {

            var handle = Addressables.LoadAssetAsync<SpriteAtlas>(_atlasAddress);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _atlasHandle = handle;
                //_atlasLoaded = true;
                _atlas_Menu = handle.Result;
                AddAllImages();
            }
            else
            {
                Debug.LogError($"{nameof(PopupPreviewBinder)}: failed to load atlas at '{_atlasAddress}': {handle.OperationException}");
            }
        }

        private void AddAllImages()
        {
            _imageBack.sprite = _atlas_Menu.GetSprite("Exit");
            _imageDollars.sprite = _atlas_Menu.GetSprite("dollar_Logo");
            _imageKeys.sprite = _atlas_Menu.GetSprite("key");

            foreach (var image in _imagesBackGroundBlack)
                image.sprite = _atlas_Menu.GetSprite("slot_transparent 1");
        }

        /// <summary>
        /// Событие запроса для открытия окна "Поражение".
        /// </summary>
        public Observable<Unit> LoseRequest => _loseRequest;

        private readonly Subject<Unit> _loseRequest = new();

        /// <summary>
        /// Событие клика по элементу.
        /// </summary>
        public Observable<Unit> ClickElementRequest => _clickElementRequest;

        private readonly Subject<Unit> _clickElementRequest = new();

        /// <summary>
        /// Событие выйгрыша.
        /// </summary>
        public Observable<int> LevelUPRequest => _levelUPRequest;
        
        private readonly Subject<int> _levelUPRequest = new();

        private string _atlasAddress = "Run UI";
        private SpriteAtlas _atlas_Menu;
        private AsyncOperationHandle<SpriteAtlas> _atlasHandle;
        //private bool _atlasLoaded;
    }
}
