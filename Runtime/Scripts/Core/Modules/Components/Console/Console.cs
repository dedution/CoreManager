using System;
using System.Collections.Generic;
using System.IO;
using core.modules;
using UnityEngine;

namespace core.console
{
    public interface IConsoleOutput
    {
        void Info(string tag, string output);
        void Warn(string tag, string output);
        void Error(string tag, string output);
        void Rainbow(string tag, string output);
        void Clear();
    }

    public sealed class LoggerConsoleOutput : IConsoleOutput
    {
        public static readonly LoggerConsoleOutput Instance = new LoggerConsoleOutput();

        private LoggerConsoleOutput()
        {
        }

        public void Info(string tag, string output)
        {
            Logger.Info(tag, output, true);
        }

        public void Warn(string tag, string output)
        {
            Logger.Warn(tag, output, true);
        }

        public void Error(string tag, string output)
        {
            Logger.Error(tag, output, true);
        }

        public void Rainbow(string tag, string output)
        {
            Logger.Rainbow(tag, output, true);
        }

        public void Clear()
        {
            Logger.Clear();
        }
    }

    [Serializable]
    public class ConsoleConfig
    {
        public string version = "0.0.0";
        public string introPath = "console/intro";
    }

    public static class Console
    {
        private const string ConfigResourcePath = "console/console_config";
        private static ConsoleConfig config = new ConsoleConfig();
        private static bool initialized;

        public static ConsoleConfig Config => config;
        public static string Version => config.version;
        public static string IntroText => LoadIntroText();

        public static void RegisterCommand(string command, Argument[] arguments, Action<Dictionary<string, object>> action, string description = "", bool hidden = false)
        {
            ConsoleCommands.RegisterCommand(command, arguments, action, description, hidden);
        }

        public static void RegisterCommand(Command new_command)
        {
            ConsoleCommands.RegisterCommand(new_command);
        }

        public static List<string> GetAllCommands()
        {
            return ConsoleCommands.GetAllCommands();
        }

        public static IReadOnlyDictionary<string, Command> GetCommands()
        {
            return ConsoleCommands.GetCommands();
        }

        public static void ProcessCommand(string commandFull)
        {
            ConsoleCommands.ProcessCommand(commandFull);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (initialized)
                return;

            initialized = true;
            LoadConfig();
            RegisterSignals();
            CommandInternal.Register();
        }

        private static void LoadConfig()
        {
            TextAsset configAsset = Resources.Load<TextAsset>(ConfigResourcePath);
            if (configAsset == null || string.IsNullOrWhiteSpace(configAsset.text))
                return;

            ConsoleConfig loaded = JsonUtility.FromJson<ConsoleConfig>(configAsset.text);
            if (loaded == null)
                return;

            if (!string.IsNullOrWhiteSpace(loaded.version))
                config.version = loaded.version;

            if (!string.IsNullOrWhiteSpace(loaded.introPath))
                config.introPath = loaded.introPath;
        }

        private static void RegisterSignals()
        {
            EventManager.Connect("console_command", OnConsoleCommand);
        }

        private static void OnConsoleCommand(Dictionary<string, object> param)
        {
            if (param == null)
                return;

            if (param.TryGetValue("command", out object command) || param.TryGetValue("input", out command))
                ProcessCommand(command as string);
        }

        private static string LoadIntroText()
        {
            string text = LoadText(config.introPath);
            if (string.IsNullOrEmpty(text))
                text = "Welcome to UTerm! {0}";

            return string.Format(text, Version);
        }

        private static string LoadText(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            string resourcePath = Path.ChangeExtension(path, null);
            TextAsset asset = Resources.Load<TextAsset>(resourcePath);
            if (asset != null)
                return asset.text;

            if (File.Exists(path))
                return File.ReadAllText(path);

            string projectPath = Path.Combine(Application.dataPath, "..", path);
            return File.Exists(projectPath) ? File.ReadAllText(projectPath) : string.Empty;
        }
    }
}
