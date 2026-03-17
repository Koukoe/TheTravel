using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
}

[System.Serializable]
public class SFX : Sound
{
    [Range(0.1f, 3f)] public float pitch = 1f;
    [Range(0f, 0.3f)] public float pitchRandom = 0.05f;
    [HideInInspector] public float lastPlayTime;
}

[System.Serializable]
public class AudioTrack : Sound
{
    public bool loop;
}

public enum StopTarget
{
    Oldest,
    Latest,
    All
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("对象池配置")]
    public string audioPrefabName = "AudioSource";

    [Header("混音器")]
    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup bgmGroup;
    public AudioMixerGroup ambGroup;
    public AudioMixerGroup voiceGroup;

    [Header("音频库")]
    public List<SFX> sfxSounds = new List<SFX>();
    public List<AudioTrack> bgmSounds = new List<AudioTrack>();
    public List<AudioTrack> ambSounds = new List<AudioTrack>();
    public List<AudioTrack> voiceSounds = new List<AudioTrack>();

    private Dictionary<string, SFX> sfxDict = new Dictionary<string, SFX>();
    private Dictionary<string, AudioTrack> bgmDict = new Dictionary<string, AudioTrack>();
    private Dictionary<string, AudioTrack> ambDict = new Dictionary<string, AudioTrack>();
    private Dictionary<string, AudioTrack> voiceDict = new Dictionary<string, AudioTrack>();

    private AudioSource[] channels = new AudioSource[6];
    private Coroutine[] channelCoroutines = new Coroutine[6];

    // 轮询以确保双轨切换不冲突
    private int _bgmToggle = 0;    // 0, 1
    private int _ambToggle = 2;    // 2, 3
    private int _voiceToggle = 4;  // 4, 5

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitDicts();
        }
        else { Destroy(gameObject); }
    }

    private void InitDicts()
    {
        foreach (var s in sfxSounds) sfxDict[s.name] = s;
        foreach (var s in bgmSounds) bgmDict[s.name] = s;
        foreach (var s in ambSounds) ambDict[s.name] = s;
        foreach (var s in voiceSounds) voiceDict[s.name] = s;
    }

    #region 公开接口

    /// <summary> 播放 BGM </summary>
    /// /// <param name="fadeTime">淡入时间</param>
    public void PlayBGM(string name, float fadeTime = 1.0f)
    {
        if (!bgmDict.TryGetValue(name, out AudioTrack s)) return;
        if (IsClipPlayingOnRange(s.clip, 0, 1)) return;  // 防止重复播放

        StopChannel(_bgmToggle, fadeTime);
        PlayOnChannel(_bgmToggle, s, fadeTime, bgmGroup);
        _bgmToggle = 1 - _bgmToggle;
    }

    /// <summary> 停止 BGM </summary>
    /// <param name="target">指定要停止的目标 (Oldest, Latest, All)</param>
    /// <param name="fadeTime">淡出时间</param>
    public void StopBGM(StopTarget target = StopTarget.All, float fadeTime = 1.0f)
    {
        switch (target)
        {
            case StopTarget.Oldest:
                StopChannel(_bgmToggle, fadeTime);
                break;

            case StopTarget.Latest:
                StopChannel(1 - _bgmToggle, fadeTime);
                break;

            case StopTarget.All:
                StopChannel(0, fadeTime);
                StopChannel(1, fadeTime);
                break;
        }
    }

    /// <summary> 播放环境音 </summary>
    public void PlayAmbient(string name, float fadeTime = 1.0f)
    {
        if (!ambDict.TryGetValue(name, out AudioTrack s)) return;
        if (IsClipPlayingOnRange(s.clip, 2, 3)) return;

        StopChannel(_ambToggle, fadeTime);
        PlayOnChannel(_ambToggle, s, fadeTime, ambGroup);
        _ambToggle = 5 - _ambToggle;
    }

    /// <summary> 停止环境音 </summary>
    public void StopAmbient(StopTarget target = StopTarget.All, float fadeTime = 1.0f)
    {
        switch (target)
        {
            case StopTarget.Oldest:
                StopChannel(_ambToggle, fadeTime);
                break;

            case StopTarget.Latest:
                StopChannel(5 - _ambToggle, fadeTime);
                break;

            case StopTarget.All:
                StopChannel(2, fadeTime);
                StopChannel(3, fadeTime);
                break;
        }
    }


    /// <summary> 播放人声对白 </summary>
    public void PlayVoice(string name, float fadeTime = 0f)
    {
        if (!voiceDict.TryGetValue(name, out AudioTrack s)) return;
        if (IsClipPlayingOnRange(s.clip, 4, 5)) return;

        StopChannel(_voiceToggle, fadeTime);
        PlayOnChannel(_voiceToggle, s, fadeTime, voiceGroup);
        _voiceToggle = 9 - _voiceToggle;
    }

    /// <summary> 停止人声对白 </summary>
    public void StopVoice(StopTarget target = StopTarget.All, float fadeTime = 0f)
    {
        switch (target)
        {
            case StopTarget.Oldest:
                StopChannel(_voiceToggle, fadeTime);
                break;

            case StopTarget.Latest:
                StopChannel(9 - _voiceToggle, fadeTime);
                break;

            case StopTarget.All:
                StopChannel(4, fadeTime);
                StopChannel(5, fadeTime);
                break;
        }
    }

    /// <summary> 播放音效 </summary>
    public void PlaySFX(string name, Vector3? pos = null)
    {
        if (!sfxDict.TryGetValue(name, out SFX s)) return;
        if (Time.unscaledTime - s.lastPlayTime < 0.05f) return;  // 限制同一个音效

        GameObject obj = PoolManager.Global.Get(audioPrefabName);
        AudioSource source = obj.GetComponent<AudioSource>();
        source.outputAudioMixerGroup = sfxGroup;
        source.clip = s.clip;
        source.volume = s.volume;
        source.pitch = s.pitch + Random.Range(-s.pitchRandom, s.pitchRandom);

        source.spatialBlend = pos.HasValue ? 1f : 0f;
        if (pos.HasValue) obj.transform.position = pos.Value;

        source.Play();
        s.lastPlayTime = Time.unscaledTime;
        StartCoroutine(ReturnToPool(obj, s.clip.length / Mathf.Max(0.1f, source.pitch)));
    }

    /// <summary>
    /// 暂停所有正在播放的轨道
    /// </summary>
    public void PauseAllChannels()
    {
        for (int i = 0; i <= 5; i++)
        {
            if (channels[i] != null && channels[i].isPlaying)
            {
                channels[i].Pause();
            }
        }
    }

    /// <summary>
    /// 恢复所有已暂停的轨道
    /// </summary>
    public void ResumeAllChannels()
    {
        for (int i = 0; i <= 5; i++)
        {
            if (channels[i] != null && channels[i].isPlaying)
            {
                channels[i].UnPause();
            }
        }
    }

    /// <summary>
    /// 停止所有的轨道
    /// </summary>
    public void StopAllChannels()
    {
        for (int i = 0; i <= 5; i++)
        {
            if (channels[i] != null && channels[i].isPlaying)
            {
                channels[i].Stop();
            }
        }
    }

    #endregion

    #region 内置方法

    private void PlayOnChannel(int index, AudioTrack s, float fadeTime, AudioMixerGroup group)
    {
        // 如果有正在运行的协程，先停止它
        if (channelCoroutines[index] != null)
        {
            Debug.Log($"⚠️ 通道{index} 中断当前协程，准备播放新音乐: {s.name}");
            StopCoroutine(channelCoroutines[index]);
            channelCoroutines[index] = null;
        }

        // 开始新的淡入
        channelCoroutines[index] = StartCoroutine(FadeInChannel(index, s, fadeTime, group));
    }

    private void StopChannel(int index, float fadeTime)
    {
        if (channels[index] == null || !channels[index].isPlaying)
        {
            Debug.Log($"⚠️ 通道{index} 没有在播放，无需停止");
            return;
        }

        if (channelCoroutines[index] != null)
        {
            Debug.Log($"⚠️ 通道 {index} 中断当前协程");
            StopCoroutine(channelCoroutines[index]);
            channelCoroutines[index] = null;
        }

        channelCoroutines[index] = StartCoroutine(FadeOutChannel(index, fadeTime));
    }

    private IEnumerator FadeInChannel(int index, AudioTrack s, float duration, AudioMixerGroup group)
    {
        Coroutine thisCoroutine = channelCoroutines[index];

        if (channels[index] == null) InitChannel(index);
        AudioSource source = channels[index];

        // 如果正在播放，先停止
        if (source.isPlaying)
        {
            source.Stop();
        }

        // 配置新音频
        source.outputAudioMixerGroup = group;
        source.clip = s.clip;
        source.loop = s.loop;
        source.volume = 0f;
        source.Play();

        float elapsed = 0;
        while (elapsed < duration)
        {
            // 检查是否被中断
            if (channelCoroutines[index] != thisCoroutine)
                yield break;

            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, s.volume, elapsed / duration);
            yield return null;
        }

        // 检查并清理
        if (channelCoroutines[index] == thisCoroutine)
        {
            source.volume = s.volume;
            channelCoroutines[index] = null;
        }
    }

    private IEnumerator FadeOutChannel(int index, float duration)
    {
        Coroutine thisCoroutine = channelCoroutines[index];
        AudioSource source = channels[index];

        float startVol = source.volume;
        float elapsed = 0;

        while (elapsed < duration)
        {
            if (channelCoroutines[index] != thisCoroutine)
                yield break;

            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVol, 0, elapsed / duration);
            yield return null;
        }

        if (channelCoroutines[index] == thisCoroutine)
        {
            source.Stop();
            source.clip = null;
            channelCoroutines[index] = null;
        }
    }

    private void InitChannel(int index)
    {
        GameObject go = PoolManager.Global.Get(audioPrefabName);
        go.name = $"Fixed_Channel_{index}";
        go.transform.SetParent(transform);
        channels[index] = go.GetComponent<AudioSource>();
        channels[index].spatialBlend = 0f;  // 完全2D
    }

    private bool IsClipPlayingOnRange(AudioClip clip, int start, int end)
    {
        for (int i = start; i <= end; i++)
        {
            if (channels[i] != null && channels[i].isPlaying && channels[i].clip == clip)
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator ReturnToPool(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null) PoolManager.Release(obj);
    }

    #endregion
}