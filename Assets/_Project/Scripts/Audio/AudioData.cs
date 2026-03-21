using UnityEngine;

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