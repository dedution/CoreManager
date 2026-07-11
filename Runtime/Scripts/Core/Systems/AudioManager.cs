using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace core.modules
{
    [System.Serializable]
    public class AudioData
    {
        public AudioData()
        {

        }

        public string[] audioclips;
    }

    public static class AudioManager
    {
        // TODO:
        // Loading audio for UI
        // Pool-based play audio at 3D point (that doesnt bug out with timescale at 0)
        // Controller audio support
        // Implement support for audio occlusion

        private static List<AudioSource> AudioSourcePool = new List<AudioSource>();
        private static Dictionary<string, AudioClip> LoadedAudioData = new Dictionary<string, AudioClip>();
        private static int AudioSourcePoolID = 0;
        private static GameObject PoolRoot;
        private static AudioManagerCoroutineRunner CoroutineRunner;
        private const int POOLSIZE = 30;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void Init()
        {
            if (PoolRoot != null)
                UnityEngine.Object.Destroy(PoolRoot);

            AudioSourcePool.Clear();
            LoadedAudioData.Clear();
            AudioSourcePoolID = 0;

            // Populate pool of audio sources
            PopulatePool();

            // Load audio from streaming assets. This data is persistent, load with caution.
            LoadAudioFromAssets();
        }

        private static void LoadAudioFromAssets()
        {
            // StreamingAssets/Data/Audio.asset -- AssetBundle containing this persistent data
            var audioPackPath = Application.streamingAssetsPath + "/data/audio";
            CoroutineRunner.StartCoroutine(AsyncLoadAudioPack(audioPackPath));
        }

        private static IEnumerator AsyncLoadAudioPack(string _path)
        {
            if(!File.Exists(_path))
            {
                Debug.LogWarning("Audio data not found!");
                yield break;
            }

            var bundleLoadRequest = AssetBundle.LoadFromFileAsync(_path);
            yield return bundleLoadRequest;

            var myLoadedAssetBundle = bundleLoadRequest.assetBundle;

            if (myLoadedAssetBundle == null)
            {
                Debug.LogError("Failed to load Audio AssetBundle!");
                yield break;
            }
            
            // Load audio.json, which contains all the data about the files being loaded
            var audioConfigLR = myLoadedAssetBundle.LoadAssetAsync<TextAsset>("audio");
            yield return audioConfigLR;

            TextAsset _audioconfig = audioConfigLR.asset as TextAsset;

            if (_audioconfig == null)
            {
                Debug.LogError("Audio config reading failed!");
                yield break;
            }

            AudioData _audioData = JsonUtility.FromJson<AudioData>(_audioconfig.text);

            foreach(string _clipID in _audioData.audioclips)
            {
                var assetLoadRequest = myLoadedAssetBundle.LoadAssetAsync<AudioClip>(_clipID);
                yield return assetLoadRequest;

                AudioClip _audioClip = assetLoadRequest.asset as AudioClip;

                LoadedAudioData[_clipID] = _audioClip;
            }
            
            myLoadedAssetBundle.Unload(false);
        }

        private static void PopulatePool()
        {
            PoolRoot = new GameObject("Audio Source Pool");
            UnityEngine.Object.DontDestroyOnLoad(PoolRoot);
            CoroutineRunner = PoolRoot.AddComponent<AudioManagerCoroutineRunner>();

            for(int i = 0; i < POOLSIZE; i++)
            {
                GameObject newASource_obj = new GameObject("ASource_" + i);
                newASource_obj.transform.SetParent(PoolRoot.transform);
                AudioSource newASource = newASource_obj.AddComponent<AudioSource>();
                ConfigNewASource(newASource);
                AudioSourcePool.Add(newASource);
            }
        }

        private static void ConfigNewASource(AudioSource _source)
        {
            
        }

        private static void MoveNextPool()
        {
            if(AudioSourcePoolID < POOLSIZE -1)
                AudioSourcePoolID++;
            else
                AudioSourcePoolID = 0;
        }

        public static void ResetAll()
        {
            for(int i = 0; i < POOLSIZE; i++)
            {
                ResetAudio(i);
            }
        }

        public static void ResetAudio(int IDx)
        {
            AudioSource _s = AudioSourcePool[IDx];
            _s.Stop();
            ConfigNewASource(_s);
        }

        public static void ResetAudio(AudioSource _source)
        {
            _source.Stop();
            ConfigNewASource(_source);
        }

        public static AudioSource PlayAudio(AudioClip _clip, Vector3 _audioPosition = new Vector3(), bool _isLoop = false, bool _is2D = true)
        {
            AudioSource _s = GetNextAudio();
            _s.clip = _clip;
            _s.loop = _isLoop;
            _s.transform.position = _audioPosition;
            _s.Play();

            return _s;
        }

        public static AudioSource PlayAudio(string _clipID, Vector3 _audioPosition = new Vector3(), bool _isLoop = false)
        {
            if(!LoadedAudioData.ContainsKey(_clipID))
                return null;

            AudioSource _s = GetNextAudio();
            _s.clip = LoadedAudioData[_clipID];
            _s.loop = _isLoop;
            _s.transform.position = _audioPosition;
            _s.Play();

            return _s;
        }

        public static AudioSource GetNextAudio()
        {
            AudioSource _s = AudioSourcePool[AudioSourcePoolID];
            MoveNextPool();
            return _s;
        }
    }

    internal sealed class AudioManagerCoroutineRunner : MonoBehaviour
    {
    }
}
