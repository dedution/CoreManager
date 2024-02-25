using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace core.modules
{
    public class AudioManager : BaseModule
    {
        // TODO:
        // Loading audio for UI
        // Pool-based play audio at 3D point (that doesnt bug out with timescale at 0)
        // Controller audio support
        // 

        private static AudioManager _instance = null;
        private List<AudioSource> AudioSourcePool = new List<AudioSource>();
        private int AudioSourcePoolID = 0;
        private GameObject PoolRoot;
        private const int POOLSIZE = 30;

        public AudioManager()
        {
            if (_instance == null)
                _instance = this;
        }

        public override void onInitialize()
        {
            // Populate pool of audio sources
            PopulatePool();
        }

        private void PopulatePool()
        {
            PoolRoot = new GameObject("Audio Source Pool");
            UnityEngine.Object.DontDestroyOnLoad(PoolRoot);

            for(int i = 0; i < POOLSIZE; i++)
            {
                GameObject newASource_obj = new GameObject("ASource_" + i);
                newASource_obj.transform.SetParent(PoolRoot.transform);
                AudioSource newASource = newASource_obj.AddComponent<AudioSource>();
                ConfigNewASource(newASource);
                AudioSourcePool.Add(newASource);
            }
        }

        private void ConfigNewASource(AudioSource _source)
        {
            
        }

        private void MoveNextPool()
        {
            if(AudioSourcePoolID < POOLSIZE)
                AudioSourcePoolID++;
            else
                AudioSourcePoolID = 0;
        }

        public static void ResetAll()
        {
            if(_instance == null)
            {
                Debug.LogError("Audio Manager Module wasn't initialized.");
                return;
            }

            for(int i = 0; i < POOLSIZE; i++)
            {
                ResetAudio(i);
            }
        }

        public static void ResetAudio(int IDx)
        {
            if(_instance == null)
            {
                Debug.LogError("Audio Manager Module wasn't initialized.");
                return;
            }

            AudioSource _s = _instance.AudioSourcePool.ElementAt(IDx);
            _s.Stop();
            _instance.ConfigNewASource(_s);
        }

        public static AudioSource PlayAudio(AudioClip _clip, bool _isLoop = false)
        {
            if(_instance == null)
            {
                Debug.LogError("Audio Manager Module wasn't initialized.");
                return null;
            }

            AudioSource _s = _instance.AudioSourcePool.ElementAt(_instance.AudioSourcePoolID);
            _s.clip = _clip;
            _s.loop = _isLoop;
            _instance.MoveNextPool();
            return _s;
        }
    }
}