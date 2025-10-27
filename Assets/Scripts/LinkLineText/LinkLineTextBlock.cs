using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class LinkLineTextBlock : MonoBehaviour
{
    [Header("Visuals")]
    public string Text;
    public int Seq;
    public TextMeshProUGUI textMeshProUGUI;
    public Image image;

    // 引用
    private LinkLinePuzzle puzzle;
    private RectTransform rectTransform;
    public RectTransform Rect => rectTransform;

    public static Material PublicMat { get; private set; }
    // 运行时克隆材质的引用（仅当本块需要独立/分组动画时才会生成）
    private Material _runtimeMat;

    void Start()
    {
        if (textMeshProUGUI != null) textMeshProUGUI.text = Text;

        puzzle = transform.parent.GetComponent<LinkLinePuzzle>();
        rectTransform = GetComponent<RectTransform>();

        var grid = puzzle.IndexToGrid(transform.GetSiblingIndex());
        puzzle.RegisterBlock(this, grid.x, grid.y);

        if (PublicMat == null)
        {
            PublicMat = this.image.material;
        }
    }

    void OnDisable()
    {
        ReleaseRuntimeMaterialIfAny();
    }

    void OnDestroy()
    {
        ReleaseRuntimeMaterialIfAny();
    }

    // ====== 颜色由管理器统一控制 ======
    Coroutine coroutine;
    public void SetColor(Color c)
    {
        if (image != null)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
            coroutine = StartCoroutine(IEChangeColor(c));
        }
    }

    [ContextMenu("Reset Block (visual)")]
    public void ResetBlock()
    {
        if (image != null)
        {
            image.color = Color.white;
            // 如材质存在，重置 fill 参数，避免残留
            var mat = image.material;
            if (mat != null && mat.HasProperty("_f")) mat.SetFloat("_f", 0f);
        }
    }

    // === 获取/克隆运行时材质（会赋到 image.material）===
    public Material GetClonedRuntimeMaterial()
    {
        if (image == null) return null;
        if (_runtimeMat != null) return _runtimeMat;

        // 从 image.material（不是 materialForRendering）克隆
        var template = image.material != null ? image.material : image.defaultMaterial;
        _runtimeMat = LinkLineTextBlock_MaterialPool.Get(template);
        _runtimeMat.name = $"BlockMat_{GetInstanceID()}";
        image.material = _runtimeMat; // 绑定到本块，隔离共享材质
        return _runtimeMat;
    }

    // === 新增：外部传入材质来做填充动画（推荐在管理器里调用）===
    public void FillColor(Material mat)
    {
        if (mat == null) mat = GetClonedRuntimeMaterial();
        StartCoroutine(IEFillColor(mat));
    }

    // === 保留一个便捷重载：本块自建材质后播放 ===
    public void FillColor()
    {
        var mat = GetClonedRuntimeMaterial();
        StartCoroutine(IEFillColor(mat));
    }

    IEnumerator IEFillColor(Material material)
    {
        float counter = 0f;

        if (material != null && material.HasProperty("_f"))
            material.SetFloat("_f", 0f);

        while (counter < LinkLinePuzzle.max_anim_time)
        {
            counter += Time.deltaTime;
            if (material != null && material.HasProperty("_f"))
                material.SetFloat("_f", 2 * counter / LinkLinePuzzle.max_anim_time);
            yield return null;
        }
        if (material != null && material.HasProperty("_f"))
            material.SetFloat("_f", 2f);
    }

    IEnumerator IEChangeColor(Color color)
    {
        Vector3 start = new(image.color.r, image.color.g, image.color.b);
        Vector3 end = new(color.r, color.g, color.b);
        float counter = 0;
        while (counter < 1f)
        {
            counter += Time.deltaTime;
            start = Vector3.Lerp(start, end, 0.05f);
            image.color = new Color(start.x, start.y, start.z);
            yield return null;
        }
        image.color = color;
    }

    // === 回收当前块的运行时材质 ===
    public void ReleaseRuntimeMaterialIfAny()
    {
        if (_runtimeMat != null)
        {
            // 复位可选参数，避免脏值进入池
            if (_runtimeMat.HasProperty("_f")) _runtimeMat.SetFloat("_f", 0f);
            LinkLineTextBlock_MaterialPool.Release(_runtimeMat);
            _runtimeMat = null;
        }
    }
}
