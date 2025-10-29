using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public partial class LinkLinePuzzle : MonoBehaviour
{
    [Header("Answers")]
    [Tooltip("Accept lines whatever the content is")]
    public bool OpenAnswer = false;
    public List<string> correctAnswers;
    public int requiredNumber = 1;
    private int correctCnt = 0;

    public const float max_anim_time = 1.5f;

    public UnityEvent OnSolved;

    [Header("Play SFX ahead of Anim")]
    public bool PlaySFXAhead = false;

    // ======== NEW: cache pre-execute actions ========
    private List<Action> _preSolveMarkedActions;

    public bool CheckLine(int lineId)
    {
        if (OpenAnswer)
        {
            correctCnt += 1;
            return true;
        }
        var str = string.Join("", this.GetSequenceFromCommittedLineStart(lineId));
        if (str.Equals("") || correctAnswers.Contains(str))
        {
            correctCnt += 1;
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CheckPuzzle()
    {
        if (correctCnt == requiredNumber)
        {
            if (PlaySFXAhead)
            {
                // ======== TODO 完成：提前执行带有 NeedReorderedSFXSettings 的监听方法 ========
                ExecuteActionsSafely(_preSolveMarkedActions);
                // 然后再播放 SFX（如你原注释所述 “ahead of anim”）
                AudioMng.Instance.PlayPuzzleSolvedSound();
            }

            Invoke(nameof(InvokeOnSolveEvent), max_anim_time);
            return true;
        }
        return false;
    }

    private void InvokeOnSolveEvent()
    {
        OnSolved?.Invoke();
        if (!PlaySFXAhead)
        {
            AudioMng.Instance.PlayPuzzleSolvedSound();
        }
        else
        {
            SetPuzzleSolveSFXContent("");
        }
    }

    public List<string> GetAllResults()
    {
        List<string> results = new();
        foreach (var item in this.committedLines)
        {
            var t = GetSequenceFromCommittedLineStart(item.lineId);
            results.Add(string.Join("", t));
        }
        return results;
    }

    [NeedReorderedSFXSettings]
    public void SetPuzzleSolveStatus(int v)
    {
        AudioMng.Instance.puzzleSolveLevel = v;
    }

    [NeedReorderedSFXSettings]
    public void SetPuzzleSolveSFXContent(string filename)
    {
        AudioMng.Instance.puzzleSolveLevel = -1;
        AudioMng.Instance.puzzleSolveSFXContent = filename;
    }

    // ========= 构建并执行 Action 的通用工具 =========
    /// <summary>
    /// 从 UnityEvent 中提取带指定 Attribute 的监听方法，按其在 Inspector 中配置的参数封装为 Action。
    /// 仅支持 0 或 1 个参数（int/float/bool/string/Object）。
    /// </summary>
    private static List<Action> BuildMarkedActionsFromUnityEvent(UnityEvent evt, Type markAttributeType)
    {
        var actions = new List<Action>();
        if (evt == null) return actions;

        try
        {
            // 反射拿到 UnityEventBase.m_PersistentCalls.m_Calls (List<PersistentCall>)
            var uebType = typeof(UnityEventBase);
            var fPersistentCalls = uebType.GetField("m_PersistentCalls", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fPersistentCalls == null) return actions;

            var persistentCalls = fPersistentCalls.GetValue(evt);
            if (persistentCalls == null) return actions;

            var pcgType = persistentCalls.GetType(); // PersistentCallGroup
            var fCalls = pcgType.GetField("m_Calls", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fCalls == null) return actions;

            var calls = fCalls.GetValue(persistentCalls) as IEnumerable; // List<PersistentCall>
            if (calls == null) return actions;

            foreach (var pc in calls)
            {
                if (pc == null) continue;

                var pcType = pc.GetType(); // PersistentCall
                var fTarget = pcType.GetField("m_Target", BindingFlags.Instance | BindingFlags.NonPublic);
                var fMethodName = pcType.GetField("m_MethodName", BindingFlags.Instance | BindingFlags.NonPublic);
                var fArguments = pcType.GetField("m_Arguments", BindingFlags.Instance | BindingFlags.NonPublic);

                var target = fTarget?.GetValue(pc);
                var methodName = fMethodName?.GetValue(pc) as string;
                var arguments = fArguments?.GetValue(pc);

                if (target == null || string.IsNullOrEmpty(methodName)) continue;

                // 找到实际 MethodInfo（实例或静态）
                var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                var method = target.GetType().GetMethod(methodName, flags);
                if (method == null) continue;

                // 仅收集带指定 Attribute 的方法
                if (markAttributeType != null && method.GetCustomAttribute(markAttributeType, true) == null)
                    continue;

                // 解析参数（0 或 1 个参数）
                var ps = method.GetParameters();
                if (ps.Length == 0)
                {
                    // 无参
                    actions.Add(() =>
                    {
                        try { method.Invoke(target, null); }
                        catch (Exception e) { Debug.LogWarning($"Invoke {method.DeclaringType.Name}.{method.Name} failed: {e}"); }
                    });
                }
                else if (ps.Length == 1)
                {
                    object argVal = GetSingleArgumentFromArgumentCache(arguments, ps[0].ParameterType);
                    if (argVal == null && ps[0].ParameterType.IsValueType)
                    {
                        // 提供默认值
                        argVal = Activator.CreateInstance(ps[0].ParameterType);
                    }

                    actions.Add(() =>
                    {
                        try { method.Invoke(target, new object[] { argVal }); }
                        catch (Exception e) { Debug.LogWarning($"Invoke {method.DeclaringType.Name}.{method.Name}({argVal}) failed: {e}"); }
                    });
                }
                else
                {
                    Debug.LogWarning($"[BuildMarkedActionsFromUnityEvent] {method.DeclaringType.Name}.{method.Name} has {ps.Length} parameters. Only 0/1 is supported.");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[BuildMarkedActionsFromUnityEvent] Reflection failed: {ex}");
        }

        return actions;
    }

    /// <summary>
    /// 从 UnityEngine.Events.ArgumentCache（内部类）中提取 Inspector 配置的单个参数值。
    /// 支持 int/float/bool/string/Object。
    /// </summary>
    private static object GetSingleArgumentFromArgumentCache(object argumentCache, Type expectedType)
    {
        if (argumentCache == null) return null;

        var acType = argumentCache.GetType(); // UnityEngine.Events.ArgumentCache
        // 这些字段名称来自 Unity 内部实现，版本差异可能会变动
        var fInt = acType.GetField("m_IntArgument", BindingFlags.Instance | BindingFlags.NonPublic);
        var fFloat = acType.GetField("m_FloatArgument", BindingFlags.Instance | BindingFlags.NonPublic);
        var fString = acType.GetField("m_StringArgument", BindingFlags.Instance | BindingFlags.NonPublic);
        var fBool = acType.GetField("m_BoolArgument", BindingFlags.Instance | BindingFlags.NonPublic);
        var fObj = acType.GetField("m_ObjectArgument", BindingFlags.Instance | BindingFlags.NonPublic);
        var fObjAsmName = acType.GetField("m_ObjectArgumentAssemblyTypeName", BindingFlags.Instance | BindingFlags.NonPublic);

        try
        {
            if (expectedType == typeof(int) && fInt != null)
                return (int)fInt.GetValue(argumentCache);

            if (expectedType == typeof(float) && fFloat != null)
                return (float)fFloat.GetValue(argumentCache);

            if (expectedType == typeof(string) && fString != null)
                return (string)fString.GetValue(argumentCache);

            if (expectedType == typeof(bool) && fBool != null)
                return (bool)fBool.GetValue(argumentCache);

            if (typeof(UnityEngine.Object).IsAssignableFrom(expectedType) && fObj != null)
            {
                var obj = fObj.GetValue(argumentCache) as UnityEngine.Object;
                // 如果 Inspector 中的 Object 类型与期望类型不同，尝试兼容（通常应一致）
                if (obj != null && expectedType.IsAssignableFrom(obj.GetType()))
                    return obj;

                // 如果需要，可以根据 m_ObjectArgumentAssemblyTypeName 做更严格的类型校验
                return obj; // 可能为 null
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[GetSingleArgumentFromArgumentCache] Failed to read argument: {e}");
        }

        // 不支持的类型返回 null
        return null;
    }

    private static void ExecuteActionsSafely(List<Action> actions)
    {
        if (actions == null) return;
        foreach (var a in actions)
        {
            try { a?.Invoke(); }
            catch (Exception e) { Debug.LogWarning($"[ExecuteActionsSafely] {e}"); }
        }
    }

    public void RebuildPreSolvedActions()
    {
        _preSolveMarkedActions = BuildMarkedActionsFromUnityEvent(OnSolved, typeof(NeedReorderedSFXSettings));
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class NeedReorderedSFXSettings : Attribute
{
}
