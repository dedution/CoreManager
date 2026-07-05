using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using UnityEngine;

namespace core.console
{
    public struct Argument
    {
        public string name;
        public Type type;

        public Argument(string name, Type type)
        {
            this.name = name;
            this.type = type;
        }
    }
    
    public struct Command
    {
        public string name;
        public Argument[] arguments;
        public Action<Dictionary<string, object>> action;
        public Func<Dictionary<string, object>, IConsoleOutput, IEnumerator> routine;
        public string description;
        public bool hidden;

        public Command(string name, Argument[] arguments, Action<Dictionary<string, object>> action, string description = "", bool hidden = false)
        {
            this.name = name;
            this.arguments = arguments;
            this.action = action;
            routine = null;
            this.description = description;
            this.hidden = hidden;
        }

        public Command(string name, Argument[] arguments, Func<Dictionary<string, object>, IConsoleOutput, IEnumerator> routine, string description = "", bool hidden = false)
        {
            this.name = name;
            this.arguments = arguments;
            action = null;
            this.routine = routine;
            this.description = description;
            this.hidden = hidden;
        }
    }

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

    public static class Console
    {
        private const string CoreManagerVersion = "0.6.0";
        private static readonly Dictionary<string, Command> console_commands = new Dictionary<string, Command>();
        private static readonly Regex TokenRegex = new Regex("\"([^\"]+)\"|\\S+", RegexOptions.Compiled);

        public static void RegisterCommand(string command, Argument[] arguments, Action<Dictionary<string, object>> action, string description = "", bool hidden = false)
        {
            RegisterCommand(new Command(command, arguments, action, description, hidden));
        }

        public static void RegisterCommand(string command, Argument[] arguments, Func<Dictionary<string, object>, IConsoleOutput, IEnumerator> routine, string description = "", bool hidden = false)
        {
            RegisterCommand(new Command(command, arguments, routine, description, hidden));
        }

        public static void RegisterCommand(Command new_command)
        {
            if (string.IsNullOrWhiteSpace(new_command.name))
                return;

            string name = NormalizeCommandName(new_command.name);
            console_commands[name] = new Command(
                name,
                new_command.arguments ?? new Argument[] { },
                new_command.routine,
                new_command.description,
                new_command.hidden)
            {
                action = new_command.action
            };
        }

        public static List<string> GetAllCommands()
        {
            return console_commands
                .Where(pair => !pair.Value.hidden)
                .Select(pair => pair.Key)
                .ToList();
        }

        public static IReadOnlyDictionary<string, Command> GetCommands()
        {
            return console_commands;
        }

        public static void ProcessCommand(string commandFull)
        {
            ConsoleRuntime.Run(ProcessCommandRoutine(commandFull, LoggerConsoleOutput.Instance));
        }

        public static IEnumerator ProcessCommandRoutine(string commandFull, IConsoleOutput output = null)
        {
            output = output ?? LoggerConsoleOutput.Instance;

            foreach (string command in SplitCommands(commandFull))
            {
                yield return ProcessSingleCommand(command, output);
            }
        }

        private static IEnumerator ProcessSingleCommand(string commandFull, IConsoleOutput output)
        {
            if (string.IsNullOrWhiteSpace(commandFull))
                yield break;

            List<string> tokens = Tokenize(commandFull);
            if (tokens.Count == 0 || !console_commands.ContainsKey(tokens[0]))
            {
                output.Error("console", $"Failed to execute command <i>{commandFull}</i>");
                yield break;
            }

            Command cmd = console_commands[tokens[0]];
            Argument[] argDefs = cmd.arguments ?? new Argument[] { };
            Dictionary<string, object> parsedArgs = new Dictionary<string, object>();

            if (argDefs.Length != tokens.Count - 1)
            {
                output.Error("console", $"Arguments for command <i>{tokens[0]}</i> don't match");
                output.Info("console", "Expected:");
                foreach (Argument arg in argDefs)
                {
                    output.Info("console", $"  <i>{arg.name}</i> ({TypeToString(arg.type)})");
                }
                yield break;
            }

            for (int i = 0; i < argDefs.Length; i++)
            {
                Argument argDef = argDefs[i];
                if (!TryParse(tokens[i + 1], argDef.type, out object value))
                {
                    output.Error("console", $"Argument '{argDef.name}' has invalid type. Expected {TypeToString(argDef.type)}");
                    yield break;
                }

                parsedArgs[argDef.name] = value;
            }

            IEnumerator routine = null;
            try
            {
                if (cmd.routine != null)
                    routine = cmd.routine.Invoke(parsedArgs, output);
                else
                    cmd.action?.Invoke(parsedArgs);
            }
            catch (Exception e)
            {
                output.Error("console", $"Error executing command {tokens[0]}: {e.Message}");
                yield break;
            }

            if (routine != null)
                yield return routine;
        }

