// using UnityEngine;
// using UnityEngine.Audio;
// using System.Collections;
// using System.Collections.Generic;

// [System.Serializable]
// public class Sound
// {
//     public string name;
//     public AudioClip clip;
//     [Range(0f, 1f)] public float volume = 1f;
// }

// [System.Serializable]
// public class SFX : Sound
// {
//     [Range(0.1f, 3f)] public float pitch = 1f;
//     [Range(0f, 0.3f)] public float pitchRandom = 0.05f;
//     [HideInInspector] public float lastPlayTime;
// }

// [System.Serializable]
// public class AudioTrack : Sound
// {
//     public bool loop;
// }

// public class AudioManager : MonoBehaviour
// {
//     public static AudioManager Instance;

//     [Header("对象池配置")]
//     public string audioPrefabName = "AudioSource";

//     [Header("混音器")]
//     public AudioMixerGroup sfxGroup;
//     public AudioMixerGroup bgmGroup;
//     public AudioMixerGroup ambGroup;
//     public AudioMixerGroup voiceGroup;

//     [Header("音频库")]
//     public List<SFX> sfxSounds = new List<SFX>();
//     public List<AudioTrack> bgmSounds = new List<AudioTrack>();
//     public List<AudioTrack> ambSounds = new List<AudioTrack>();
//     public List<AudioTrack> voiceSounds = new List<AudioTrack>();

//     private Dictionary<string, SFX> sfxDict = new Dictionary<string, SFX>();
//     private Dictionary<string, AudioTrack> bgmDict = new Dictionary<string, AudioTrack>();
//     private Dictionary<string, AudioTrack> ambDict = new Dictionary<string, AudioTrack>();
//     private Dictionary<string, AudioTrack> voiceDict = new Dictionary<string, AudioTrack>();

//     private AudioSource[] channels = new AudioSource[6];
//     private Coroutine[] channelCoroutines = new Coroutine[6];

//     // 轮询以确保双轨切换不冲突
//     private int _bgmToggle = 0;    // 0, 1
//     private int _ambToggle = 2;    // 2, 3
//     private int _voiceToggle = 4;  // 4, 5

//     private void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//             InitDicts();
//         }
//         else { Destroy(gameObject); }
//     }

//     private void InitDicts()
//     {
//         foreach (var s in sfxSounds) sfxDict[s.name] = s;
//         foreach (var s in bgmSounds) bgmDict[s.name] = s;
//         foreach (var s in ambSounds) ambDict[s.name] = s;
//         foreach (var s in voiceSounds) voiceDict[s.name] = s;
//     }

//     #region 公开接口

//     /// <summary> 播放 BGM </summary>
//     public void PlayBGM(string name, float fadeTime = 1.0f)
//     {
//         if (!bgmDict.TryGetValue(name, out AudioTrack s)) return;

//         if (IsClipPlayingOnRange(s.clip, 0, 1)) return;

//         int oldIdx = _bgmToggle;
//         _bgmToggle = 1 - _bgmToggle;
//         int newIdx = _bgmToggle;

//         StopChannel(oldIdx, fadeTime);
//         PlayOnChannel(newIdx, s, fadeTime, bgmGroup);
//     }

//     /// <summary> 停止 BGM </summary>
//     public void StopBGM(string name, float fadeTime = 1.0f)
//     {
//         if (!bgmDict.TryGetValue(name, out Sound s)) return;
//         if (IsClipPlayingOnRange(s.clip, 0, 1)) return;

//         int oldIdx = _bgmToggle;
//         _bgmToggle = (_bgmToggle == 0) ? 1 : 0;
//         int newIdx = _bgmToggle;

//         StopChannel(oldIdx, fadeTime);
//         PlayOnChannel(newIdx, s, fadeTime, bgmGroup);
//     }

//     /// <summary> 播放环境音 </summary>
//     public void PlayAmbient(string name, float fadeTime = 2.0f)
//     {
//         if (!ambDict.TryGetValue(name, out Sound s)) return;
//         if (IsClipPlayingOnRange(s.clip, 2, 3)) return;

//         int targetIdx = _ambToggle;
//         _ambToggle = (_ambToggle == 2) ? 3 : 2;

//         PlayOnChannel(targetIdx, s, fadeTime, ambGroup);
//     }

//     /// <summary> 停止环境音 </summary>
//     public void StopAmbient(string name, float fadeTime = 2.0f)
//     {
//         if (!ambDict.TryGetValue(name, out Sound s)) return;
//         if (IsClipPlayingOnRange(s.clip, 2, 3)) return;

