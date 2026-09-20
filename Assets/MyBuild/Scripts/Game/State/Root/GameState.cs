using System;
using System.Collections.Generic;

namespace MyBuild.Scripts.Game.State
{
    /// <summary>
    /// Класс сериализуемого списка со всеми картами игры.
    /// </summary>
    [Serializable]
    public class GameState
    {
        /// <summary>
        /// Индефикатор сущности (любой объект) в игре.
        /// </summary>
        public int GlobalEntityId;

        /// <summary>
        /// Текущая карта.
        /// </summary>
        public string CurrentMapTag;

        /// <summary>
        /// Список всех карт игры.
        /// </summary>
        public List<MapState> Maps = new();

        /// <summary>
        /// Создание уникального идентификатора.
        /// </summary>
        /// <returns>Уникальный идентификатор.</returns>
        public int CreateEntityId()
        {
            // В данной игре не используется,
            // но пригодится в играх, где есть динамическое создание объектов во время игры.
            return GlobalEntityId++;
        }
    }
}
