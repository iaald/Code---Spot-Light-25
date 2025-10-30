using TMPro;
using UnityEngine;

# if UNITY_EDITOR

using UnityEditor;

# endif

[ExecuteInEditMode]
public class TextEffectEditor : MonoBehaviour
{
    private TextMeshProUGUI targetTMp;
    public TextShaker targetShaker;
    public TextBlinkSwitch targetBlinkSwitch;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Blink效果")]
    public bool blinkOn;
    public string textA => targetBlinkSwitch.textA;
    public string textB => targetBlinkSwitch.textB;
    public float fadeDuration => targetBlinkSwitch.fadeDuration;    // 淡入淡出时长
    public float stayDuration => targetBlinkSwitch.stayDuration;    // 保持停留时长
    [Header("shaker效果")]
    public bool shakerOn;
    public float amplitude => targetShaker.amplitude;   // 晃动幅度
    public float frequency => targetShaker.frequency;   // 晃动速度
    public bool randomize => targetShaker.randomize;  // 是否随机相位（避免同步晃动）
    
    public Color textColor
    {
        get
        {
            if (targetTMp == null) targetTMp = GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
            return targetTMp != null ? targetTMp.color : default;
        }
        set
        {
            if (targetTMp == null) targetTMp = GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
            if (targetTMp != null)
            {
                targetTMp.color = value;
            }
        }
    }
    
    void Start()
    {
        // 保持空实现，Inspector 中直接驱动组件属性
    }

    private void Update()
    {

        targetTMp ??= GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
        targetBlinkSwitch ??= GetComponentInChildren<TextBlinkSwitch>(includeInactive: true);
        targetShaker ??= GetComponentInChildren<TextShaker>(includeInactive: true);
    }


}

# if UNITY_EDITOR

[CustomEditor(typeof(TextEffectEditor)), CanEditMultipleObjects]
public class TextEffectEditorInspector : Editor
{
    SerializedProperty isBlinkOn;
    SerializedProperty isShakerOn;
    SerializedProperty isRandomize;

    public void OnEnable()
    {
        isBlinkOn = serializedObject.FindProperty(nameof(TextEffectEditor.blinkOn));
        isShakerOn = serializedObject.FindProperty(nameof(TextEffectEditor.shakerOn));
        // randomize 非序列化字段，不通过序列化属性访问
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 颜色编辑：直接读写 TextMeshProUGUI.color
        using (new EditorGUI.DisabledScope(false))
        {
            Color mixedColor = ((TextEffectEditor)targets[0]).textColor;
            bool mixed = false;
            for (int i = 1; i < targets.Length; i++)
            {
                if (((TextEffectEditor)targets[i]).textColor != mixedColor)
                {
                    mixed = true; break;
                }
            }
            EditorGUI.showMixedValue = mixed;
            EditorGUI.BeginChangeCheck();
            var newColor = EditorGUILayout.ColorField("Text Color", mixedColor);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(System.Array.ConvertAll(targets, t => ((TextEffectEditor)t).GetComponentInChildren<TextMeshProUGUI>(true) as Object), "Change TMP Color");
                foreach (var t in targets)
                {
                    var comp = (TextEffectEditor)t;
                    comp.textColor = newColor;
                    var tmp = comp.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (tmp != null) EditorUtility.SetDirty(tmp);
                }
            }
            EditorGUI.showMixedValue = false;
        }

        EditorGUILayout.Space();

        // Blink 分组
        EditorGUILayout.BeginVertical("box");
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(isBlinkOn, GUIContent.none, GUILayout.Width(20));
		EditorGUILayout.EndHorizontal();

