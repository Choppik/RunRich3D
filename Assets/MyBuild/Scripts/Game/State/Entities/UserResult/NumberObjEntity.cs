using System;

namespace MyBuild.Scripts.Game.State.Entities
{
    /// <summary>
    /// Класс сериализуемых данных игры.
    /// </summary>
    [Serializable]
    public class NumberObjEntity : Entity
    {
        /// <summary>
        /// Рекорд игрока.
        /// </summary>
        public int Result;
    }
}
