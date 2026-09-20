using UnityEngine;

namespace MyBuild.Scripts.Game.Settings
{
    /// <summary>
    /// Скрипт с настройками языка игры.
    /// </summary>
    [CreateAssetMenu(fileName = "LocalizationSettings", menuName = "Game Settings/New Localization Settings")]
    public class LocalizationSettings : ScriptableObject
    {
        public string Language; // Язык.
        public TextAsset JsonData; // Файл с данными для текущего языка.
    }
}