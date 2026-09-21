using UnityEngine;

namespace ButchersGames
{
    [CreateAssetMenu(menuName = "Data/Lvl")]
    public class Level : ScriptableObject
    {
        private int numberLevel; // Здесь можно добавить нетолько номер уровня, но и другие данные, например, название другой текстуры неба или воды и т.д.
    }
}