# if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using TMPro;

namespace Kuchinashi.Editor
{
    public class FontChanger : EditorWindow
    {
        [MenuItem("Kuchinashi/Tools/One Click Font Changer")]
        public static void Menu()
        {
            var window = GetWindow<FontChanger>();
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.PrefixLabel("Source Font");
            _sourceFont = EditorGUILayout.ObjectField(_sourceFont, typeof(TMP_FontAsset), false) as TMP_FontAsset;
            EditorGUILayout.PrefixLabel("Target Font");
            _targetFont = EditorGUILayout.ObjectField(_targetFont, typeof(TMP_FontAsset), false) as TMP_FontAsset;

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Reset"))
            {
                ResetFont();
            }

            if (_sourceFont != null && _targetFont != null)
            {
                GUI.backgroundColor = Color.green;
            }
            else
            {
                GUI.backgroundColor = Color.red;
            }

            if (GUILayout.Button("Change") && _sourceFont != null && _targetFont != null)
            {
                ChangeAllPrefabs();
                ChangeAllScenes();
            }

            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
        }

        private static void ResetFont()
        {
            _sourceFont = SystemFont;
            _targetFont = null;
        }

        private static void ChangeAllPrefabs()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab");

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                ChangePrefab(prefab);
            }

            AssetDatabase.SaveAssets();
        }

        public static void ChangePrefab(GameObject prefab, bool ignoreSourceFont = false)
        {
            bool hasChanged = ChangeFont(prefab, ignoreSourceFont);
            if (hasChanged)
            {
                EditorUtility.SetDirty(prefab);
            }
        }

        private static void ChangeAllScenes()
        {
            var guids = AssetDatabase.FindAssets("t:Scene");

            foreach (var guid in guids)
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(guid);
                ChangeScene(scenePath);
            }
        }

        public static void ChangeScene(string scenePath, bool ignoreSourceFont = false)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath);
            bool hasChanged = false;
            foreach (var go in scene.GetRootGameObjects())
            {
                hasChanged |= ChangeFont(go, ignoreSourceFont);
            }

            if (hasChanged)
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            EditorSceneManager.SaveOpenScenes();
        }

        private static bool ChangeFont(GameObject prefab, bool ignoreSourceFont = false)
        {
            TMP_Text[] texts = prefab.GetComponentsInChildren<TMP_Text>(true);

            bool hasChanged = false;
            foreach (var text in texts)
            {
                if (ignoreSourceFont || text.font == _sourceFont)
                {
                    text.font = _targetFont;

                    hasChanged = true;
                }
            }

            return hasChanged;
        }

        public static TMP_FontAsset SystemFont
        {
            get { return Resources.GetBuiltinResource<TMP_FontAsset>("Arial.ttf"); }
        }

        public static TMP_FontAsset _sourceFont;
        public static TMP_FontAsset _targetFont;
    }
}

# endif