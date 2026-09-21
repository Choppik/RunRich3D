using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    /// <summary>
    /// Загружает модельку и спрайты через Addressables в пустой префаб пикапа.
    /// </summary>
    public class PickupInitializer : MonoBehaviour
    {
        [Header("Addressable Keys")]
        public string coinModelKey;
        public string atlasKey1;
        public string atlasKey2;
        public string spriteName1;
        public string spriteName2;

        [Header("Дочерние объекты")]
        public Transform coinHolder;
        public Image image1;
        public Image image2;

        private bool _isReady = false;
        private GameObject _coinInstance;

        public bool IsReady => _isReady;

        public IEnumerator Initialize()
        {
            _isReady = false;

            // Моделька
            if (!string.IsNullOrEmpty(coinModelKey) && coinHolder != null)
            {
                var load = Addressables.LoadAssetAsync<GameObject>(coinModelKey);
                yield return load;
                if (load.Status == AsyncOperationStatus.Succeeded && load.Result != null)
                {
                    _coinInstance = Instantiate(load.Result, coinHolder);
                    _coinInstance.transform.localPosition = Vector3.zero;
                }
                Addressables.Release(load);
            }

            // Атлас 1
            if (!string.IsNullOrEmpty(atlasKey1) && image1 != null)
            {
                var load = Addressables.LoadAssetAsync<SpriteAtlas>(atlasKey1);
                yield return load;
                if (load.Status == AsyncOperationStatus.Succeeded && load.Result != null)
                {
                    var sprite = load.Result.GetSprite(spriteName1);
                    if (sprite != null) image1.sprite = sprite;
                }
                Addressables.Release(load);
            }

            // Атлас 2
            if (!string.IsNullOrEmpty(atlasKey2) && image2 != null)
            {
                var load = Addressables.LoadAssetAsync<SpriteAtlas>(atlasKey2);
                yield return load;
                if (load.Status == AsyncOperationStatus.Succeeded && load.Result != null)
                {
                    var sprite = load.Result.GetSprite(spriteName2);
                    if (sprite != null) image2.sprite = sprite;
                }
                Addressables.Release(load);
            }

            _isReady = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            // Определяем тип (positive / negative) — можно через тег или поле
            bool isPositive = gameObject.CompareTag("PositivePickup");
            int value = isPositive ? 1 : -1;

            if (ScoreManager.Instance != null)
            {
                if (isPositive) ScoreManager.Instance.CollectPositive();
                else ScoreManager.Instance.CollectNegative();
            }

            // Партикл-эффект
            var effect = GetComponentInParent<PickupEffect>();
            if (effect != null)
                effect.PlayAt(transform.position);

            // Возвращаем в пул
            gameObject.SetActive(false);
            var obj = PoolManager.Instance.Spawn(gameObject);
            PoolManager.Instance.Despawn(obj);
        }
    }

}
