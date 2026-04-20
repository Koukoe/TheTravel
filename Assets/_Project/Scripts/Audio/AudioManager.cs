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
    public List<AudioTrack> ambSounds = new List<AudioTrack>();
    public List<AudioTrack> voiceSounds = new List<AudioTrack>();

    private Dictionary<string, SFX> sfxDict = new Dictionary<string, SFX>();
    private Dictionary<string, AudioTrack> bgmDict = new Dictionary<string, AudioTrack>();
    private Dictionary<string, AudioTrack> ambDict = new Dictionary<string, AudioTrack>();
    private Dictionary<string, AudioTrack> voiceDict = new Dictionary<string, AudioTrack>();

    private AudioChannelGroup _bgmGroup;
    private AudioChannelGroup _ambGroup;
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
        _ambGroup = CreateGroup("Ambient");
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
        src.spatialBlend = 0f; // 保持你原来的 2D 设置
        return src;
    }

    #region 公开接口

    /// <summary> 播放 BGM </summary>
    public void PlayBGM(string name, float fade = 1.0f)
    {
        if (bgmDict.TryGetValue(name, out var s)) _bgmGroup.Play(s, fade, bgmGroup);
    }
    public void StopBGM(StopTarget target = StopTarget.All, float fade = 1.0f) => _bgmGroup.Stop(target, fade);

    /// <summary> 播放环境音 </summary>
    public void PlayAmbient(string name, float fade = 1.0f)
    {
        if (ambDict.TryGetValue(name, out var s)) _ambGroup.Play(s, fade, ambGroup);
    }
    public void StopAmbient(StopTarget target = StopTarget.All, float fade = 1.0f) => _ambGroup.Stop(target, fade);

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

    private IEnumerator ReturnToPool(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null) PoolManager.Release(obj);
    }
    #endregion


    public void SetGroupVolume(string paramName, float volume)
    {
        float dB = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20f;
        sfxGroup.audioMixer.SetFloat(paramName, dB);
    }
}