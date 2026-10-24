using System;

namespace MyBuild.Scripts.Game.State.Entities
{
    /// <summary>
    /// Класс сериализуемых данных игры.
    /// </summary>
    [Serializable]
    public class ProgressPlayer : Entity
    {
        /// <summary>
        /// Рекорд игрока.
        /// </summary>
        public int Record;

        /// <summary>
        /// Текущий уровень.
        /// </summary>
        public int CurrentLevel;

        /// <summary>
        /// Количество текущей валюты.
        /// </summary>
        public int Currency;
    }
}
