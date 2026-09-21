using MyBuild.Scripts.Utils.MVP.UI;
using R3;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace MyBuild.Scripts.Game.Gameplay.Binderes
{
    /// <summary>
    /// Биндер превью перед игрой.
    /// </summary>
    public class PopupPreviewBinder : PopupBinder
    {
        [SerializeField] private Button _btnToGo;
        [SerializeField] private Image _imageBackGroundGrey;
        [SerializeField] private Image _imageMainBackLeft;
        [SerializeField] private Image _imageMainBackRight;
        [SerializeField] private Image _imageHouse;
        [SerializeField] private Image _imageShop;
        [SerializeField] private Image _imageHand;
        [SerializeField] private Image _imageAddDollars;
        [SerializeField] private Image _imageArrow;
        [SerializeField] private Image _imageSettings;
        [SerializeField] private Image[] _imagesBackGroundBlack;
        [SerializeField] private Image[] _imagesBackGroundWhite;
        [SerializeField] private Image[] _imagesBackGroundLine;

        private void Awake()
        {
            // Запустить загрузку атласа и назначение спрайтов
            StartCoroutine(LoadAtlasAndAssign());
        }

        private void OnEnable()
        {
            if (_btnToGo != null)
            {
                // Добавляем EventTrigger для PointerDown / PointerUp (если его нет)
                _eventTrigger = _btnToGo.gameObject.GetComponent<EventTrigger>();
                if (_eventTrigger == null) _eventTrigger = _btnToGo.gameObject.AddComponent<EventTrigger>();

                // PointerDown
                var entryDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
                entryDown.callback.AddListener((data) => OnPointerDownInternal((PointerEventData)data));
                _eventTrigger.triggers.Add(entryDown);

                // PointerUp
                var entryUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                entryUp.callback.AddListener((data) => OnPointerUpInternal((PointerEventData)data));
                _eventTrigger.triggers.Add(entryUp);
            }
            else
            {
                Debug.LogWarning("PopupPreviewBinder: _btnToGo is not assigned.");
            }
        }

        private void OnDisable()
        {
            if (_atlasLoaded)
            {
                try
                {
                    Addressables.Release(_atlasHandle);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"PopupPreviewBinder: error releasing atlas handle: {e}");
                }
                _atlasLoaded = false;
                _atlas_Menu = null;
            }
        }

        private void OnPointerDownInternal(PointerEventData data)
        {
            _toGoRequest.OnNext(true);
        }

        private void OnPointerUpInternal(PointerEventData data)
        {
            _toGoRequest.OnNext(false);
        }

        private IEnumerator LoadAtlasAndAssign()
        {

           var handle = Addressables.LoadAssetAsync<SpriteAtlas>(_atlasAddress);
           yield return handle;

           if (handle.Status == AsyncOperationStatus.Succeeded)
           {
               _atlasHandle = handle;
               _atlasLoaded = true;
               _atlas_Menu = handle.Result;
                AddAllImages();
           }
           else
           {
               Debug.LogError($"PopupPreviewBinder: failed to load atlas at '{_atlasAddress}': {handle.OperationException}");
           }

        }

        private void AddAllImages()
        {
            _imageHand.sprite =_atlas_Menu.GetSprite("hand");
            _imageArrow.sprite =_atlas_Menu.GetSprite("arrow_left_right");
            _imageMainBackLeft.sprite =_atlas_Menu.GetSprite("default");
            _imageMainBackRight.sprite =_atlas_Menu.GetSprite("city_orange");
            _imageSettings.sprite =_atlas_Menu.GetSprite("settings");
            _imageShop.sprite =_atlas_Menu.GetSprite("shop_skin");
            _imageHouse.sprite =_atlas_Menu.GetSprite("house");
            _imageAddDollars.sprite =_atlas_Menu.GetSprite("pickups");

            foreach (var image in _imagesBackGroundBlack)
                image.sprite = _atlas_Menu.GetSprite("9grid_black_transparent");
            foreach (var image in _imagesBackGroundWhite)
                image.sprite = _atlas_Menu.GetSprite("Circle");
            foreach (var image in _imagesBackGroundLine)
                image.sprite = _atlas_Menu.GetSprite("rounded-rectangle");
        }

        /// <summary>
        /// Событие запроса для закрытия превью.
        /// </summary>
        public Observable<bool> ToGoRequest => _toGoRequest;
        private readonly Subject<bool> _toGoRequest = new();

        private EventTrigger _eventTrigger;
        private string _atlasAddress = "Main UI";
        private SpriteAtlas _atlas_Menu;
        private AsyncOperationHandle<SpriteAtlas> _atlasHandle;
        private bool _atlasLoaded;
    }
}
