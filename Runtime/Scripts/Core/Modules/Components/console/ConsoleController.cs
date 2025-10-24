using System.Collections;
using System.Collections.Generic;
using core.console;
using core.gameplay;
using core.modules;
using UnityEngine;
using static core.GameManager;

namespace core.debug
{
    public class ConsoleController : baseGameActor
    {
        public ConsoleInput consoleInput = null;
        public ConsoleLogger consoleLogger = null;
        public RectTransform consoleCanvas;
        public RectTransform consoleMask;
        public float animationDuration = 0.3f;
        private Coroutine currentAnim;

        protected override void onStart()
        {
            ActOnModule((EventManager _ref) =>
            {
                _ref.StartListening("Console", OnConsoleUpdate);
            });

            UpdateMasksVisibility(false, true);

            consoleInput.OnTextSubmitted.AddListener(OnProcessInput);
        }

        private void OnProcessInput(string input)
        {
            // Print command
            Logger.Info("system", input, true);

            // Process and execute it
            Console.ProcessCommand(input);
        }

        private void OnConsoleUpdate(Dictionary<string, object> param)
        {
            if (param.ContainsKey("isMenuOpen"))
            {
                bool _isMenuOpen = (bool)param["isMenuOpen"];
                consoleInput.onToggleMenu(_isMenuOpen);
                consoleLogger.onToggleMenu(_isMenuOpen);
                UpdateMasksVisibility(_isMenuOpen);
            }
        }

        private void UpdateMasksVisibility(bool state, bool instant = false)
        {
            if (consoleCanvas == null || consoleMask == null)
                return;

            if (instant)
            {
                Vector2 offsetMin = consoleMask.offsetMin;
                offsetMin.y = state ? 0 : consoleCanvas.sizeDelta.y;
                consoleMask.offsetMin = offsetMin;
                return;
            }

            if (currentAnim != null)
                StopCoroutine(currentAnim);

            currentAnim = StartCoroutine(AnimateMask(state));
        }

        private IEnumerator AnimateMask(bool visible)
        {
            float elapsed = 0f;
            float duration = animationDuration;

            Vector2 startOffset = consoleMask.offsetMin;
            Vector2 endOffset = startOffset;
            endOffset.y = visible ? 0f : consoleCanvas.sizeDelta.y;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

                Vector2 newOffset = Vector2.Lerp(startOffset, endOffset, t);
                consoleMask.offsetMin = newOffset;

                yield return null;
            }

            // Snap to final position (avoid precision issues)
            consoleMask.offsetMin = endOffset;
        }
    }
}