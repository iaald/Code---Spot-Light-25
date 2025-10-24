using System.Collections.Generic;
using UnityEngine;

public static class LinkLineTextBlock_MaterialPool
{
    // 简单池：按 Shader 分堆；如果你的模板材质还区分更多特征，可扩展 key
    private static readonly Dictionary<Shader, Stack<Material>> _pool = new();

    public static Material Get(Material template)
    {
        if (template == null)
        {
            // 兜底：用 Unity 内置 UI/Default
            var fallback = new Material(Shader.Find("UI/Default"));
            return fallback;
        }

        var shader = template.shader;
        if (!_pool.TryGetValue(shader, out var stack))
        {
            stack = new Stack<Material>();
            _pool[shader] = stack;
        }

        if (stack.Count > 0)
        {
            var m = stack.Pop();
            // 同步必要属性（颜色/纹理/Keywords），避免历史残留
            CopyCommonProps(template, m);
            return m;
        }

        // 新建并复制关键属性
        var clone = new Material(template) { name = template.name + " (Clone)" };
        return clone;
    }

    public static void Release(Material mat)
    {
        if (mat == null) return;
        var shader = mat.shader;
        if (!_pool.TryGetValue(shader, out var stack))
        {
            stack = new Stack<Material>();
            _pool[shader] = stack;
        }

        // 可根据需要限制池大小
        stack.Push(mat);
    }

    private static void CopyCommonProps(Material from, Material to)
    {
        // 同步常见属性；如需更多，按实际 shader 增补
        if (from.HasProperty("_Color") && to.HasProperty("_Color"))
            to.SetColor("_Color", from.GetColor("_Color"));
        if (from.HasProperty("_MainTex") && to.HasProperty("_MainTex"))
            to.SetTexture("_MainTex", from.GetTexture("_MainTex"));
        if (from.HasProperty("_f") && to.HasProperty("_f"))
            to.SetFloat("_f", from.GetFloat("_f"));

        // 关键词同步（避免渲染差异）
        to.shaderKeywords = from.shaderKeywords;
        to.renderQueue = from.renderQueue;
        to.enableInstancing = from.enableInstancing;
        to.globalIlluminationFlags = from.globalIlluminationFlags;
    }
}
