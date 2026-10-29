using UnityEngine;

namespace MyBuild.Scripts.Game.Settings.Level
{
    [CreateAssetMenu(fileName = "Level", menuName = "Game Settings/Level/Lvl")]
    public class Level : ScriptableObject
    {
        public int MaxNumberLevel; // Уровень, до которого действуют текущие настройки
        public int DoorCount; // Кол-во дверей
        public int MinTurnsPerChunk; // Минимальное кол-во поворотов
        public int ChanceDoor; // Шанс спавна двери
        public int ExtraTurnChance; // Шанс спавна поворота
    }
}
