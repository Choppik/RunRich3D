namespace MyBuild.Scripts.Game.State.Cmd
{
    public interface ICommandHandler<TCommand> where TCommand : ICommand
    {
        /// <summary>
        /// Обработчик команды.
        /// </summary>
        /// <param name="command">Команда.</param>
        /// <returns>Удачно ли выполнилась команда.</returns>
        bool Handle (TCommand command);

        /// <summary>
        /// Возвращение какого-либо результата по завершении команды.
        /// </summary>
        /// <returns>Какое-то значение, если команда выполнилась, иначе null.</returns>
        object Result();
    }
}
