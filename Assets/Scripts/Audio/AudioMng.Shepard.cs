using UnityEngine;
using System.Collections.Generic;

public partial class AudioMng : MonoBehaviour
{
    [Header("Shepard Tone Tracks (templates: -1, 0, +1 octaves)")]
    [Tooltip("高一八度层（模板）")]
    public AudioSource blockSource1; // 0 octave template
    [Tooltip("中间八度层（模板）")]
    public AudioSource blockSource2; //  -1 octave template
    
    [Header("Audio Source Pool Settings")]
    [Tooltip("音频源池大小")]
    public int poolSize = 10;
    [Tooltip("音频源池自动扩展大小")]
    public int poolExpandSize = 5;

    // 音频源对象池
    private class AudioSourcePool
    {
        private Queue<AudioSource> availableSources = new Queue<AudioSource>();
        private List<AudioSource> allSources = new List<AudioSource>();
        private GameObject parent;
        private AudioSource template;
        private int expandSize;

        public AudioSourcePool(GameObject parent, AudioSource template, int initialSize, int expandSize)
        {
            this.parent = parent;
            this.template = template;
            this.expandSize = expandSize;
            ExpandPool(initialSize);
        }

        public AudioSource Get()
        {
            if (availableSources.Count == 0)
            {
                ExpandPool(expandSize);
            }

            if (availableSources.Count > 0)
            {
                AudioSource source = availableSources.Dequeue();
                source.gameObject.SetActive(true);
                return source;
            }

            return null;
        }

        public void Return(AudioSource source)
        {
            if (source != null)
            {
                source.Stop();
                source.clip = null;
                source.gameObject.SetActive(false);
                availableSources.Enqueue(source);
            }
        }

        private void ExpandPool(int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject go = new GameObject($"PooledAudioSource_{allSources.Count}");
                go.transform.SetParent(parent.transform);
                go.SetActive(false);
                
                AudioSource newSource = go.AddComponent<AudioSource>();
                CopyAudioSourceProperties(template, newSource);
                
                availableSources.Enqueue(newSource);
                allSources.Add(newSource);
            }
        }

        private void CopyAudioSourceProperties(AudioSource from, AudioSource to)
        {
            to.outputAudioMixerGroup = from.outputAudioMixerGroup;
            to.mute = from.mute;
            to.bypassEffects = from.bypassEffects;
            to.bypassListenerEffects = from.bypassListenerEffects;
            to.bypassReverbZones = from.bypassReverbZones;
            to.playOnAwake = from.playOnAwake;
            to.loop = from.loop;
            to.priority = from.priority;
            to.volume = from.volume;
            to.pitch = from.pitch;
            to.panStereo = from.panStereo;
            to.spatialBlend = from.spatialBlend;
            to.reverbZoneMix = from.reverbZoneMix;
            to.dopplerLevel = from.dopplerLevel;
            to.spread = from.spread;
            to.rolloffMode = from.rolloffMode;
            to.minDistance = from.minDistance;
            to.maxDistance = from.maxDistance;
        }

