using R3;
using UnityEngine;
using System.Threading.Tasks;
using MyBuild.Scripts.Utils;

namespace MyBuild.Scripts.Game.Settings
{
    /// <summary>
    /// Провайдер данных настроек игры.
    /// </summary>
    public class SettingsProvider : ISettingsProvider
    {
        public SettingsProvider()
        {
            AppSettings = Resources.Load<AppSettings>("Settings\\AppSettings");
        }

        public Task<GameSettings> LoadGameSettings()
        {
            _gameSettings = Resources.Load<GameSettings>("Settings\\GameSettings");

            return Task.FromResult(_gameSettings);
        }

        public void SetLocalization(string language)
        {
            LocalizationSettings localizationSettings = language switch
            {
                "ru" => Resources.Load<LocalizationSettings>("Settings\\Localization\\LocalizationRuSettings"),
                _ => Resources.Load<LocalizationSettings>("Settings\\Localization\\LocalizationEnSettings"),
            };
            AppSettings.Language = language;
            
            if (null != localizationSettings)
            {
                var json = localizationSettings.JsonData.text;
                AppSettings.LocalizationData = JsonUtility.FromJson<LocalizationData>(json);
                _changeLangRequest.OnNext(Unit.Default);
            }
        }

        public Observable<Unit> ChangeLangRequest => _changeLangRequest;

        private readonly ReactiveProperty<Unit> _changeLangRequest = new();

        public AppSettings AppSettings { get; }

        public GameSettings GameSettings => _gameSettings;

        private GameSettings _gameSettings;
    }
}
