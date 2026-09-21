using UnityEngine;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    /// <summary>
    /// Финальный триггер. Останавливает движение и вызывает презентер UI.
    /// </summary>
    public class EndTrigger : MonoBehaviour
    {
        [Header("Result")]
        public bool isVictory = true;

        private bool _triggered = false;

        void OnTriggerEnter(Collider other)
        {
            if (_triggered) return;
            if (!other.CompareTag("Player")) return;

            _triggered = true;
            OnPlayerReached();
        }

        /// <summary>Вызывается из RoadSegment, если EndTrigger стоит после последней дороги.</summary>
        public void OnPlayerReached()
        {
            if (_triggered) return;
            _triggered = true;

            var player = FindFirstObjectByType<PlayerController>();
            if (player != null) player.StopMovement();

            // Уведомляем ScoreManager
            if (ScoreManager.Instance != null)
            {
                if (isVictory)
                    ScoreManager.Instance.OnVictory();
                else
                    ScoreManager.Instance.OnDefeat();
            }

            Debug.Log("[EndTrigger] Игрок достиг конца уровня.");
        }
    }
}