        public void Cleanup()
        {
            foreach (var source in allSources)
            {
                if (source != null)
                {
                    Destroy(source.gameObject);
                }
            }
            availableSources.Clear();
            allSources.Clear();
        }
    }

    private AudioSourcePool track1Pool;
    private AudioSourcePool track2Pool;
    private List<ActiveAudioPlayback> activePlaybacks = new List<ActiveAudioPlayback>();

    private class ActiveAudioPlayback
    {
        public AudioSource track1Source;
        public AudioSource track2Source;
        public AudioClip clip;
        public float duration;
        public float startTime;
    }

    float audioCd = 0.05f;
    bool ableToPlay = true;
    private void Update()
    {
        audioCd -= Time.deltaTime;
        if (audioCd <= 0)
        {
            ableToPlay = true;
            audioCd = 0.05f;
        }
        CleanupFinishedPlaybacks();
    }

    private void OnDestroy()
    {
        track1Pool?.Cleanup();
        track2Pool?.Cleanup();
    }

    private void InitializePools()
    {
        track1Pool = new AudioSourcePool(gameObject, blockSource1, poolSize, poolExpandSize);
        track2Pool = new AudioSourcePool(gameObject, blockSource2, poolSize, poolExpandSize);
    }

    public void PlayHigherOneshot(AudioClip audioClip)
    {
        if (audioClip == null) return;
        if (!ableToPlay) return;
        ableToPlay = false;
        StepUp();
        PlayOneshotInternal(audioClip);
    }

    public void PlayLowerOneshot(AudioClip audioClip)
    {
        if (audioClip == null) return;
        if (!ableToPlay) return;
        ableToPlay = false;
        StepDown();
        PlayOneshotInternal(audioClip);
    }

    public void PlayCurrentOneshot(AudioClip audioClip)
    {
        if (audioClip == null) return;
        if (!ableToPlay) return;
        ableToPlay = false;
        PlayOneshotInternal(audioClip);
    }

    private void PlayOneshotInternal(AudioClip audioClip)
    {
        GetTrackStates(out float t1Pitch, out float t1Volume, out float t2Pitch, out float t2Volume);

        AudioSource track1Source = track1Pool.Get();
        AudioSource track2Source = track2Pool.Get();

        if (track1Source == null || track2Source == null)
        {
            Debug.LogWarning("Audio source pool exhausted, cannot play sound");
            track1Pool.Return(track1Source);
            track2Pool.Return(track2Source);
            return;
        }

        // 设置音调和音量
        track1Source.pitch = t1Pitch;
        track1Source.volume = t1Volume;
        track2Source.pitch = t2Pitch;
        track2Source.volume = t2Volume;

        // 播放音频
        track1Source.clip = audioClip;
        track2Source.clip = audioClip;
        track1Source.Play();
        track2Source.Play();

        // 记录活跃播放
        var playback = new ActiveAudioPlayback
        {
            track1Source = track1Source,
            track2Source = track2Source,
            clip = audioClip,
            duration = audioClip.length,
            startTime = Time.time
        };
        activePlaybacks.Add(playback);
    }

    private void CleanupFinishedPlaybacks()
    {
        for (int i = activePlaybacks.Count - 1; i >= 0; i--)
        {
            var playback = activePlaybacks[i];
            
            // 检查是否播放完成
            if (Time.time - playback.startTime >= playback.duration || 
                !playback.track1Source.isPlaying || !playback.track2Source.isPlaying)
            {
                // 回收音频源
                track1Pool.Return(playback.track1Source);
                track2Pool.Return(playback.track2Source);
                
                activePlaybacks.RemoveAt(i);
            }
        }
    }

    // 轨道状态（原有代码保持不变）
    private float track1Pitch = 1.0f;
    private float track1Volume = 1.0f;
    private float track2Pitch = 0.5f;
    private float track2Volume = 0.0f;
    private int currentSemitones = 0;
    private readonly float semitoneRatio = Mathf.Pow(2f, 1f / 12f);

    public void StepUp()
    {
        currentSemitones++;
        track1Pitch *= semitoneRatio;
        track2Pitch *= semitoneRatio;
        UpdateCrossfade();
        CheckAndResetTracks();
    }

    public void StepDown()
    {
        currentSemitones--;
        track1Pitch /= semitoneRatio;
        track2Pitch /= semitoneRatio;
        UpdateCrossfade();
        CheckAndResetTracks();
    }

    private void UpdateCrossfade()
    {
        float phase = (currentSemitones % 12 + 12) % 12 / 11.0f;
        track1Volume = 1.0f - phase;
        track2Volume = phase;
    }

    private void CheckAndResetTracks()
    {
        if (track1Pitch >= 2.0f)
        {
            track1Pitch = track2Pitch;
            track2Pitch = track1Pitch * 0.5f;
        }
        else if (track1Pitch < 1.0f && track2Pitch <= 0.5f)
        {
            track2Pitch = track1Pitch;
            track1Pitch = track2Pitch * 2.0f;
        }
    }

    public void GetTrackStates(out float t1Pitch, out float t1Volume, out float t2Pitch, out float t2Volume)
    {
        t1Pitch = track1Pitch;
        t1Volume = track1Volume;
        t2Pitch = track2Pitch;
        t2Volume = track2Volume;
    }

    public void ResetToInitialState()
    {
        track1Pitch = 1.0f;
        track1Volume = 1.0f;
        track2Pitch = 0.5f;
        track2Volume = 0.0f;
        currentSemitones = 0;
    }
}