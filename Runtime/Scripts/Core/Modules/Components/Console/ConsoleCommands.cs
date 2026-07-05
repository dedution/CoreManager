using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace core.console
{
    // Argument definition
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

    // Command definition
    public struct Command
    {
        public string name;
        public Argument[] arguments;
        public Action<Dictionary<string, object>, IConsoleContext> action;
        public string description;
        public bool hidden;

        public Command(string name, Argument[] arguments, Action<Dictionary<string, object>, IConsoleContext> action, string description = "", bool hidden = false)
        {
            this.name = name;
            this.arguments = arguments;
            this.action = action;
            this.description = description;
            this.hidden = hidden;
        }
    }

    public static class ConsoleCommands
    {
        private static readonly Argument[] EmptyArguments = new Argument[] { };
        private static readonly Dictionary<string, Command> console_commands = new Dictionary<string, Command>(StringComparer.Ordinal);
        private static readonly Regex TokenRegex = new Regex("\"([^\"]+)\"|\\S+", RegexOptions.Compiled);

        public static void RegisterCommand(string command, Argument[] arguments, Action<Dictionary<string, object>, IConsoleContext> action, string description = "", bool hidden = false)
        {
            RegisterCommand(new Command(command, arguments, action, description, hidden));
        }

        public static void RegisterCommand(Command new_command)
        {
            if (string.IsNullOrWhiteSpace(new_command.name))
                return;

            string name = NormalizeCommandName(new_command.name);
            console_commands[name] = new Command(
                name,
                new_command.arguments ?? EmptyArguments,
                new_command.action,
                new_command.description,
                new_command.hidden);
        }

        public static List<string> GetAllCommands()
        {
            List<string> commands = new List<string>(console_commands.Count);
            foreach (KeyValuePair<string, Command> pair in console_commands)
            {
                if (!pair.Value.hidden)
                    commands.Add(pair.Key);
            }

            return commands;
        }

        public static IReadOnlyDictionary<string, Command> GetCommands()
        {
            return console_commands;
        }

        public static void ProcessCommand(string commandFull, IConsoleContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            foreach (string command in SplitCommands(commandFull))
            {
                ProcessSingleCommand(command, context);
            }
        }

        private static void ProcessSingleCommand(string commandFull, IConsoleContext context)
        {
            if (string.IsNullOrWhiteSpace(commandFull))
                return;

            List<string> tokens = Tokenize(commandFull);
            if (tokens.Count == 0 || !console_commands.TryGetValue(tokens[0], out Command cmd))
            {
                context.Error("console", $"Failed to execute command <i>{commandFull}</i>");
                return;
            }

            Argument[] argDefs = cmd.arguments ?? EmptyArguments;
            Dictionary<string, object> parsedArgs = new Dictionary<string, object>();

            if (argDefs.Length != tokens.Count - 1)
            {
                context.Error("console", $"Arguments for command <i>{tokens[0]}</i> don't match");
                context.Info("console", "Expected:");
                foreach (Argument arg in argDefs)
                {
                    context.Info("console", $"  <i>{arg.name}</i> ({TypeToString(arg.type)})");
                }
                return;
            }

            for (int i = 0; i < argDefs.Length; i++)
            {
                Argument argDef = argDefs[i];
                if (!TryParse(tokens[i + 1], argDef.type, out object value))
                {
                    context.Error("console", $"Argument '{argDef.name}' has invalid type. Expected {TypeToString(argDef.type)}");
                    return;
                }

                parsedArgs[argDef.name] = value;
            }

            try
            {
                cmd.action?.Invoke(parsedArgs, context);
            }
            catch (Exception e)
            {
                context.Error("console", $"Error executing command {tokens[0]}: {e.Message}");
            }
        }

        private static List<string> SplitCommands(string commandFull)
        {
            List<string> result = new List<string>();
            if (string.IsNullOrWhiteSpace(commandFull))
                return result;

            StringBuilder current = new StringBuilder(commandFull.Length);
            bool inQuotes = false;
            foreach (char c in commandFull)
            {
                if (c == '"')
                    inQuotes = !inQuotes;
                else if (c == ';' && !inQuotes)
                {
                    AddCommand(result, current.ToString());
                    current.Length = 0;
                    continue;
                }

                current.Append(c);
            }

            AddCommand(result, current.ToString());
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
    }
}
