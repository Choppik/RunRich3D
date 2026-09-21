using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MyBuild.Scripts.Game.Settings
{
    /// <summary>
    /// Провайдер данных настроек игры (Addressables).
    /// - Вызовите InitializeAsync() на старте приложения, чтобы загрузить AppSettings.
    /// - LoadGameSettings() загружает и кэширует GameSettings по адресу "Settings/GameSettings".
    /// </summary>
    public class SettingsProvider : ISettingsProvider
    {
        // Адреса Addressables (должны совпадать с адресами в Addressables Groups)
        private const string AppSettingsAddress = "AppSettings";
        private const string GameSettingsAddress = "GameSettings";

        public SettingsProvider()
        {
            // Конструктор оставляем быстрым — загрузка выполняется в InitializeAsync
        }

        /// <summary>
        /// Асинхронная инициализация провайдера — загружает AppSettings.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_appSettings != null) return;

            try
            {
                var handle = Addressables.LoadAssetAsync<AppSettings>(AppSettingsAddress);
                await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _appSettings = handle.Result;
                    // Не вызывать Release — мы хотим иметь ссылку на ассет в памяти.
                    // Если требуется выгружать позже, храните handle и вызывайте Addressables.Release(handle).
                }
                else
                {
                    Debug.LogError($"Failed to load AppSettings at '{AppSettingsAddress}': {handle.OperationException}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception while loading AppSettings: {ex}");
            }
        }

        /// <summary>
        /// Загрузить настройки игры (отложенно, кэшируются после первой загрузки).
        /// Адрес ассета должен быть настроен в Addressables: "GameSettings".
        /// </summary>
        public async Task<GameSettings> LoadGameSettings()
        {
            if (_gameSettings != null) return _gameSettings;

            try
            {
                var handle = Addressables.LoadAssetAsync<GameSettings>(GameSettingsAddress);
                await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _gameSettings = handle.Result;
                }
                else
                {
                    Debug.LogError($"Failed to load GameSettings at '{GameSettingsAddress}': {handle.OperationException}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception while loading GameSettings: {ex}");
            }

            return _gameSettings;
        }

        /// <summary>
        /// Асинхронный доступ к AppSettings — вернёт null, если InitializeAsync не был вызван или загрузка не удалась.
        /// </summary>
        public AppSettings AppSettings => _appSettings;

        public GameSettings GameSettings => _gameSettings;

        private AppSettings _appSettings;
        private GameSettings _gameSettings;
    }
}