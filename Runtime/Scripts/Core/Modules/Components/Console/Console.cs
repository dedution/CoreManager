using System;
using System.Collections.Generic;
using System.IO;
using core.modules;
using UnityEngine;

namespace core.console
{
    public interface IConsoleContext
    {
        void Info(string tag, string message);
        void Warn(string tag, string message);
        void Error(string tag, string message);
        void Rainbow(string tag, string message);
        void Clear();
    }

    [Serializable]
    public class ConsoleConfig
    {
        public string version = "0.0.0";
        public string introPath = "";
    }

    public static class Console
    {
        private const string ConfigResourcePath = "console/console_config";
        private static ConsoleConfig Config = new ConsoleConfig();
        private static string ConsoleVersion = "0.0.0";
        private static string ConsoleIntroText = "Welcome to UTerm! {0}";

        public static string GetIntroBanner()
        {
            return ConsoleIntroText;
        }

        public static string GetVersion()
        {
            return ConsoleVersion;
        }


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            LoadConfigData();
            LoadIntroText();
            CommandInternal.Register();
        }

        private static void LoadConfigData()
        {
            TextAsset configAsset = Resources.Load<TextAsset>(ConfigResourcePath);
            if (configAsset == null || string.IsNullOrWhiteSpace(configAsset.text))
                return;

            ConsoleConfig loaded = IO.ParseFromJSON<ConsoleConfig>(configAsset.text);
            if (loaded == null)
                return;

            Config = loaded;

            if (!string.IsNullOrWhiteSpace(Config.version))
                ConsoleVersion = Config.version;

            if (!string.IsNullOrWhiteSpace(Config.introPath))
                ConsoleIntroText = Config.introPath;
        }

        private static void LoadIntroText()
        {
            string text = LoadResourceText(Config.introPath);
            if (string.IsNullOrEmpty(text))
                return;

            ConsoleIntroText = string.Format(text, ConsoleVersion);
        }

        // Only support resource loading for now.
        private static string LoadResourceText(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            string resourcePath = Path.ChangeExtension(path, null);

            TextAsset asset = Resources.Load<TextAsset>(resourcePath);
            if (asset != null)
                return asset.text;

            return "";
        }
    }
}
