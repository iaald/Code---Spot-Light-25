using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Kuchinashi.Utils.Progressable;

public class DestroyChildrenWithParticles : MonoBehaviour
{
    [Header("Particle System Settings")]
    public ParticleSystem particlePrefab;
    public float particleLifetime = 0f;

    private TextMeshProUGUI tmpText;
    private CanvasGroupAlphaProgressable canvasGroup;

    [ContextMenu("Destroy All Children With Particles")]

    private void Start()
    {
        tmpText = GetComponent<TextMeshProUGUI>();

        canvasGroup ??= GetComponent<CanvasGroupAlphaProgressable>();
        canvasGroup.LinearTransition(0.5f);
    }
    public void DestroyChildren()
    {
        if (particlePrefab == null) return;

        var children = GetComponentsInChildren<RectTransform>();

        foreach (var child in children)
        {
            if (child == transform) continue;

            Vector3 worldPos = child.transform.position;

            ParticleSystem ps = Instantiate(particlePrefab, worldPos, Quaternion.identity, transform.parent);

            var psRenderer = ps.GetComponent<ParticleSystemRenderer>();
            if (psRenderer != null)
            {
                psRenderer.sortingLayerID = GetComponentInParent<Canvas>().sortingLayerID;
                psRenderer.sortingOrder = 999;
            }

            Destroy(ps.gameObject, particleLifetime);

            canvasGroup.InverseLinearTransition(0.5f);
            Destroy(child.gameObject);
            Destroy(gameObject, particleLifetime);
        }
    }
}