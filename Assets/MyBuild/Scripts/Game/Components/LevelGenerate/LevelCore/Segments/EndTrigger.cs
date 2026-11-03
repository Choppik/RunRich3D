using MyBuild.Scripts.Game.Common;
using R3;
using UnityEngine;

namespace MyBuild.Scripts.Game.Components.LevelGenerate
{
    [RequireComponent(typeof(Collider))]
    public class EndTrigger : MonoBehaviour
    {
        /// <summary>
        /// Событие конца уровня.
        /// </summary>
        public static Observable<Unit> OnEndLevel => _onEndLevel;

        private static readonly Subject<Unit> _onEndLevel = new();

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Конец уровня");
            if (!other.CompareTag(AppConsts.Player))
                return;

            _onEndLevel.OnNext(Unit.Default);
        }
    }
}
