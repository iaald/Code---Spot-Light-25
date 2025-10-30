using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

public partial class AudioMng : MonoBehaviour
{
    public static AudioMng Instance { get; private set; }

    public AudioSource audioSource;

    [Header("SFX (oneshot & pitch random)")]
    public GameObject SFXSourceTemplate;
    public SimpleObjectPool<GameObject> SFXSourcePool = new SimpleObjectPool<GameObject>(() =>
    {
        var obj = Instantiate(Instance.SFXSourceTemplate, Instance.transform);
        var audioSource = obj.GetComponent<AudioSource>();
        Instance.SFXSources.Add(audioSource);
        return obj;
    }, obj =>
    {
        try
        {
            var temp = obj.GetComponent<AudioSource>();
            temp.Stop();
            temp.clip = null;
            temp.loop = false;
        }
        catch (System.Exception) { }
        obj.SetActive(false);
    });
    public List<AudioSource> SFXSources = new List<AudioSource>();

    public int puzzleSolveLevel = 1;
    public string puzzleSolveSFXContent;
    public void PlayPuzzleSolvedSound()
    {
        if (puzzleSolveLevel < 0)
        {
            if (puzzleSolveLevel == -1)
            {
                try
                {
                    PlaySound(puzzleSolveSFXContent);
                }
                catch { }
                puzzleSolveSFXContent = "";
            }
            else
            {
                return;
            }
        }
        else
        {
            PlaySound($"PuzzlePassed{puzzleSolveLevel}");
            puzzleSolveLevel = 1;
        }
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

    public void PlayMusic(string name, float volume = 1, bool loop = true)
    {
        AudioClip audioClip = Resources.Load<AudioClip>(name);
        if (audioClip == null) return;

        audioClip.LoadAudioData();

        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.clip = audioClip;

        audioSource.Play();
    }

    public void PlayMusicWithFade(string name, float startVolume = 1, bool loop = true, float fadeDuration = 1f, float targetVolume = 0f)
    {
        AudioClip audioClip = Resources.Load<AudioClip>(name);
        if (audioClip == null) return;

        audioClip.LoadAudioData();

        audioSource.volume = startVolume;
        audioSource.loop = loop;
        audioSource.clip = audioClip;
        audioSource.Play();

        StartCoroutine(IFadeAudioSource(audioSource, fadeDuration, targetVolume));
    }
    private IEnumerator IFadeAudioSource(AudioSource audioSource, float fadeDuration, float targetVolume)
    {
        float startVolume = audioSource.volume;
        float time = 0;
        while (time < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = targetVolume;
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }

    public AudioSource PlaySound(string name, float volume = 0.75f, bool useRandom = false)
    {
        AudioClip audioClip = Resources.Load<AudioClip>(name);
        if (audioClip == null) return null;

        audioClip.LoadAudioData();

        var audioSource = SFXSourcePool.Allocate().GetComponent<AudioSource>();
        audioSource.gameObject.SetActive(true);

        audioSource.volume = volume;

        if (useRandom)
        {
            audioSource.pitch = pitchOffsets[idx++];
            if (idx == pitchOffsets.Length) idx = 0;
        }
        else
        {
            audioSource.pitch = 1f;
        }
        audioSource.PlayOneShot(audioClip);
        if (!audioSource.loop)
        {
            StartCoroutine(RecycleAfterTime(audioSource, audioClip.length / Mathf.Abs(audioSource.pitch)));
        }
        return audioSource;
    }

    IEnumerator RecycleAfterTime(AudioSource audioSource, float time)
    {
        yield return new WaitForSecondsRealtime(time);
        SFXSourcePool.Recycle(audioSource.gameObject);
    }

    public AudioSource PlaySoundWithFade(string name, float startVolume = 1, bool loop = false, float fadeDuration = 1f, float targetVolume = 0f)
    {
        AudioClip audioClip = Resources.Load<AudioClip>(name);
        if (audioClip == null) return null;

        audioClip.LoadAudioData();
        var audioSource = SFXSourcePool.Allocate().GetComponent<AudioSource>();

        audioSource.volume = startVolume;
        audioSource.loop = loop;
        audioSource.clip = audioClip;
        audioSource.Play();
        StartCoroutine(IFadeAudioSource(audioSource, fadeDuration, targetVolume));

        return audioSource;
    }

    public void StopSound(AudioSource audioSource)
    {
        audioSource.Stop();
        SFXSourcePool.Recycle(audioSource.gameObject);
    }

    private static float[] pitchOffsets = new float[] {
        1.2f, 1.8f, 1.5f, 0.8f, 1.1f, 0.9f, 1.1f, 1f, 1.2f, 0.9f
    };
    int idx = 0;
}
