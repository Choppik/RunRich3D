using System;

namespace MyBuild.Scripts.Utils
{
    [Serializable]
    public class LocalizationData
    {
        public ButtonUIData buttonUIData; // Тексты кнопок.
        public CommonTextUIData commonTextUIData; // Просто тексты.
    }

    [Serializable]
    public class ButtonUIData
    {
        public string button_start;
        public string button_modes;
        public string button_other;
        public string button_auth;
        public string button_back_mainMenu;
        public string button_try_again;
        public string button_show_adv;
        public string button_leader_board;
        public string button_mod_non_stop;
        public string button_mod_challenge;
    }

    [Serializable]
    public class CommonTextUIData
    {
        public string text_level;
        public string text_stage;
        public string text_info_auth;
        public string text_best_player;
        public string text_no_games;
        public string text_record;
        public string text_info_auth_for_lose;
        public string text_music;
        public string text_rules;
        public string text_language_ru;
        public string text_language_en;
        public string text_language_tr;
        public string text_language_de;
        public string text_language_be;
        public string text_info_mode_non_stop;
        public string text_info_mode_challenge;
    }
}
