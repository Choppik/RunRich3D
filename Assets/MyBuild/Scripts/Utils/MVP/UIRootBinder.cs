using UnityEngine;

namespace MyBuild.Scripts.Utils.MVP.UI
{
    /// <summary>
    /// Биндер корневого UI сцены.
    /// </summary>
    public class UIRootBinder : MonoBehaviour
    {
        [SerializeField] private Transform _screenContainer;
        [SerializeField] private Transform _popupsContainer;

        public IWindowBinder OpenScreen(string prefabPath)
        {
            var prefab = Resources.Load<GameObject>(prefabPath);
            var createdScreen = Instantiate(prefab, _screenContainer);
            var binder = createdScreen.GetComponent<IWindowBinder>();

            Debug.Log($"Создаю экран {binder.GetType().Name}");

            return binder;
        }

        public IWindowBinder OpenPopup(string prefabPath)
        {
            var prefab = Resources.Load<GameObject>(prefabPath);
            var createdPopup = Instantiate(prefab, _popupsContainer);
            var binder = createdPopup.GetComponent<IWindowBinder>();

            Debug.Log($"Создаю экран попапа {binder.GetType().Name}");

            return binder;
        }
    }
}
