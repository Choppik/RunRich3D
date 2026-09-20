using UnityEngine;
using MyBuild.Scripts.Game.Common;

namespace MyBuild.Scripts.Game.Settings
{
    /// <summary>
    /// Скрипт с настройками режима игры.
    /// </summary>
    [CreateAssetMenu(fileName = "GameModeSettings", menuName = "Game Settings/New Game Mode Settings")]
    public class GameModeSettings : ScriptableObject
    {
        //public Mods CurrentMode; // Выбранный режим игры.
    }
}