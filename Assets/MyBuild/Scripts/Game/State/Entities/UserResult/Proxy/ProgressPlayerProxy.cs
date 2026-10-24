using R3;

namespace MyBuild.Scripts.Game.State.Entities
{
    /// <summary>
    /// Класс-обертка данных игры.
    /// </summary>
    public class ProgressPlayerProxy
    {
        /// <summary>
        /// Конструктор для создания обертки над оригинальными объектами.
        /// </summary>
        /// <param name="data">Оригинальный объект.</param>
        public ProgressPlayerProxy(ProgressPlayer data)
        {
            Origin = data;

            Record = new ReactiveProperty<int>(data.Record);
            Record.Skip(1).Subscribe(value => Origin.Record = value);
            CurrentLevel = new ReactiveProperty<int>(data.CurrentLevel);
            CurrentLevel.Skip(1).Subscribe(value => Origin.CurrentLevel = value);
            Currency = new ReactiveProperty<int>(data.Currency);
            Currency.Skip(1).Subscribe(value => Origin.Currency = value);
        }

        /// <summary>
        /// Уникальный идентификатор.
        /// </summary>
        public int Id => Origin.Id;

        /// <summary>
        /// Рекорд игрока.
        /// </summary>
        public ReactiveProperty<int> Record { get; }

        /// <summary>
        /// Текущий уровень игрока.
        /// </summary>
        public ReactiveProperty<int> CurrentLevel { get; }

        /// <summary>
        /// Количество валюты игрока.
        /// </summary>
        public ReactiveProperty<int> Currency { get; }

        /// <summary>
        /// Ориганальный объект.
        /// </summary>
        public ProgressPlayer Origin { get; }
    }
}
