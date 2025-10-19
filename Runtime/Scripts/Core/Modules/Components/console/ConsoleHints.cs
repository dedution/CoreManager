using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public class ConsoleHints : baseGameActor
    {
        public RectTransform hintsMask;
        public List<Button> hintButtons;      // Buttons to show suggestions
        public float buttonHeight = 30f;      // Height of one button
        public ConsoleInput consoleInput = null;

        private bool isVisible = false;
        private float animationDuration = 0.3f;
        private Coroutine currentAnim;

        protected override void onStart()
        {
            if (consoleInput != null)
            {
                consoleInput.OnTextChanged.AddListener(ProcessSuggestions);
            }

            // Add click callbacks to buttons
            foreach (var btn in hintButtons)
            {
                btn.onClick.RemoveAllListeners(); // clear previous listeners
                btn.onClick.AddListener(() => OnHintButtonClicked(btn));
            }

            UpdateMasksVisibility(false, true);
        }

        private void ProcessSuggestions(string input)
        {
            if (hintButtons == null || hintButtons.Count == 0)
                return;

            List<string> allCommands = Console.GetAllCommands();
            List<string> matches = new List<string>();

            if (string.IsNullOrEmpty(input))
            {
                HideAllButtons();
                UpdateMasksVisibility(false);
                return;
            }

            if (input.StartsWith("/"))
            {
                string userPrefix = input.Substring(1).ToLower();

                foreach (string cmd in allCommands)
                {
                    string cmdName = cmd.StartsWith("/") ? cmd.Substring(1) : cmd;
                    cmdName = cmdName.ToLower();

                    if (cmdName.StartsWith(userPrefix))
                        matches.Add(cmd);
                }
            }
            else
            {
                matches.AddRange(allCommands);
            }

            // If input fully matches a command, hide hints
            if (allCommands.Contains(input))
            {
                HideAllButtons();
                UpdateMasksVisibility(false);
                return;
            }

            if (matches.Count > 0)
            {
                matches.Sort((a, b) =>
                {
                    if (a.Length != b.Length)
                        return a.Length.CompareTo(b.Length);
                    return string.Compare(a, b, true);
                });

                SetHints(matches);
            }
            else
            {
                HideAllButtons();
                UpdateMasksVisibility(false);
            }
        }

        private void SetHints(List<string> hintsData)
        {
            int visibleCount = Mathf.Min(hintsData.Count, hintButtons.Count);

            // Populate visible buttons
            for (int i = 0; i < hintButtons.Count; i++)
            {
                Button btn = hintButtons[i];
                TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();

                if (i < visibleCount)
                {
                    btn.gameObject.SetActive(true);
                    if (txt != null)
                        txt.text = hintsData[i];
                }
                else
                {
                    btn.gameObject.SetActive(false);
                }
            }

            // Animate mask height based on visible buttons
            UpdateMasksVisibility(visibleCount > 0, true);
        }

        private void OnHintButtonClicked(Button btn)
        {
            if (consoleInput == null) return;

            TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                consoleInput.ForceText(txt.text);
            }

            HideAllButtons();
            UpdateMasksVisibility(false);
        }

        private void HideAllButtons()
        {
            foreach (var btn in hintButtons)
                btn.gameObject.SetActive(false);
        }

        #region Mask Animation
        private void UpdateMasksVisibility(bool state, bool instant = false)
        {
            if (hintsMask == null || hintButtons == null)
                return;

            float targetHeight = 0f;
            if (state)
            {
                int visibleCount = hintButtons.Count(b => b.gameObject.activeSelf);
                targetHeight = visibleCount * buttonHeight;
            }

            if (instant)
            {
                Vector2 size = hintsMask.sizeDelta;
                size.y = targetHeight;
                hintsMask.sizeDelta = size;
                return;
            }

            if (currentAnim != null)
                StopCoroutine(currentAnim);

            currentAnim = StartCoroutine(AnimateMask(targetHeight));
        }

        private IEnumerator AnimateMask(float targetHeight)
        {
            float elapsed = 0f;
            float duration = animationDuration;

            float startHeight = hintsMask.sizeDelta.y;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

                Vector2 size = hintsMask.sizeDelta;
                size.y = Mathf.Lerp(startHeight, targetHeight, t);
                hintsMask.sizeDelta = size;

                yield return null;
            }

            Vector2 finalSize = hintsMask.sizeDelta;
            finalSize.y = targetHeight;
            hintsMask.sizeDelta = finalSize;
        }
        #endregion
    }
}