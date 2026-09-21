using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace MyBuild.Scripts.Utils
{
    public class AddressablesSceneLoaderCoroutine : MonoBehaviour
    {
        public IEnumerator LoadSceneCoroutine(string address, bool additive = false)
        {
            var op = Addressables.LoadSceneAsync(address, additive ? LoadSceneMode.Additive : LoadSceneMode.Single, activateOnLoad: true);
            while (!op.IsDone)
            {
                yield return null;
            }

            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                SceneManager.SetActiveScene(op.Result.Scene);
            }
            else
            {
                Debug.LogError("Load failed: " + op.OperationException);
            }
        }

        public IEnumerator UnloadSceneCoroutine(AsyncOperationHandle<SceneInstance> handle)
        {
            var unload = Addressables.UnloadSceneAsync(handle, true);
            while (!unload.IsDone) yield return null;
        }
    }
}