        private static List<string> SplitCommands(string commandFull)
        {
            List<string> result = new List<string>();
            if (string.IsNullOrWhiteSpace(commandFull))
                return result;

            string current = string.Empty;
            bool inQuotes = false;
            foreach (char c in commandFull)
            {
                if (c == '"')
                    inQuotes = !inQuotes;
                else if (c == ';' && !inQuotes)
                {
                    AddCommand(result, current);
                    current = string.Empty;
                    continue;
                }

                current += c;
            }

            AddCommand(result, current);
            return result;
        }

        private static void AddCommand(List<string> result, string command)
        {
            command = command.Trim();
            if (!string.IsNullOrEmpty(command))
                result.Add(command);
        }

        private static List<string> Tokenize(string commandFull)
        {
            List<string> tokens = new List<string>();
            foreach (Match match in TokenRegex.Matches(commandFull))
            {
                string token = match.Value;
                if (token.StartsWith("\"", StringComparison.Ordinal) && token.EndsWith("\"", StringComparison.Ordinal))
                    token = token.Substring(1, token.Length - 2);

                tokens.Add(token);
            }

            return tokens;
        }

        private static bool TryParse(string rawValue, Type type, out object value)
        {
            if (type == typeof(string))
            {
                value = rawValue;
                return true;
            }

            if (type == typeof(int) && int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
            {
                value = intValue;
                return true;
            }

            if (type == typeof(float) && float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue))
            {
                value = floatValue;
                return true;
            }

            if (type == typeof(bool))
            {
                string lower = rawValue.ToLowerInvariant();
                if (lower == "true" || lower == "1" || lower == "yes")
                {
                    value = true;
                    return true;
                }

                if (lower == "false" || lower == "0" || lower == "no")
                {
                    value = false;
                    return true;
                }
            }

            value = null;
            return false;
        }

        private static string TypeToString(Type type)
        {
            if (type == typeof(int))
                return "int";
            if (type == typeof(float))
                return "float";
            if (type == typeof(bool))
                return "bool";

            return "string";
        }

        private static string NormalizeCommandName(string command)
        {
            return command.StartsWith("/", StringComparison.Ordinal) ? command : "/" + command;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register_Default_Commands()
        {
            RegisterCommand("/version", new Argument[] { }, (param, output) =>
            {
                output.Info("console", $"Console version: {CoreManagerVersion}");
                return null;
            }, "Prints the console version");

            RegisterCommand("/sleep", Args(("time", typeof(float))), (param, output) => Sleep((float)param["time"]), "Sleeps for a given time");

            RegisterCommand("/exec", Args(("file_name", typeof(string))), (param, output) =>
            {
                string content = ReadText((string)param["file_name"]);
                if (string.IsNullOrEmpty(content))
                {
                    output.Error("console", "File not found or invalid!");
                    return null;
                }

                return ProcessCommandRoutine(content, output);
            }, "Executes a file containing commands");

            RegisterCommand("/load-mod", Args(("file_name", typeof(string))), (param, output) =>
            {
                string path = ResolvePath((string)param["file_name"]);
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    output.Warn("AssetBundle", $"Bundle file not found: {path}");
                    return null;
                }

                AssetBundle bundle = AssetBundle.LoadFromFile(path);
                if (bundle != null)
                    output.Info("AssetBundle", $"Successfully loaded: {path}");
                else
                    output.Error("AssetBundle", $"Failed to load bundle: {path}");

                return null;
            }, "Loads an AssetBundle file");

            RegisterCommand("/load-script", Args(("file_name", typeof(string))), (param, output) =>
            {
                output.Warn("console", "/load-script is not available in the Unity port. Register C# commands instead.");
                return null;
            }, "Reports Unity scripting limitation");

            RegisterCommand("/clear", new Argument[] { }, (param, output) =>
            {
                output.Clear();
                return null;
            }, "Clears console logs");

            RegisterCommand("/pause", Args(("pause", typeof(bool))), (param, output) =>
            {
                bool pause = (bool)param["pause"];
                Time.timeScale = pause ? 0f : 1f;
                Time.fixedDeltaTime = 0.02f * Mathf.Max(Time.timeScale, 0.0001f);
                output.Info("console", $"Game paused: {pause}");
                return null;
            }, "Pauses and unpauses the game");

            RegisterCommand("/game-speed", Args(("time", typeof(float))), (param, output) =>
            {
                Time.timeScale = (float)param["time"];
                Time.fixedDeltaTime = 0.02f * Mathf.Max(Time.timeScale, 0.0001f);
                output.Info("console", $"Game speed set to: {Time.timeScale.ToString(CultureInfo.InvariantCulture)}");
                return null;
            }, "Sets the current game speed");

            RegisterCommand("/fps-cap", Args(("cap", typeof(int))), (param, output) =>
            {
                Application.targetFrameRate = (int)param["cap"];
                output.Info("console", $"Game max fps cap set to {Application.targetFrameRate} fps");
                return null;
            }, "Limits the max game framerate");

            RegisterCommand("/vsync", Args(("state", typeof(bool))), (param, output) =>
            {
                QualitySettings.vSyncCount = (bool)param["state"] ? 1 : 0;
                output.Info("console", $"Game vsync: {QualitySettings.vSyncCount > 0}");
                return null;
            }, "Turns vsync on and off");

            RegisterCommand("/monitor-info", new Argument[] { }, (param, output) =>
            {
                for (int i = 0; i < Display.displays.Length; i++)
                {
                    Display display = Display.displays[i];
                    output.Info("console", $"Monitor {i} info:");
                    output.Info("console", $"  Resolution: {display.systemWidth}x{display.systemHeight}");
                    output.Info("console", "--------------------------------");
                }

                return null;
            }, "Prints machine monitor info");

            RegisterCommand("/stats", new Argument[] { }, (param, output) =>
            {
                int fps = Mathf.RoundToInt(1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f));
                output.Info("console", $"Current FPS: {fps}");
                output.Info("console", $"Current resolution: {Screen.width}x{Screen.height}");
                return null;
            }, "Prints game performance related stats");

