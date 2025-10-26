using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class DestroyChildrenWithParticles : MonoBehaviour
{
    [Header("Ҫ���ŵ�����Ԥ���壨Particle System Prefab��")]
    public ParticleSystem particlePrefab;

    [Header("�����ӳ٣���ѡ��")]
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
            Debug.LogWarning("δָ������Ԥ���壡");
            return;
        }

        // ��ȡ���������壨�������Լ���
        var children = GetComponentsInChildren<RectTransform>();

        foreach (var child in children)
        {
            if (child == transform) continue; // �����Լ�

            // ��¼��ǰ������λ�ã�UI �������꣩
            Vector3 worldPos = child.transform.position;

            // ʵ��������
            ParticleSystem ps = Instantiate(particlePrefab, worldPos, Quaternion.identity, transform.parent);

            // �������� Canvas ����ȷ��ʾ���ؼ���
            var psRenderer = ps.GetComponent<ParticleSystemRenderer>();
            if (psRenderer != null)
            {
                psRenderer.sortingLayerID = GetComponentInParent<Canvas>().sortingLayerID;
                psRenderer.sortingOrder = 999; // ȷ����ʾ�����ϲ�
            }

            // ����һ�κ�����
            Destroy(ps.gameObject, particleLifetime);

            // ����ԭ������
            canvasGroup.DOFade(0f, 1f);
            Destroy(child.gameObject);
            Destroy(gameObject, particleLifetime);
        }
    }
}