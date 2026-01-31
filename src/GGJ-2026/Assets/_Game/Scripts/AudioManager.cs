using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private float audioSourceDestroyDelay = 0.1f;

    [Header("Default Values")]
    [SerializeField] private AudioClipSettings defaultAudioClipSettings = AudioClipSettings.Default;
    [SerializeField] private PitchSettingsTemplate defaultPitchSettings;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayAudioClip(AudioClip audioClip)
    {
        var settings = defaultAudioClipSettings;

        settings.pitchSettings =
            defaultPitchSettings == PitchSettingsTemplate.DEFAULT
                ? PitchSettings.Default
                : PitchSettings.SFX;

        PlayAudioClip(audioClip, settings);
    }

    public void PlayAudioClip(AudioClip audioClip, AudioClipSettings settings)
    {
        if (audioClip == null) return;

        AudioSource audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.clip = audioClip;
        audioSource.volume = settings.volume;
        audioSource.pitch = settings.Pitch;
        audioSource.loop = false; // IMPORTANT

        StartCoroutine(PlayAndDestroy(audioSource, settings));
    }

    private System.Collections.IEnumerator PlayAndDestroy(AudioSource source, AudioClipSettings settings)
    {
        int playCount = Mathf.Max(1, settings.loops + 1);

        for (int i = 0; i < playCount; i++)
        {
            source.Play();
            yield return new WaitForSeconds(source.clip.length);
        }

        yield return new WaitForSeconds(audioSourceDestroyDelay);
        Destroy(source);
    }
}
[Serializable]
public struct AudioClipSettings
{
    public static AudioClipSettings Default => new AudioClipSettings(1f);
    public static AudioClipSettings SFX => new AudioClipSettings(1f, PitchSettings.SFX);

    public float volume;
    public PitchSettings pitchSettings;
    public int loops;

    public float Pitch => pitchSettings.Pitch;
    public bool IsLooped => loops > 0;

    public AudioClipSettings(float volume, int loops = 0)
    {
        this.volume = volume;
        this.pitchSettings = PitchSettings.Default;
        this.loops = loops;
    }

    public AudioClipSettings(float volume, PitchSettings pitchSettings, int loops = 0)
    {
        this.volume = volume;
        this.pitchSettings = pitchSettings;
        this.loops = loops;
    }
}
[Serializable]
public struct PitchSettings
{
    public static PitchSettings Default => new PitchSettings(1f);
    public static PitchSettings SFX => new PitchSettings(0.8f, 1.2f);

    public float Pitch;

    public PitchSettings(float pitch)
    {
        Pitch = pitch;
    }

    public PitchSettings(float min, float max)
    {
        Pitch = Random.Range(min, max);
    }
}
public enum PitchSettingsTemplate
{
    DEFAULT,
    SFX
}
