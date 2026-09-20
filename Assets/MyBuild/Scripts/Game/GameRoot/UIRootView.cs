using UnityEngine;

namespace MyBuild.Scripts.Game
{
    /// <summary>
    /// Скрипт для корневого UI, который представляет загрузочный экран и контейнер для сцен.
    /// </summary>
    public class UIRootView : MonoBehaviour
    {
        // Загрузочный экран.
        [SerializeField] private GameObject _loadingScreen;
        // Контейнер для сцены.
        [SerializeField] private Transform _uiSceneContainer;

        public void Awake()
        {
            // По-умолчанию скрываем загрузочный экран.
            HideLoadingScreen();
        }

        /// <summary>
        /// Показ загрузочного экрана.
        /// </summary>
        public void ShowLoadingScreen()
        {
            _loadingScreen.SetActive(true);
        }

        /// <summary>
        /// Скрытие загрузочного экрана.
        /// </summary>
        public void HideLoadingScreen()
        {
            _loadingScreen.SetActive(false);
        }

        /// <summary>
        /// Установка в контейнер сцены.
        /// </summary>
        /// <param name="sceneUI">Ссылка на сцену.</param>
        public void AttachSceneUI(GameObject sceneUI)
        {
            ClearSceneUI();

            sceneUI.transform.SetParent(_uiSceneContainer, false);
        }

        /// <summary>
        /// Уничтожение всех элементов сцены.
        /// </summary>
        private void ClearSceneUI()
        {
            var childCount = _uiSceneContainer.childCount;

            for (int i = 0; i < childCount; i++)
            {
                Destroy(_uiSceneContainer.GetChild(i).gameObject);
            }
        }
    }
}
