using R3;
using System.Linq;
using ObservableCollections;
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

            // Перебор объектов оригинального сотояния и создания их оберток.
            mapState.NumbersObj.ForEach(pictureOrigin => Dataes.Add(new NumberObjEntityProxy(pictureOrigin)));

            /// В данном месте нужна подписка, если во время игры что-то будет добавлятся.

            // Подписываемся на добавление в список новых объектов и добавляем их сразу в оригинальное состояние.
            Dataes.ObserveAdd().Subscribe(e =>
            {
                var addedBuildingEntity = e.Value;
                mapState.NumbersObj.Add(addedBuildingEntity.Origin);
            });

            // Подписываемся на удаление объеков.
            Dataes.ObserveRemove().Subscribe(e =>
            {
                var removeBuildingEntityProxy = e.Value;
                var removeBuildingEntity = mapState.NumbersObj.FirstOrDefault(b => b.Id == removeBuildingEntityProxy.Id);
                mapState.NumbersObj.Remove(removeBuildingEntity);
            });

            ///
        }

        /// <summary>
        /// Список всех данных.
        /// </summary>
        public ObservableList<NumberObjEntityProxy> Dataes { get; } = new();

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
