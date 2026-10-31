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

        #region Обозначения локализации.

        public const string LocalizationRU = "ru";
        public const string LocalizationEN = "en";

        #endregion

        #region Адреса ресурсов.

        public const string PickupPositive = nameof(PickupPositive);
        public const string PickupNegative = nameof(PickupNegative);
        public const string Player = nameof(Player);
        public const string Ground = nameof(Ground);
        public const string GroundL = nameof(GroundL);
        public const string GroundR = nameof(GroundR);
        public const string GroundEND = nameof(GroundEND);
        public const string Door1 = nameof(Door1);
        public const string Door2 = nameof(Door2);
        public const string Door3 = nameof(Door3);
        public const string Door4 = nameof(Door4);

        #endregion
    }
}