using UnityEngine;

public class OverallGraphicController : MonoBehaviour
{
    public RenderTexture renderTexture;
    [ContextMenu("1920x1080")]
    public void Resize1920()
    {
        ResizeRT(1920, 1080);
    }
    [ContextMenu("1366x768")]
    public void Resize1366()
    {
        ResizeRT(1366, 768);
    }
    [ContextMenu("QHD")]
    public void Resize2560()
    {
        ResizeRT(2560, 1440);
    }
    [ContextMenu("4K")]
    public void Resize4K()
    {
        ResizeRT(3840, 2160);
    }

    public void ResizeRT(int w, int h)
    {
        renderTexture.Release();
        renderTexture.width = w;
        renderTexture.height = h;
        renderTexture.Create();
    }
}
