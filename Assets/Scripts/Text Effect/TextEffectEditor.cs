using TMPro;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class TextEffectEditor : MonoBehaviour
{
    public bool editFromGrid;
    [SerializeField] private Color textcolor;
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
    
    void Start()
    {
        if(editFromGrid)
        { 
        targetTMp=GetComponentInChildren<TextMeshProUGUI>();
        targetBlinkSwitch=GetComponentInChildren<TextBlinkSwitch>();
        targetShaker=GetComponentInChildren<TextShaker>();
        targetTMp.color=textcolor;
            if(shakerOn)
            {
                targetShaker.enabled=true;
                targetShaker.amplitude = amplitude;
                targetShaker.frequency = frequency;
                targetShaker.randomize = randomize;
            }
            else
            {
                targetShaker.enabled = false;
            }


            if(blinkOn) 
            {
                targetBlinkSwitch.enabled=true;
                targetBlinkSwitch.textA = textA;
                targetBlinkSwitch.textB = textB;
                targetBlinkSwitch.fadeDuration = fadeDuration;
                targetBlinkSwitch.stayDuration = stayDuration;
            }
            else
            {
                targetBlinkSwitch.enabled = false;
            }
        }
    }

    private void Update()
    {

        targetTMp ??= GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
        targetBlinkSwitch ??= GetComponentInChildren<TextBlinkSwitch>(includeInactive: true);
        targetShaker ??= GetComponentInChildren<TextShaker>(includeInactive: true);
    }


}

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
        isRandomize = serializedObject.FindProperty (nameof(TextEffectEditor.randomize));
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();
        EditorGUILayout.Separator();

        TextEffectEditor editor = (TextEffectEditor)target;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Text A");
        editor.targetBlinkSwitch.textA = (string) EditorGUILayout.TextField(editor.textA);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Text B");
        editor.targetBlinkSwitch.textB = EditorGUILayout.TextField(editor.textB);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Stay Duration");
        editor.targetBlinkSwitch.stayDuration = EditorGUILayout.FloatField(editor.stayDuration);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Fade Duration");
        editor.targetBlinkSwitch.fadeDuration = EditorGUILayout.FloatField(editor.fadeDuration);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Frequency");
        editor.targetShaker.frequency = EditorGUILayout.FloatField(editor.frequency);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Amplitude");
        editor.targetShaker.amplitude = EditorGUILayout.FloatField(editor.amplitude);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Randomize");
        editor.targetShaker.randomize = EditorGUILayout.Toggle(editor.randomize);
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }
}
