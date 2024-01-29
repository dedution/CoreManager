using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace core.modules
{
    public class AudioManager : BaseModule
    {
        private static AudioManager _instance = null;

        protected override void onInitialize()
        {
            if(_instance == null)
                _instance = this;
        }

        // TODO:
        // Loading audio for UI
        // Pool-based play audio at 3D point (that doesnt bug out with timescale at 0)
        // 
    }
}