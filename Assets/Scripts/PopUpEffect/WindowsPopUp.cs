using UnityEngine;
using UnityEngine.Events;

public class WindowsPopUp : MonoBehaviour
{
    public UnityEvent OnStart;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnStart?.Invoke();

    }
}
