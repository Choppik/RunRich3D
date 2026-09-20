using UnityEngine;

namespace MyBuild.Scripts.Game.Settings
{
    /// <summary>
    /// Скрипт с настройками игры.
    /// </summary>
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Game Settings/New Game Settings")]
    public class GameSettings : ScriptableObject
    {
        public GameModeSettings GameModeSettings;
    }
}
