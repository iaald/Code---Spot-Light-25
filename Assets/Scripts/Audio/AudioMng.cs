using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class AudioMng : MonoBehaviour
{
    public static AudioMng Instance { get; private set; }

    [Header("BGM")]
    public AudioClip bgm1;
    public AudioClip bgm2;
    public AudioSource audioSource;

    [Header("SFX (oneshot & pitch random)")]
    public AudioSource sfxSource;

    public int puzzleSolveLevel = 1;
    public void PlayPuzzleSolvedSound()
    {
        if (puzzleSolveLevel < 0) return;
        PlaySound($"PuzzlePassed{puzzleSolveLevel}");
        puzzleSolveLevel = 1;
    }

    void Awake()
    {
        Instance = this;
        InitializePools();
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void PlayMusic(string name, float volume = 1)
    {
        audioSource.volume = volume;
        if (name == "1")
        {
            audioSource.clip = bgm1;
            audioSource.Play();
        }
        else
        {
            audioSource.clip = bgm2;
            audioSource.Play();
        }
    }

    public void PlaySound(string name, bool useRandom = false)
    {
        AudioClip audioClip = Resources.Load<AudioClip>(name);
        if (audioClip == null) return;

        audioClip.LoadAudioData();

        if (useRandom)
        {
            sfxSource.pitch = pitchOffsets[idx++];
            if (idx == pitchOffsets.Length) idx = 0;
        }
        else
        {
            sfxSource.pitch = 1f;
        }
        sfxSource.PlayOneShot(audioClip);
    }

    private static float[] pitchOffsets = new float[] {
        1.2f, 1.8f, 1.5f, 0.8f, 1.1f, 0.9f, 1.1f, 1f, 1.2f, 0.9f
    };
    int idx = 0;
}
