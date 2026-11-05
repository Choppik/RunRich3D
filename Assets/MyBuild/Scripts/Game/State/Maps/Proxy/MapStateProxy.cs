using MyBuild.Scripts.Game.State.Entities;

namespace MyBuild.Scripts.Game.State
{
    /// <summary>
    /// Класс-обертка над оригинальным объектом, представляющий состояние всех объектов игры для конкретной карты.
    /// </summary>
    public class MapStateProxy
    {
        /// <summary>
        /// Конструктор для создания обертки над состоянием игры.
        /// </summary>
        /// <param name="mapState">Ориганальное состояние.</param>
        public MapStateProxy(MapState mapState)
        {
            Origin = mapState;

            Progress = new ProgressPlayerProxy(mapState.ProgressPlayer);
        }

        /// <summary>
        /// Данные игрока.
        /// </summary>
        public ProgressPlayerProxy Progress { get; }

        /// <summary>
        /// Ориганальное состояние карты.
        /// </summary>
        public MapState Origin { get; }

        /// <summary>
        /// Тег карты.
        /// </summary>
        public string Tag => Origin.Tag;
    }
}
