using System.Collections;
using System.Collections.Generic;
using core.modules;
using UnityEngine;
using static core.GameManager;

public static class Logger
{
    public static void Info(string logTag, string output)
    {
        Debug.Log(output);

        ActOnModule((EventManager _ref) =>
            {
                _ref.TriggerEvent("log", new Dictionary<string, object> { { "log_tag", logTag }, { "log_output", output }, { "log_type", "info" } });
            });
    }

    public static void Warn(string logTag, string output)
    {
        Debug.LogWarning(output);

        ActOnModule((EventManager _ref) =>
            {
                _ref.TriggerEvent("log", new Dictionary<string, object> { { "log_tag", logTag }, { "log_output", output }, { "log_type", "warning" } });
            });
    }

    public static void Error(string logTag, string output)
    {
        Debug.LogError(output);

        ActOnModule((EventManager _ref) =>
            {
                _ref.TriggerEvent("log", new Dictionary<string, object> { { "log_tag", logTag }, { "log_output", output }, { "log_type", "error" } });
            });
    }

    public static void Clear()
    {
        ActOnModule((EventManager _ref) =>
            {
                _ref.TriggerEvent("log_clear", new Dictionary<string, object> {});
            });
    }
}