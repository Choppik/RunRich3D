namespace MyBuild.Scripts.Game.State.Cmd
{
    /// <summary>
    /// Интерфейс для работы с командами.
    /// </summary>
    public interface ICommandProcessor
    {
        /// <summary>
        /// Инициализация обработчика команды.
        /// </summary>
        /// <typeparam name="TCommand">Тип команды.</typeparam>
        /// <param name="handler">Обработчик команды.</param>
        void RegisterHandler<TCommand>(ICommandHandler<TCommand> handler) where TCommand : ICommand;

        /// <summary>
        /// Процесс обработки команды с сохранением состояния игры после выполнения команды.
        /// </summary>
        /// <typeparam name="TCommand">Тип команды.</typeparam>
        /// <param name="command">Команда.</param>
        /// <returns>Удачно ли выполнилась команда.</returns>
        bool ProcessAndSave<TCommand>(TCommand command) where TCommand : ICommand;

        /// <summary>
        /// Процесс обработки команды.
        /// </summary>
        /// <typeparam name="TCommand">Тип команды.</typeparam>
        /// <param name="command">Команда.</param>
        /// <returns>Удачно ли выполнилась команда.</returns>
        bool Process<TCommand>(TCommand command) where TCommand : ICommand;

        /// <summary>
        /// Возвращение последнего результата выполнения какой-либо команды.
        /// </summary>
        /// <returns>Какое-то значение, если команда выполнилась, иначе null.</returns>
        object Result();
    }
}
