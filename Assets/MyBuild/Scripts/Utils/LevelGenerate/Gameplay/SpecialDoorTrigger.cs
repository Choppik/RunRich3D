using MyBuild.Scripts.Game.Common;
using UnityEngine;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    public class SpecialDoorTrigger : MonoBehaviour
    {
        private SpecialDoorType _type;
        private SpecialDoorSpawner _spawner;
        private bool _triggered = false;

        public void Init(
            SpecialDoorType type,
            Vector3 triggerSize,
            SpecialDoorSpawner spawner)
        {
            _type = type;
            _spawner = spawner;

            // Добавляем или настраиваем коллайдер-триггер
            if (!TryGetComponent<BoxCollider>(out var col))
                col = gameObject.AddComponent<BoxCollider>();

            col.size = triggerSize;
            col.isTrigger = true;
            col.center = Vector3.zero;
        }

        void OnTriggerEnter(Collider other)
        {
            if (_triggered) return;
            if (!other.CompareTag("Player")) return;

            _triggered = true;
            _spawner.HandleDoorPassed(_type);
        }

        void OnDisable()
        {
            _triggered = false; // сброс при переиспользовании
        }
    }
}