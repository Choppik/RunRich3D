using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MyBuild.Scripts.Utils.MVP.UI
{
    /// <summary>
    /// Базовый класс для привязки окна к конкретному презентору.
    /// </summary>
    /// <remarks>Полноценное окно сцены (не модульное, не высплываещее и .т. д.).</remarks>
    public abstract class WindowBinder : MonoBehaviour, IWindowBinder
    {
        public virtual void Close()
        {
            // TODO: Здесь можно прописать анимацию закрытия окна.
            Addressables.ReleaseInstance(gameObject);
            Destroy(gameObject);
        }
    }
}