        using (new EditorGUI.DisabledScope(!isBlinkOn.boolValue && !isBlinkOn.hasMultipleDifferentValues))
        {
            string mixedTextA = ((TextEffectEditor)targets[0]).targetBlinkSwitch != null ? ((TextEffectEditor)targets[0]).targetBlinkSwitch.textA : string.Empty;
            string mixedTextB = ((TextEffectEditor)targets[0]).targetBlinkSwitch != null ? ((TextEffectEditor)targets[0]).targetBlinkSwitch.textB : string.Empty;
            bool mixedA = false, mixedB = false;
            float mixedStay = ((TextEffectEditor)targets[0]).targetBlinkSwitch != null ? ((TextEffectEditor)targets[0]).targetBlinkSwitch.stayDuration : 0f;
            float mixedFade = ((TextEffectEditor)targets[0]).targetBlinkSwitch != null ? ((TextEffectEditor)targets[0]).targetBlinkSwitch.fadeDuration : 0f;
            bool mixedStayV = false, mixedFadeV = false;
            for (int i = 1; i < targets.Length; i++)
            {
                var ed = (TextEffectEditor)targets[i];
                if (ed.targetBlinkSwitch == null) continue;
                if (ed.targetBlinkSwitch.textA != mixedTextA) mixedA = true;
                if (ed.targetBlinkSwitch.textB != mixedTextB) mixedB = true;
                if (!Mathf.Approximately(ed.targetBlinkSwitch.stayDuration, mixedStay)) mixedStayV = true;
                if (!Mathf.Approximately(ed.targetBlinkSwitch.fadeDuration, mixedFade)) mixedFadeV = true;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = mixedA;
            var newA = EditorGUILayout.TextField("Text A", mixedTextA);
            EditorGUI.showMixedValue = mixedB;
            var newB = EditorGUILayout.TextField("Text B", mixedTextB);
            EditorGUI.showMixedValue = mixedStayV;
            var newStay = EditorGUILayout.FloatField("Stay Duration", mixedStay);
            EditorGUI.showMixedValue = mixedFadeV;
            var newFade = EditorGUILayout.FloatField("Fade Duration", mixedFade);
            EditorGUI.showMixedValue = false;

            if (EditorGUI.EndChangeCheck())
            {
                foreach (var t in targets)
                {
                    var comp = (TextEffectEditor)t;
                    if (comp.targetBlinkSwitch == null) continue;
                    Undo.RecordObject(comp.targetBlinkSwitch, "Edit Blink");
                    comp.targetBlinkSwitch.textA = newA;
                    comp.targetBlinkSwitch.textB = newB;
                    comp.targetBlinkSwitch.stayDuration = newStay;
                    comp.targetBlinkSwitch.fadeDuration = newFade;
                    EditorUtility.SetDirty(comp.targetBlinkSwitch);
                }
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Shaker 分组
        EditorGUILayout.BeginVertical("box");
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(isShakerOn, GUIContent.none, GUILayout.Width(20));
		EditorGUILayout.EndHorizontal();

        using (new EditorGUI.DisabledScope(!isShakerOn.boolValue && !isShakerOn.hasMultipleDifferentValues))
        {
            float mf = ((TextEffectEditor)targets[0]).targetShaker != null ? ((TextEffectEditor)targets[0]).targetShaker.frequency : 0f;
            float ma = ((TextEffectEditor)targets[0]).targetShaker != null ? ((TextEffectEditor)targets[0]).targetShaker.amplitude : 0f;
            bool mr = ((TextEffectEditor)targets[0]).targetShaker != null && ((TextEffectEditor)targets[0]).targetShaker.randomize;
            bool mfv = false, mav = false, mrv = false;
            for (int i = 1; i < targets.Length; i++)
            {
                var ed = (TextEffectEditor)targets[i];
                if (ed.targetShaker == null) continue;
                if (!Mathf.Approximately(ed.targetShaker.frequency, mf)) mfv = true;
                if (!Mathf.Approximately(ed.targetShaker.amplitude, ma)) mav = true;
                if (ed.targetShaker.randomize != mr) mrv = true;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = mfv;
            var nf = EditorGUILayout.FloatField("Frequency", mf);
            EditorGUI.showMixedValue = mav;
            var na = EditorGUILayout.FloatField("Amplitude", ma);
            EditorGUI.showMixedValue = mrv;
            var nr = EditorGUILayout.Toggle("Randomize", mr);
            EditorGUI.showMixedValue = false;

            if (EditorGUI.EndChangeCheck())
            {
                foreach (var t in targets)
                {
                    var comp = (TextEffectEditor)t;
                    if (comp.targetShaker == null) continue;
                    Undo.RecordObject(comp.targetShaker, "Edit Shaker");
                    comp.targetShaker.frequency = nf;
                    comp.targetShaker.amplitude = na;
                    comp.targetShaker.randomize = nr;
                    EditorUtility.SetDirty(comp.targetShaker);
                }
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // 应用 bool 属性修改
        serializedObject.ApplyModifiedProperties();

        // 根据最新的开关值同步组件启用状态
        foreach (var t in targets)
        {
            var comp = (TextEffectEditor)t;
            if (comp == null) continue;
            if (comp.targetBlinkSwitch == null) comp.targetBlinkSwitch = comp.GetComponentInChildren<TextBlinkSwitch>(true);
            if (comp.targetShaker == null) comp.targetShaker = comp.GetComponentInChildren<TextShaker>(true);

            if (comp.targetBlinkSwitch != null)
            {
                Undo.RecordObject(comp.targetBlinkSwitch, "Toggle Blink");
                comp.targetBlinkSwitch.enabled = comp.blinkOn;
                EditorUtility.SetDirty(comp.targetBlinkSwitch);
            }
            if (comp.targetShaker != null)
            {
                Undo.RecordObject(comp.targetShaker, "Toggle Shaker");
                comp.targetShaker.enabled = comp.shakerOn;
                EditorUtility.SetDirty(comp.targetShaker);
            }
        }
    }
}

# endif