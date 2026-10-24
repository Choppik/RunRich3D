using UnityEngine;

namespace MyBuild.Scripts.Game.Settings.Level
{
    [CreateAssetMenu(fileName = "Level", menuName = "Game Settings/Level/Lvl")]
    public class Level : ScriptableObject
    {
        public int MaxNumberLevel; // Уровень, до которого действуют текущие настройки
        public int StraightCount; // Кол-во прямых сегментов дороги
        public int TurnCount; // Кол-во поворотов сегментов дороги
        public int DoorCount; // Кол-во дверей
    }
}
