using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayHelper : MonoBehaviour
{
    public AudioClip audioClip;
    public void PH()
    {
        AudioMng.Instance.PlayHigherOneshot(audioClip);
    }
    public void PL()
    {
        AudioMng.Instance.PlayLowerOneshot(audioClip);
    }
    public void PC()
    {
        AudioMng.Instance.PlayCurrentOneshot(audioClip);
    }
}
