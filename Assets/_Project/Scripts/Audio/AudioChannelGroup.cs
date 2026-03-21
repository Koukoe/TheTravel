using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AudioChannelGroup
{
    private string _groupName;
    private MonoBehaviour _owner;
    private AudioSource[] _channels = new AudioSource[2];
    private Coroutine[] _channelCoroutines = new Coroutine[2];

    // 轮询以确保双轨切换不冲突 (0, 1)
    private int _toggle = 0;

    public AudioChannelGroup(string name, MonoBehaviour owner, AudioSource s1, AudioSource s2)
    {
        _groupName = name;
        _owner = owner;
        _channels[0] = s1;
        _channels[1] = s2;
    }

    public void Play(AudioTrack s, float fadeTime, AudioMixerGroup group)
    {
        // 防止重复播放
        if (IsClipPlaying(s.clip)) return;

        // 停止当前轨道（淡出）
        StopChannel(_toggle, fadeTime);

        // 切换轮询索引
        _toggle = 1 - _toggle;

        // 在新轨道播放
        if (_channelCoroutines[_toggle] != null)
        {
            Debug.Log($"⚠️ [{_groupName}] 通道{_toggle} 中断当前协程，准备播放: {s.name}");
            _owner.StopCoroutine(_channelCoroutines[_toggle]);
            _channelCoroutines[_toggle] = null;
        }
        _channelCoroutines[_toggle] = _owner.StartCoroutine(FadeInChannel(_toggle, s, fadeTime, group));
    }

    public void Stop(StopTarget target, float fadeTime)
    {
        switch (target)
        {
            case StopTarget.Oldest:
                StopChannel(1 - _toggle, fadeTime);
                break;
            case StopTarget.Latest:
                StopChannel(_toggle, fadeTime);
                break;
            case StopTarget.All:
                StopChannel(0, fadeTime);
                StopChannel(1, fadeTime);
                break;
        }
    }

    private bool IsClipPlaying(AudioClip clip)
    {
        for (int i = 0; i < 2; i++)
        {
            if (_channels[i] != null && _channels[i].isPlaying && _channels[i].clip == clip) return true;
        }
        return false;
    }

    private void StopChannel(int index, float fadeTime)
    {
        if (_channels[index] == null || !_channels[index].isPlaying)
        {
            Debug.Log($"ℹ️ [{_groupName}] 通道{index} 空闲，无需停止");
            return;
        }

        if (_channelCoroutines[index] != null)
        {
            Debug.Log($"⚠️ [{_groupName}] 通道 {index} 正在执行淡入淡出，强制中断并开始淡出");
            _owner.StopCoroutine(_channelCoroutines[index]);
            _channelCoroutines[index] = null;
        }

        _channelCoroutines[index] = _owner.StartCoroutine(FadeOutChannel(index, fadeTime));
    }

    public void PauseGroup()
    {
        for (int i = 0; i < 2; i++)
        {
            if (_channels[i] != null && _channels[i].isPlaying)
            {
                _channels[i].Pause();
            }
        }
    }

    public void ResumeGroup()
    {
        for (int i = 0; i < 2; i++)
        {
            if (_channels[i] != null)
            {
                _channels[i].UnPause();
            }
        }
    }

    public void StopAllImmediate()
    {
        for (int i = 0; i < 2; i++)
        {
            if (_channels[i] != null)
            {
                if (_channelCoroutines[i] != null)
                {
                    _owner.StopCoroutine(_channelCoroutines[i]);
                    _channelCoroutines[i] = null;
                }
                _channels[i].Stop();
                _channels[i].clip = null;
            }
        }
    }

    private IEnumerator FadeInChannel(int index, AudioTrack s, float duration, AudioMixerGroup group)
    {
        Coroutine thisCoroutine = _channelCoroutines[index];
        AudioSource source = _channels[index];

        if (source.isPlaying) source.Stop();

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
            if (_channelCoroutines[index] != thisCoroutine) yield break;

            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, s.volume, elapsed / duration);
            yield return null;
        }

        // 检查并清理
        if (_channelCoroutines[index] == thisCoroutine)
        {
            source.volume = s.volume;
            _channelCoroutines[index] = null;
        }
    }

    private IEnumerator FadeOutChannel(int index, float duration)
    {
        Coroutine thisCoroutine = _channelCoroutines[index];
        AudioSource source = _channels[index];

        float startVol = source.volume;
        float elapsed = 0;

        while (elapsed < duration)
        {
            if (_channelCoroutines[index] != thisCoroutine) yield break;

            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVol, 0, elapsed / duration);
            yield return null;
        }

        if (_channelCoroutines[index] == thisCoroutine)
        {
            source.Stop();
            source.clip = null;
            _channelCoroutines[index] = null;
        }
    }
}