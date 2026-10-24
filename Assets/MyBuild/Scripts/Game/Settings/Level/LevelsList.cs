using System.Collections.Generic;
using UnityEngine;

namespace MyBuild.Scripts.Game.Settings.Level
{
    [CreateAssetMenu(fileName = "LevelList", menuName = "Game Settings/Level/LvlList")]
    public class LevelsList : ScriptableObject
    {
        public List<Level> Lvls; // Список настроек уровней.
    }
}
