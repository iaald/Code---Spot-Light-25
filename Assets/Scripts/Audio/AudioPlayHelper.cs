using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayHelper : MonoBehaviour
{
    public AudioClip audioClip;
    public void PH()
    {
        if (AudioMng.Instance == null || audioClip == null) return;
        AudioMng.Instance.PlayHigherOneshot(audioClip);
    }
    public void PL()
    {
        if (AudioMng.Instance == null || audioClip == null) return;
        AudioMng.Instance.PlayLowerOneshot(audioClip);
    }
    public void PC()
    {
        if (AudioMng.Instance == null || audioClip == null) return;
        AudioMng.Instance.PlayCurrentOneshot(audioClip);
    }
    public void PlayClickSound()
    {
        if (AudioMng.Instance == null || audioClip == null) return;
        AudioMng.Instance.PlaySound("click");
    }
}
