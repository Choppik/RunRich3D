using System;
using System.Collections.Generic;
using MyBuild.Scripts.Game.State.Entities;

namespace MyBuild.Scripts.Game.State
{
    /// <summary>
    /// Класс сериализуемого состояния со всеми объектами игры для определенной карты.
    /// </summary>
    [Serializable]
    public class MapState
    {
        /// <summary>
        /// Тег карты.
        /// </summary>
        public string Tag;


        public List<NumberObjEntity> NumbersObj = new();
    }
}
