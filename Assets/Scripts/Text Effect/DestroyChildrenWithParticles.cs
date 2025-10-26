using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class DestroyChildrenWithParticles : MonoBehaviour
{
    [Header("要播放的粒子预制体（Particle System Prefab）")]
    public ParticleSystem particlePrefab;

    [Header("销毁延迟（可选）")]
    public float particleLifetime = 0f;

    private TextMeshProUGUI tmpText;
    private CanvasGroup canvasGroup;

    [ContextMenu("Destroy All Children With Particles")]

    private void Start()
    {
     
        canvasGroup= GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        tmpText =GetComponent<TextMeshProUGUI>();
        canvasGroup.DOFade(1f, 2f);
    }
    public void DestroyChildren()
    {
        if (particlePrefab == null)
        {
            Debug.LogWarning("未指定粒子预制体！");
            return;
        }

        // 获取所有子物体（不包括自己）
        var children = GetComponentsInChildren<RectTransform>();

        foreach (var child in children)
        {
            if (child == transform) continue; // 跳过自己

            // 记录当前子物体位置（UI 世界坐标）
            Vector3 worldPos = child.transform.position;

            // 实例化粒子
            ParticleSystem ps = Instantiate(particlePrefab, worldPos, Quaternion.identity, transform.parent);

            // 让粒子在 Canvas 层正确显示（关键）
            var psRenderer = ps.GetComponent<ParticleSystemRenderer>();
            if (psRenderer != null)
            {
                psRenderer.sortingLayerID = GetComponentInParent<Canvas>().sortingLayerID;
                psRenderer.sortingOrder = 999; // 确保显示在最上层
            }

            // 播放一次后销毁
            Destroy(ps.gameObject, particleLifetime);

            // 销毁原子物体
            canvasGroup.DOFade(0f, 1f);
            Destroy(child.gameObject);
            Destroy(gameObject, particleLifetime);
        }
    }
}