using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NoteHelper : MonoBehaviour
{
    [TextArea(3, 10)]
    public string note = "在这里写下你的注释...";
    
    public NoteType noteType = NoteType.Info;
    public bool showInBuild = false;
    
    public enum NoteType
    {
        Info,
        Warning,
        Important,
        Todo
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(NoteHelper))]
public class NoteHelperEditor : Editor
{
    private SerializedProperty noteProperty;
    private SerializedProperty noteTypeProperty;
    private SerializedProperty showInBuildProperty;
    
    private static readonly Color[] noteColors = new Color[]
    {
        new Color(0.2f, 0.6f, 1.0f, 0.3f),    // Info - 蓝色
        new Color(1.0f, 0.8f, 0.2f, 0.3f),    // Warning - 黄色
        new Color(1.0f, 0.3f, 0.3f, 0.3f),    // Important - 红色
        new Color(0.3f, 0.8f, 0.3f, 0.3f)     // Todo - 绿色
    };
    
    private static readonly string[] noteIcons = new string[]
    {
        "console.infoicon",     // Info
        "console.warnicon",     // Warning  
        "console.erroricon",    // Important
        "d_Animation.AddEvent"  // Todo
    };
    
    private static readonly string[] noteTitles = new string[]
    {
        "开发注释",
        "警告",
        "重要提示", 
        "待办事项"
    };

    private void OnEnable()
    {
        noteProperty = serializedObject.FindProperty("note");
        noteTypeProperty = serializedObject.FindProperty("noteType");
        showInBuildProperty = serializedObject.FindProperty("showInBuild");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // 保存原始背景颜色
        Color originalBackgroundColor = GUI.backgroundColor;
        
        // 根据注释类型设置颜色
        int noteTypeIndex = noteTypeProperty.enumValueIndex;
        Color noteColor = noteColors[noteTypeIndex];
        
        // 开始绘制注释区域
        EditorGUILayout.BeginVertical(GUI.skin.box);
        
        // 设置背景颜色
        GUI.backgroundColor = noteColor;
        
        // 绘制标题栏
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        // 绘制图标和标题
        GUIContent titleContent = EditorGUIUtility.IconContent(noteIcons[noteTypeIndex]);
        titleContent.text = noteTitles[noteTypeIndex];
        GUILayout.Label(titleContent, EditorStyles.boldLabel);
        
        GUILayout.FlexibleSpace();
        
        // 注释类型下拉菜单
        EditorGUI.BeginChangeCheck();
        noteTypeProperty.enumValueIndex = (int)(NoteHelper.NoteType)EditorGUILayout.EnumPopup(
            (NoteHelper.NoteType)noteTypeProperty.enumValueIndex, 
            GUILayout.Width(100));
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
        
        EditorGUILayout.EndHorizontal();
        
        // 重置背景颜色
        GUI.backgroundColor = originalBackgroundColor;
        
        // 绘制文本区域
        EditorGUILayout.Space();
        
        EditorGUI.BeginChangeCheck();
        string newNote = EditorGUILayout.TextArea(noteProperty.stringValue, GUILayout.MinHeight(60));
        if (EditorGUI.EndChangeCheck())
        {
            noteProperty.stringValue = newNote;
        }
        
        EditorGUILayout.Space();
        
        // 绘制底部选项
        EditorGUILayout.BeginHorizontal();
        
        // 在构建中显示选项
        EditorGUILayout.PropertyField(showInBuildProperty, new GUIContent("构建中显示"), GUILayout.Width(100));
        
        GUILayout.FlexibleSpace();
        
        // 清空按钮
        if (GUILayout.Button("清空", GUILayout.Width(60)))
        {
            noteProperty.stringValue = "";
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        
        // 应用修改
        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
        }
        
        // 绘制分隔线
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
        
        // 显示一些基本信息
        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField("组件信息", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("挂载对象", target.name);
        EditorGUILayout.LabelField("注释长度", $"{noteProperty.stringValue.Length} 字符");
        EditorGUILayout.LabelField("创建时间", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
        EditorGUILayout.EndVertical();
    }
}
#endif