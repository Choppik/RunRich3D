using UnityEngine;
using MyBuild.Scripts.Utils;

namespace MyBuild.Scripts.Game.Settings
{
    /// <summary>
    /// Скрипт с настройками приложения.
    /// </summary>
    [CreateAssetMenu(fileName = "AppSettings", menuName = "Game Settings/New Application Settings")]
    public class AppSettings : ScriptableObject
    {
        public string Language; // Выбранный язык.

        public LocalizationData LocalizationData; // Данные текущей локализации.
    }
}
