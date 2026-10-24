using UnityEngine;

namespace ButchersGames
{
    [CreateAssetMenu(menuName = "Data/Lvl")]
    public class Level : ScriptableObject
    {
        public int MaxnNumberLevel; // Уровень, до которого действуют текущие настройки
        public int StraightCount; // Кол-во прямых сегментов дороги
        public int TurnCount; // Кол-во поворотов сегментов дороги
        public int DoorCount; // Кол-во дверей
    }
}