using R3;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using MyBuild.Scripts.Game.Common;
using MyBuild.Scripts.Game.State.Entities;
using MyBuild.Scripts.Game.State.Root.Proxy;

namespace MyBuild.Scripts.Game.State.Providers
{
    /// <summary>
    /// Класс-провайдер данных для игры.
    /// </summary>
    public class PlayerPrefsGameStateProvider : IGameStateProvider
    {
        public PlayerPrefsGameStateProvider() { }

        /// <summary>
        /// Загрузка состояния игры из настроек.
        /// </summary>
        /// <returns>Текущее состояние игры.</returns>
        public Observable<GameStateProxy> LoadGameState()
        {
            // Проверка на наличие настроек (если 1 сессия, то создаем по-умолчанию).
            if (!PlayerPrefs.HasKey(GAME_STATE_KEY))
            {
                // Если не было настроек, то создание настроек по-умолчанию.
                GameState = CreateDefaultGameStateFromSettings();
                Debug.Log("Дефолт: " + nameof(CreateDefaultGameStateFromSettings) + JsonUtility.ToJson(_gameStateOrigin, true));
            }
            else
            {
                var json = PlayerPrefs.GetString(GAME_STATE_KEY);
                _gameStateOrigin = JsonUtility.FromJson<GameState>(json);
                GameState = new GameStateProxy(_gameStateOrigin);
                Debug.Log("Загрузка данных: " + nameof(LoadGameState) + json);
            }
                
            return Observable.Return(GameState);
        }

        /// <summary>
        /// Сохранение состояния игры.
        /// </summary>
        /// <returns>Удачно ли сохранено.</returns>
        public Observable<bool> SaveGameState()
        {
            var json = JsonUtility.ToJson(_gameStateOrigin, true);
            PlayerPrefs.SetString(GAME_STATE_KEY, json);

            return Observable.Return(true);
        }

        /// <summary>
        /// Сброс настроек.
        /// </summary>
        /// <returns>Удачно ли выполнен сброс.</returns>
        public Observable<bool> ResetGameState()
        {
            GameState = CreateDefaultGameStateFromSettings();
            SaveGameState();

            return Observable.Return(true);
        }

        /// <summary>
        /// Состояние по-умолчанию из настроек.
        /// </summary>
        /// <returns>Обертка над оригинальным состоянием игры.</returns>
        private GameStateProxy CreateDefaultGameStateFromSettings()
        {
            // Создаем оригинальное состояние игры.
            _gameStateOrigin = new GameState()
            {
                CurrentMapTag = AppConsts.MAIN_MAP_TAG,
                Maps = new List<MapState>()
                {
                    new()
                    {
                        Tag = AppConsts.MAIN_MAP_TAG
                    }
                }
            };

            var map = _gameStateOrigin.Maps.First(m => m.Tag == AppConsts.MAIN_MAP_TAG);

            // Добавляем все изображения.
            map.NumbersObj.AddRange(new List<NumberObjEntity>() { new() });

            return new GameStateProxy(_gameStateOrigin);
        }

        /// <summary>
        /// Текущее состояние игры.
        /// </summary>
        public GameStateProxy GameState { get; private set; }

        // Ссылка на оригинальное состояние игры.
        private GameState _gameStateOrigin;

        private const string GAME_STATE_KEY = nameof(GAME_STATE_KEY);
    }
}
