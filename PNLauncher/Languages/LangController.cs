namespace PNLauncher.Languages
{
    using PNLauncher.Core;
    using PNLauncher.Help;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    public static class LangController
    {
        public const string AuthKey = "launcher.auth";
        private const string LanguagesFile = "lang/lang.tsv";
        public const string DefaultLanguageFile = "lang/lang.default";
        private const string DefaultLanguage = "English";
        private static Dictionary<string, int> _langsCodeIds = new Dictionary<string, int>();
        private static Dictionary<string, Dictionary<string, string>> _langsList = new Dictionary<string, Dictionary<string, string>>();
        private static Dictionary<UpdateStatus, string> _update_statuses = new Dictionary<UpdateStatus, string>();
        private static string _lang = string.Empty;

        public static string[] GetAvaibleLanguages() => 
            _langsList.Keys.ToArray<string>();

        public static string GetTranslate(string value) => 
            (CurrentLanguage != string.Empty) ? (_langsList[CurrentLanguage].ContainsKey(value) ? _langsList[CurrentLanguage][value] : ("(" + value + ")")) : ("(" + value + ")");

        public static string GetTranslate(string lang, string value) => 
            _langsList[lang][value];

        public static string GetUpdateTagByStatus(UpdateStatus st) => 
            _update_statuses.ContainsKey(st) ? _update_statuses[st] : string.Empty;

        public static bool InitLanguage()
        {
            if (File.Exists("lang/lang.default"))
            {
                string str2 = File.ReadAllText("lang/lang.default");
                if (LangExists(str2))
                {
                    return SetLanguage(str2);
                }
            }
            char[] separator = new char[] { ' ' };
            string lang = CultureInfo.CurrentCulture.EnglishName.Split(separator)[0];
            return (!LangExists(lang) ? SetLanguage("English") : SetLanguage(lang));
        }

        public static bool LangExists(string lang) => 
            _langsList.ContainsKey(lang);

        public static int LoadLanguages()
        {
            static Dictionary<string, string> loadlang(string data)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                string[] strArray = data.Substrings("</", ">", 0);
                int num = strArray.Length;
                for (int j = 0; j < num; j++)
                {
                    string str = strArray[j];
                    if (!string.IsNullOrEmpty(str))
                    {
                        dictionary.Add(str, data.Substring("<" + str + ">", "</" + str + ">", 0));
                    }
                }
                return dictionary;
            }
            if (!File.Exists("lang/lang.tsv"))
            {
                return -1;
            }
            string[] strArray = File.ReadAllText("lang/lang.tsv").Substrings("<lang>", "</lang>", 0);
            int length = strArray.Length;
            int num2 = 0;
            for (int i = 0; i < length; i++)
            {
                string str = strArray[i];
                if (!string.IsNullOrEmpty(str))
                {
                    string key = str.Substring("<name>", "</name>", 0);
                    _langsCodeIds.Add(key, int.Parse(str.Substring("<encoding_id>", "</encoding_id>", 0)));
                    _langsList.Add(key, loadlang(str));
                    num2++;
                }
            }
            _update_statuses.Add(UpdateStatus.CheckFiles, "update_check_files");
            _update_statuses.Add(UpdateStatus.DownloadFiles, "update_download_files");
            _update_statuses.Add(UpdateStatus.GettingInfo, "update_getting_info");
            _update_statuses.Add(UpdateStatus.StartUpdate, "update_start");
            _update_statuses.Add(UpdateStatus.CheckSuccess, "update_check_success");
            _update_statuses.Add(UpdateStatus.UpdateSuccess, "update_success");
            _update_statuses.Add(UpdateStatus.UpdatePaused, "update_paused");
            _update_statuses.Add(UpdateStatus.UpdateEnd, "update_end");
            _update_statuses.Add(UpdateStatus.LastVersion, "update_last");
            _update_statuses.Add(UpdateStatus.CopyFiles, "copy_files");
            return num2;
        }

        public static bool SetLanguage(string lang)
        {
            if (string.IsNullOrEmpty(lang))
            {
                return false;
            }
            File.WriteAllText("lang/lang.default", lang);
            _lang = lang;
            return true;
        }

        public static string CurrentLanguage =>
            _lang;

        public static int GetCodeId =>
            _langsCodeIds[CurrentLanguage];
    }
}