//         int targetIdx = _ambToggle;
//         _ambToggle = (_ambToggle == 2) ? 3 : 2;

//         PlayOnChannel(targetIdx, s, fadeTime, ambGroup);
//     }


//     /// <summary> 播放人声对白 </summary>
//     public void PlayVoice(string name, float fadeTime = 0f)
//     {
//         if (!voiceDict.TryGetValue(name, out Sound s)) return;

//         int targetIdx = _voiceToggle;
//         _voiceToggle = (_voiceToggle == 4) ? 5 : 4;

//         PlayOnChannel(targetIdx, s, fadeTime, voiceGroup);
//     }

//     /// <summary> 停止人声对白 </summary>
//     public void StopVoice(string name, float fadeTime = 0f)
//     {
//         if (!voiceDict.TryGetValue(name, out Sound s)) return;

//         int targetIdx = _voiceToggle;
//         _voiceToggle = (_voiceToggle == 4) ? 5 : 4;

//         PlayOnChannel(targetIdx, s, fadeTime, voiceGroup);
//     }

//     /// <summary> 播放瞬发音效 (走对象池) </summary>
//     public void PlaySFX(string name, Vector3? pos = null)
//     {
//         if (!sfxDict.TryGetValue(name, out Sound s)) return;
//         if (Time.unscaledTime - s.lastPlayTime < 0.05f) return;  // 限制同一个音效

//         GameObject obj = PoolManager.Global.Get(audioPrefabName);
//         AudioSource source = obj.GetComponent<AudioSource>();
//         source.outputAudioMixerGroup = sfxGroup;
//         source.clip = s.clip;
//         source.volume = s.volume;
//         source.pitch = s.pitch + Random.Range(-0.05f, 0.05f);

//         source.spatialBlend = pos.HasValue ? 1f : 0f;
//         if (pos.HasValue) obj.transform.position = pos.Value;

//         source.Play();
//         s.lastPlayTime = Time.unscaledTime;
//         if (!s.loop) StartCoroutine(ReturnToPool(obj, s.clip.length / Mathf.Max(0.1f, source.pitch)));
//     }

//     #endregion

//     #region 内置方法

//     private void PlayOnChannel(int index, Sound s, float fadeTime, AudioMixerGroup group)
//     {
//         if (channelCoroutines[index] != null) StopCoroutine(channelCoroutines[index]);
//         channelCoroutines[index] = StartCoroutine(FadeInChannel(index, s, fadeTime, group));
//     }

//     private void StopChannel(int index, float fadeTime)
//     {
//         if (channels[index] == null || !channels[index].isPlaying) return;
//         if (channelCoroutines[index] != null) StopCoroutine(channelCoroutines[index]);
//         channelCoroutines[index] = StartCoroutine(FadeOutChannel(index, fadeTime));
//     }

//     private IEnumerator FadeInChannel(int index, Sound s, float duration, AudioMixerGroup group)
//     {
//         if (channels[index] == null) InitChannel(index);

//         AudioSource source = channels[index];
//         source.outputAudioMixerGroup = group;
//         source.clip = s.clip;
//         source.loop = s.loop;

//         float startVol = source.isPlaying ? source.volume : 0f;
//         if (!source.isPlaying) source.Play();

//         float elapsed = 0;
//         while (elapsed < duration)
//         {
//             elapsed += Time.deltaTime;
//             source.volume = Mathf.Lerp(startVol, s.volume, elapsed / duration);
//             yield return null;
//         }
//         source.volume = s.volume;
//     }

//     private IEnumerator FadeOutChannel(int index, float duration)
//     {
//         AudioSource source = channels[index];
//         float startVol = source.volume;
//         float elapsed = 0;
//         while (elapsed < duration)
//         {
//             elapsed += Time.deltaTime;
//             source.volume = Mathf.Lerp(startVol, 0, elapsed / duration);
//             yield return null;
//         }
//         source.Stop();
//         source.clip = null;
//     }

//     private void InitChannel(int index)
//     {
//         GameObject go = PoolManager.Global.Get(audioPrefabName);
//         go.name = $"Fixed_Channel_{index}";
//         go.transform.SetParent(transform);
//         channels[index] = go.GetComponent<AudioSource>();
//         channels[index].spatialBlend = 0f;
//     }


//     private IEnumerator ReturnToPool(GameObject obj, float delay)
//     {
//         yield return new WaitForSeconds(delay);
//         if (obj != null) PoolManager.Global.Release(obj);
//     }

//     #endregion
// }