            RegisterCommand("/net-stats", new Argument[] { }, (param, output) =>
            {
                output.Info("console", $"Machine network address: {LocalIp()}");
                return null;
            }, "Prints network related stats");

            RegisterCommand("/print", Args(("text", typeof(string))), (param, output) =>
            {
                output.Info("console", (string)param["text"]);
                return null;
            }, "Prints words into the console");

            RegisterCommand("/help", new Argument[] { }, (param, output) =>
            {
                output.Rainbow("console", "Available commands:");
                foreach (KeyValuePair<string, Command> pair in console_commands.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
                {
                    if (pair.Value.hidden)
                        continue;

                    string description = pair.Value.description;
                    output.Info("console", string.IsNullOrEmpty(description) ? $"  <i>{pair.Key}</i>" : $"  <i>{pair.Key}</i> - ({description})");
                }

                return null;
            }, "Lists the available commands");
        }

        private static Argument[] Args(params (string name, Type type)[] args)
        {
            return args.Select(arg => new Argument(arg.name, arg.type)).ToArray();
        }

        private static IEnumerator Sleep(float time)
        {
            yield return new WaitForSecondsRealtime(Mathf.Max(0f, time));
        }

        private static string ReadText(string relativePath)
        {
            string path = ResolvePath(relativePath);
            return !string.IsNullOrEmpty(path) && File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        private static string ResolvePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return string.Empty;

            if (Path.IsPathRooted(relativePath))
                return relativePath;

            string projectPath = Path.Combine(Application.dataPath, "..", relativePath);
            if (File.Exists(projectPath))
                return projectPath;

            return Path.Combine(Application.streamingAssetsPath, relativePath);
        }

        private static string LocalIp()
        {
            try
            {
                foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork && !address.ToString().StartsWith("127.", StringComparison.Ordinal))
                        return address.ToString();
                }
            }
            catch (SocketException)
            {
            }

            return "127.0.0.1";
        }

        private sealed class ConsoleRuntime : MonoBehaviour
        {
            private static ConsoleRuntime instance;

            private static ConsoleRuntime Instance
            {
                get
                {
                    if (instance == null)
                    {
                        GameObject go = new GameObject("Core Console Runtime");
                        UnityEngine.Object.DontDestroyOnLoad(go);
                        instance = go.AddComponent<ConsoleRuntime>();
                    }

                    return instance;
                }
            }

            public static Coroutine Run(IEnumerator routine)
            {
                return routine == null ? null : Instance.StartCoroutine(routine);
            }

            private void OnDestroy()
            {
                if (instance == this)
                    instance = null;
            }
        }
    }
}
