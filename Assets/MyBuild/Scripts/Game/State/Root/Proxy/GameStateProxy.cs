using R3;
using System.Linq;
using ObservableCollections;

namespace MyBuild.Scripts.Game.State.Root.Proxy
{
    /// <summary>
    /// Класс-обертка над оригинальным объектом, представляющий состояние всех карт.
    /// </summary>
    public class GameStateProxy
    {
        /// <summary>
        /// Конструктор для создания обертки над состоянием карт.
        /// </summary>
        /// <param name="gameState">Ориганальное состояние.</param>
        public GameStateProxy(GameState gameState)
        {
            // Перебор объектов оригинального сотояния и создания их оберток.
            gameState.Maps.ForEach(mapOrigin => Maps.Add(new MapStateProxy(mapOrigin)));

            /// В данном месте нужна подписка, если во время игры что-то будет добавлятся.

            // Подписываемся на добавление в список новых объектов и добавляем их сразу в оригинальное состояние.
            Maps.ObserveAdd().Subscribe(e =>
            {
                var addedMap = e.Value;
                _gameState.Maps.Add(addedMap.Origin);
            });

            // Подписываемся на удаление объеков.
            Maps.ObserveRemove().Subscribe(e =>
            {
                var removeMap = e.Value;
                var removeMapState = gameState.Maps.First(m => m.Tag == removeMap.Tag);
                _gameState.Maps.Remove(removeMapState);
            });

            ///

            _gameState = gameState;
        }

        /// <summary>
        /// Получение текущей карты.
        /// </summary>
        /// <returns>Обертка над текущей картой.</returns>
        public MapStateProxy GetGurrentMap()
        {
            return Maps.First(m => m.Tag == _gameState.CurrentMapTag);
        }

        /// <summary>
        /// Получение уникального идентификатора.
        /// </summary>
        /// <returns>Уникальный идентификатор.</returns>
        public int GetEntityId()
        {
            return _gameState.CreateEntityId();
        }

        /// <summary>
        /// Список всех картинок.
        /// </summary>
        public ObservableList<MapStateProxy> Maps { get; } = new();

        // Ссылка на оригинальне состояние игры.
        private readonly GameState _gameState;
    }
}
