using R3;
using System.Threading.Tasks;

namespace MyBuild.Scripts.Game.Settings
{
    /// <summary>
    /// Интерфейс проводника данных настроек.
    /// </summary>
    public interface ISettingsProvider
    {
        /// <summary>
        /// Настройки конкретной состовляющей части игры (дургой уровень и т.д.).
        /// </summary>
        GameSettings GameSettings { get; }

        /// <summary>
        /// Настройки всего приложения.
        /// </summary>
        AppSettings AppSettings { get; }

        /// <summary>
        /// Асихронная загрузка данных игры.
        /// </summary>
        /// <returns>Настройки игры.</returns>
        /// <remarks>Можно реализовать загрузку данных откуда угодно.</remarks>
        Task<GameSettings> LoadGameSettings();

        /// <summary>
        /// Установка локализации.
        /// </summary>
        /// <param name="language">Выбранный язык локализации.</param>
        void SetLocalization(string language);

        /// <summary>
        /// Событие изменения языка.
        /// </summary>
        Observable<Unit> ChangeLangRequest { get; }
    }
}
