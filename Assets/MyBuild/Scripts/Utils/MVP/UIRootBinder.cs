using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MyBuild.Scripts.Utils.MVP.UI
{
    /// <summary>
    /// Биндер корневого UI сцены — теперь использует Addressables.
    /// Методы возвращают IWindowBinder асинхронно. Для кода на корутинах есть удобные IEnumerator-обёртки.
    /// </summary>
    public class UIRootBinder : MonoBehaviour
    {
        [SerializeField] private Transform _screenContainer;
        [SerializeField] private Transform _popupsContainer;

        /// <summary>
        /// Асинхронно загружает и инстанцирует экран по адресу Addressables.
        /// Префаб должен быть помечен как Addressable и иметь правильный адрес.
        /// </summary>
        public async Task<IWindowBinder> OpenScreenAsync(string prefabAddress)
        {
            if (string.IsNullOrEmpty(prefabAddress))
            {
                Debug.LogError("OpenScreenAsync: prefabAddress is null or empty");
                return null;
            }

            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(prefabAddress, _screenContainer);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"OpenScreenAsync: failed to instantiate '{prefabAddress}': {handle.OperationException}");
                return null;
            }

            var createdScreen = handle.Result;
            if (createdScreen == null)
            {
                Debug.LogError($"OpenScreenAsync: instantiated GO is null for '{prefabAddress}'");
                return null;
            }

            var binder = createdScreen.GetComponent<IWindowBinder>();
            if (binder == null)
            {
                Debug.LogWarning($"OpenScreenAsync: created screen does not contain IWindowBinder. Adding fallback component.");
                // Если вашей архитектуре это не подходит — лучше убедиться, что префаб содержит IWindowBinder
            }

            Debug.Log($"Создаю экран {(binder != null ? binder.GetType().Name : createdScreen.name)} по адресу '{prefabAddress}'");

            return binder;
        }

        /// <summary>
        /// Асинхронно загружает и инстанцирует попап по адресу Addressables.
        /// </summary>
        public async Task<IWindowBinder> OpenPopupAsync(string prefabAddress)
        {
            if (string.IsNullOrEmpty(prefabAddress))
            {
                Debug.LogError("OpenPopupAsync: prefabAddress is null or empty");
                return null;
            }

            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(prefabAddress, _popupsContainer);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"OpenPopupAsync: failed to instantiate '{prefabAddress}': {handle.OperationException}");
                return null;
            }

            var createdPopup = handle.Result;
            if (createdPopup == null)
            {
                Debug.LogError($"OpenPopupAsync: instantiated popup is null for '{prefabAddress}'");
                return null;
            }

            var binder = createdPopup.GetComponent<IWindowBinder>();
            if (binder == null)
            {
                Debug.LogWarning($"OpenPopupAsync: created popup does not contain IWindowBinder. Adding fallback component.");
            }

            Debug.Log($"Создаю экран попапа {(binder != null ? binder.GetType().Name : createdPopup.name)} по адресу '{prefabAddress}'");

            return binder;
        }

        // --- Удобные корутинные обёртки для кода, который использует IEnumerator ---

        public IEnumerator OpenScreenCoroutine(string prefabAddress, Action<IWindowBinder> onComplete)
        {
            var op = Addressables.InstantiateAsync(prefabAddress, _screenContainer);
            yield return op;

            if (op.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"OpenScreenCoroutine: failed to instantiate '{prefabAddress}': {op.OperationException}");
                onComplete?.Invoke(null);
                yield break;
            }

            var created = op.Result;
            var binder = created != null ? created.GetComponent<IWindowBinder>() : null;
            onComplete?.Invoke(binder);
        }

        public IEnumerator OpenPopupCoroutine(string prefabAddress, Action<IWindowBinder> onComplete)
        {
            var op = Addressables.InstantiateAsync(prefabAddress, _popupsContainer);
            yield return op;

            if (op.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"OpenPopupCoroutine: failed to instantiate '{prefabAddress}': {op.OperationException}");
                onComplete?.Invoke(null);
                yield break;
            }

            var created = op.Result;
            var binder = created != null ? created.GetComponent<IWindowBinder>() : null;
            onComplete?.Invoke(binder);
        }
    }
}