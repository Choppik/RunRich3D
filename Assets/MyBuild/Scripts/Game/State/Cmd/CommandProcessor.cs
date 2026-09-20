using System;
using System.Collections.Generic;
using MyBuild.Scripts.Game.State.Providers;

namespace MyBuild.Scripts.Game.State.Cmd
{
    public class CommandProcessor : ICommandProcessor
    {
        /// <summary>
        /// Конструктор для получения данных игры.
        /// </summary>
        /// <param name="gameStateProvider">Проводник данных игры.</param>
        public CommandProcessor(IGameStateProvider gameStateProvider) 
        {
            _gameStateProvider = gameStateProvider;
        }

        public void RegisterHandler<TCommand>(ICommandHandler<TCommand> handler) where TCommand : ICommand
        {
            _handlesMap[typeof(TCommand)] = handler;
        }

        public bool Process<TCommand>(TCommand command) where TCommand : ICommand
        {
            if (_handlesMap.TryGetValue(typeof(TCommand), out var handler))
            {
                var typedHandler = (ICommandHandler<TCommand>)handler;
                _resultCommand = typedHandler.Handle(command);

                if (_resultCommand)
                {
                    _result = typedHandler.Result();
                }
            }

            return _resultCommand;
        }

        public bool ProcessAndSave<TCommand>(TCommand command) where TCommand : ICommand
        {
            if (_handlesMap.TryGetValue(typeof(TCommand), out var handler))
            {
                var typedHandler = (ICommandHandler<TCommand>)handler;
                _resultCommand = typedHandler.Handle(command);

                if (_resultCommand)
                {
                    _result = typedHandler.Result();
                    _gameStateProvider.SaveGameState();
                }
            }

            return _resultCommand;
        }

        public object Result()
        {
            if (_resultCommand)
            {
                return _result;
            }

            return null;
        }

        private readonly IGameStateProvider _gameStateProvider;

        // Список команд.
        private readonly Dictionary<Type, object> _handlesMap = new ();
        private bool _resultCommand;
        private object _result;
    }
}
