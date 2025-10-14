using UnityEngine;

public interface IUnreadableMasker
{
    void DoMask();
    void RemoveMask();
    void SetRange(int S, int E);
    void SetIntensity(float intensity);
    void SetBlur(float radius);
}
