namespace MyBuild.Scripts.Game.Common
{
    /// <summary>
    /// Все константные значения в приложении.
    /// </summary>
    public static class AppConsts
    {
        #region Флаги.

        public const string CurrentTimer = nameof(CurrentTimer);
        public const string CurrentElementLifetime = nameof(CurrentElementLifetime);

        #endregion

        #region Теги карт.

        public const string MAIN_MAP_TAG = nameof(MAIN_MAP_TAG);

        #endregion

        #region Теги всех звуков.

        public const string Background = nameof(Background);
        public const string Lose = nameof(Lose);
        public const string LevelUP = nameof(LevelUP);
        public const string Pop = nameof(Pop);
        public const string Click = nameof(Click);

        #endregion

        #region Пути к ресурсам.

        public const string PathPrefabsGameplay = "Prefabs/UI/Gameplay/";
        public const string PathPrefabsRoot = "Prefabs/UI/Root/";

        #endregion

        #region Обозначения локализации.

        public const string LocalizationRU = "ru";
        public const string LocalizationEN = "en";

        #endregion
    }
}