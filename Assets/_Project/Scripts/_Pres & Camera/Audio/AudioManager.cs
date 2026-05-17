using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("对象池配置")]
    public string audioPrefabName = "AudioSource";

    [Header("混音器")]
    public AudioMixerGroup sfxGroup, bgmGroup, ambGroup, voiceGroup;

    [Header("音频库")]
    public List<SFX> sfxSounds = new List<SFX>();
    public List<AudioTrack> bgmSounds = new List<AudioTrack>();
    public List<Sound> ambSounds = new List<Sound>();
    public List<AudioTrack> voiceSounds = new List<AudioTrack>();

    private Dictionary<string, SFX> sfxDict = new Dictionary<string, SFX>();
    private Dictionary<string, AudioTrack> bgmDict = new Dictionary<string, AudioTrack>();
    private Dictionary<string, Sound> ambDict = new Dictionary<string, Sound>();
    private Dictionary<string, AudioTrack> voiceDict = new Dictionary<string, AudioTrack>();

    private Dictionary<GameObject, Coroutine> activeFadeCoroutines = new Dictionary<GameObject, Coroutine>();
    private AudioChannelGroup _bgmGroup;
    private AudioChannelGroup _voiceGroup;

    private void Awake()
    {
        Instance = this;
        InitDicts();
        InitLogicalChannels();
    }

    private void InitDicts()
    {
        foreach (var s in sfxSounds) sfxDict[s.name] = s;
        foreach (var s in bgmSounds) bgmDict[s.name] = s;
        foreach (var s in ambSounds) ambDict[s.name] = s;
        foreach (var s in voiceSounds) voiceDict[s.name] = s;
    }

    private void InitLogicalChannels()
    {
        _bgmGroup = CreateGroup("BGM");
        _voiceGroup = CreateGroup("Voice");
    }

    private AudioChannelGroup CreateGroup(string prefix)
    {
        AudioSource s1 = SpawnSource($"{prefix}_Channel_0");
        AudioSource s2 = SpawnSource($"{prefix}_Channel_1");
        return new AudioChannelGroup(prefix, this, s1, s2);
    }

    private AudioSource SpawnSource(string sourceName)
    {
        GameObject go = PoolManager.Global.Get(audioPrefabName);
        go.name = sourceName;
        go.transform.SetParent(transform);
        AudioSource src = go.GetComponent<AudioSource>();
        src.spatialBlend = 0f;  // 2D
        return src;
    }

    #region 公开接口

    /// <summary> 播放 BGM </summary>
    public void PlayBGM(string name, float fade = 1.0f)
    {
        if (bgmDict.TryGetValue(name, out var s)) _bgmGroup.Play(s, fade, bgmGroup);
    }
    public void StopBGM(StopTarget target = StopTarget.All, float fade = 1.0f) => _bgmGroup.Stop(target, fade);

    /// <summary> 播放人声对白 </summary>
    public void PlayVoice(string name, float fade = 0f)
    {
        if (voiceDict.TryGetValue(name, out var s)) _voiceGroup.Play(s, fade, voiceGroup);
    }
    public void StopVoice(StopTarget target = StopTarget.All, float fade = 0f) => _voiceGroup.Stop(target, fade);

    /// <summary> 播放音效 </summary>
    public void PlaySFX(string name, Vector3? pos = null)
    {
        if (!sfxDict.TryGetValue(name, out SFX s)) return;
        if (Time.unscaledTime - s.lastPlayTime < 0.05f) return;

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
    /// 播放环境音
    /// </summary>
    /// <param name="name">音频库配置名</param>
    /// <param name="pos">3D世界坐标，不传则为2D全局音</param>
    /// <param name="fadeTime">淡入时间</param>
    /// <returns>返回生成的 GameObject</returns>
    public GameObject PlayAmbient(string name, Vector3? pos = null, float fadeTime = 1.0f)
    {
        if (!ambDict.TryGetValue(name, out Sound s)) return null;

        // 创建实例
        GameObject obj = PoolManager.Global.Get(audioPrefabName);
        AudioSource source = obj.GetComponent<AudioSource>();

        // 配置属性
        source.outputAudioMixerGroup = ambGroup;
        source.clip = s.clip;
        source.loop = true;

        // 处理 3D
        source.spatialBlend = pos.HasValue ? 1f : 0f;
        if (pos.HasValue) obj.transform.position = pos.Value;

        source.Play();

        // 6平滑淡入
        if (fadeTime > 0f) StartCoroutine(FadeInInternal(source, s.volume, fadeTime));
        else source.volume = s.volume;

        return obj;
    }

    /// <summary>
    /// 停止环境音
    /// </summary>
    /// <param name="audioObj">PlayAmbient 返回的 GameObject 实例</param>
    /// <param name="fadeTime">淡出时间</param>
    public void StopAmbient(GameObject audioObj, float fadeTime = 1.0f)
    {
        if (audioObj == null) return;
        AudioSource source = audioObj.GetComponent<AudioSource>();

        StartCoroutine(AmbientRecycleCoroutine(audioObj, source, fadeTime));
    }
    #endregion

    #region 内部方法
    private IEnumerator ReturnToPool(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null) PoolManager.Release(obj);
    }

    private IEnumerator FadeInInternal(AudioSource source, float targetVol, float duration)
    {
        source.volume = 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // 安全检查：预防渐变中途场景切换或物体意外销毁
            if (source == null) yield break;

            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / duration);

            // 完美同步你原本的 OutSine 缓动曲线
            float easeT = EasingUtils.GetValue(EasingUtils.EaseType.OutSine, normalizedTime);

            source.volume = Mathf.Lerp(0f, targetVol, easeT);
            yield return null;
        }

        if (source != null) source.volume = targetVol;
    }

    private IEnumerator AmbientRecycleCoroutine(GameObject obj, AudioSource source, float duration)
    {
        if (duration > 0f && source != null && source.isPlaying)
        {
            float startVol = source.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (source == null) yield break;

                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / duration);

                float easeT = EasingUtils.GetValue(EasingUtils.EaseType.InSine, normalizedTime);

                source.volume = Mathf.Lerp(startVol, 0f, easeT);
                yield return null;
            }
        }

        if (obj != null)
        {
            if (source != null)
            {
                source.Stop();
                source.clip = null;
            }
            PoolManager.Release(obj);
        }
    }

    #endregion


    public void SetGroupVolume(string paramName, float volume)
    {
        float dB = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20f;
        sfxGroup.audioMixer.SetFloat(paramName, dB);
    }
}