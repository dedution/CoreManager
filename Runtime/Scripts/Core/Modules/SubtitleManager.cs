using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using static core.GameManager;

namespace core.modules
{
    public class SubtitleManager : BaseModule
    {
        private delegate void SubtitleInformer(string sub, float timer);
        private SubtitleInformer InformSubtitle;
        private float currentSubtitleTimer = 0f;
        private Dictionary<string, float> SubtitleQueue = new Dictionary<string, float>();
        private bool Subtitle_Enabled = false;

        public override void onInitialize()
        {
            // Update SETTINGS_ALLOWSUBTITLES through an event with the correct state
            //Subtitle_Enabled = GetLoadedModule<SaveSystemManager>().SaveSystem_Config_Get("SETTINGS_ALLOWSUBTITLES", false);

            ActOnModule((EventManager _ref) => { _ref.StartListening("SubtitleManager", OnSaveSystem); }, true);
        }

        void OnSaveSystem(Dictionary<string, object> param)
        {
            Subtitle_Enabled = (bool)param["SETTINGS_ALLOWSUBTITLES"];
        }

        public override void UpdateModule(float deltaTime, float unscaledDeltaTime)
        {
            currentSubtitleTimer = currentSubtitleTimer < 0 ? 0 : currentSubtitleTimer - Time.deltaTime;

            if (currentSubtitleTimer > 0 || SubtitleQueue.Count == 0 || InformSubtitle == null)
                return;

            string _subtitle = SubtitleQueue.Keys.ElementAt(0);
            InformSubtitle(_subtitle, SubtitleQueue[_subtitle]);
            currentSubtitleTimer = SubtitleQueue[_subtitle];

            SubtitleQueue.Remove(_subtitle);
        }

        public void Subtitles_Say_Immediate(string subtitle, float time)
        {
            if (!Subtitle_Enabled || InformSubtitle == null) return;

            InformSubtitle(subtitle, time);
            currentSubtitleTimer = time;
        }

        public void Subtitles_Say_ToQueue(string subtitle, float time)
        {
            SubtitleQueue.Add(subtitle, time);
        }

        public void Subtitles_ClearDisplayers()
        {
            if (!Subtitle_Enabled || InformSubtitle == null) return;

            // Inform a cleanup of current subtitle
            InformSubtitle("", 0.1f);
        }
    }
}