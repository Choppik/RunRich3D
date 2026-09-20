using R3;

namespace MyBuild.Scripts.Game.State.Entities
{
    /// <summary>
    /// Класс-обертка данных игры.
    /// </summary>
    public class NumberObjEntityProxy
    {
        /// <summary>
        /// Конструктор для создания обертки над оригинальными объектами.
        /// </summary>
        /// <param name="data">Оригинальный объект.</param>
        public NumberObjEntityProxy(NumberObjEntity data)
        {
            Origin = data;

            Result = new ReactiveProperty<int>(data.Result);
            Result.Skip(1).Subscribe(value => Origin.Result = value);
        }

        /// <summary>
        /// Уникальный идентификатор.
        /// </summary>
        public int Id => Origin.Id;

        /// <summary>
        /// Рекорд игрока.
        /// </summary>
        public ReactiveProperty<int> Result { get; }

        /// <summary>
        /// Ориганальный объект.
        /// </summary>
        public NumberObjEntity Origin { get; }
    }
}
