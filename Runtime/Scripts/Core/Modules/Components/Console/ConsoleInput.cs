using System.Collections.Generic;
using core;
using core.console;
using core.gameplay;
using core.modules;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static core.GameManager;

namespace core.debug
{
    public class ConsoleInput : GameActor
    {
        [Header("UI Reference")]
        public TMP_InputField inputField;

        [Header("Autocomplete")]
        public int historyLimit = 64;

        [Header("Callbacks")]
        public UnityEvent<string> OnTextSubmitted;
        public UnityEvent<string> OnTextChanged;

        private readonly List<string> commandHistory = new List<string>();
        private int historyIndex = -1;
        private int completionIndex = -1;
        private string completionSegment = "";
        private List<string> completionMatches = new List<string>();
        private bool suppressCompletionReset = false;

        public void onToggleMenu(bool state)
        {
            if (state)
            {
                enable();
                FocusInput();
            }
            else
                disable();
        }

        private void enable()
        {
            inputField.onValueChanged.AddListener(HandleTextChanged);
            inputField.onSubmit.AddListener(HandleSubmit);
        }

        private void disable()
        {
            inputField.onValueChanged.RemoveListener(HandleTextChanged);
            inputField.onSubmit.RemoveListener(HandleSubmit);
            inputField.DeactivateInputField();
            EventSystem.current.SetSelectedGameObject(null);
        }

        public override void onUpdate()
        {
            if (!inputField.isFocused)
                return;

            if (InputManager.IsActionPressedThisFrame("ConsoleAutoComplete", "Debug"))
            {
                CompleteInput();
                return;
            }

            if (InputManager.IsActionPressedThisFrame("ConsoleHistoryPrevious", "Debug"))
            {
                MoveHistory(-1);
                return;
            }

            if (InputManager.IsActionPressedThisFrame("ConsoleHistoryNext", "Debug"))
            {
                MoveHistory(1);
                return;
            }
        }

        private void HandleTextChanged(string text)
        {
            if (!suppressCompletionReset)
            {
                completionIndex = -1;
                completionSegment = "";
                completionMatches.Clear();
            }

            OnTextChanged?.Invoke(text);
        }

        private void HandleSubmit(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            AddHistory(text);
            OnTextSubmitted?.Invoke(text);

            // Clear input and refocus
            inputField.text = "";
            historyIndex = -1;
            FocusInput();
        }

        public void ForceText(string text)
        {
            inputField.text = text;
            completionIndex = -1;
            completionSegment = "";
            completionMatches.Clear();
            FocusInput();
        }

        public void ForceActiveSegment(string text)
        {
            ReplaceActiveSegment(text);
        }

        public void FocusInput()
        {
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
            inputField.ActivateInputField();
        }

        private void CompleteInput()
        {
            string text = inputField.text;
            string segment = GetActiveSegment(text);
            if (string.IsNullOrWhiteSpace(segment))
                return;

            if (completionIndex < 0 || completionSegment != segment)
            {
                completionSegment = segment;
                completionMatches = GetCompletionMatches(segment);
                completionIndex = 0;
            }
            else if (completionMatches.Count > 0)
            {
                completionIndex = (completionIndex + 1) % completionMatches.Count;
            }

            if (completionMatches.Count == 0)
                return;

            ReplaceActiveSegment(completionMatches[completionIndex]);
        }

        private List<string> GetCompletionMatches(string segment)
        {
            string trimmed = segment.TrimStart();
            string lower = trimmed.ToLowerInvariant();
            List<string> matches = new List<string>();

            foreach (string command in Console.GetAllCommands())
            {
                string commandLower = command.ToLowerInvariant();
                string commandNameLower = commandLower.StartsWith("/") ? commandLower.Substring(1) : commandLower;
                if (commandLower.StartsWith(lower) || commandNameLower.StartsWith(lower))
                    matches.Add(command);
            }

            foreach (string entry in commandHistory)
            {
                string historySegment = GetActiveSegment(entry).TrimStart();
                string historyLower = historySegment.ToLowerInvariant();
                string historyNameLower = historyLower.StartsWith("/") ? historyLower.Substring(1) : historyLower;
                if (!string.IsNullOrEmpty(historySegment) &&
                    (historyLower.StartsWith(lower) || historyNameLower.StartsWith(lower)) &&
                    !matches.Contains(historySegment))
                {
                    matches.Add(historySegment);
                }
            }

            matches.Sort((a, b) =>
            {
                if (a.Length != b.Length)
                    return a.Length.CompareTo(b.Length);
                return string.Compare(a, b, true);
            });
            return matches;
        }

        private void MoveHistory(int direction)
        {
            if (commandHistory.Count == 0)
                return;

            if (historyIndex < 0)
                historyIndex = direction < 0 ? commandHistory.Count - 1 : 0;
            else
                historyIndex = Mathf.Clamp(historyIndex + direction, 0, commandHistory.Count - 1);

            ForceText(commandHistory[historyIndex]);
        }

        private void AddHistory(string text)
        {
            text = text.Trim();
            if (commandHistory.Count > 0 && commandHistory[commandHistory.Count - 1] == text)
                return;

            commandHistory.Add(text);
            if (commandHistory.Count > historyLimit)
                commandHistory.RemoveAt(0);
        }

        private string GetActiveSegment(string text)
        {
            int start = text.LastIndexOf(';');
            return start < 0 ? text : text.Substring(start + 1);
        }

        private void ReplaceActiveSegment(string replacement)
        {
            string text = inputField.text;
            int start = text.LastIndexOf(';');
            suppressCompletionReset = true;
            inputField.text = start < 0 ? replacement : text.Substring(0, start + 1) + " " + replacement;
            suppressCompletionReset = false;
            inputField.caretPosition = inputField.text.Length;
            FocusInput();
        }
    }
}
