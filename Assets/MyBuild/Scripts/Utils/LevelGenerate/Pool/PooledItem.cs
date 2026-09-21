using UnityEngine;

namespace MyBuild.Scripts.Utils.LevelGenerate
{

    /// <summary>
    /// Метка пула: хранит ключ, по которому объект возвращается в PoolManager.
    /// </summary>
    public class PooledItem : MonoBehaviour
    {
        [HideInInspector] public string poolKey;
    }
}
