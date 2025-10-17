# if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;

namespace Kuchinashi.Editor
{
    public class ChangeShader : EditorWindow
    {
        [MenuItem("Kuchinashi/Tools/One Click Change Shader")]
        public static void ShowWindow()
        {
            EditorWindow editorWindow = GetWindow(typeof(ChangeShader));
            editorWindow.autoRepaintOnSceneChange = false;
        }
        //当前shader
        public Material currentShader;
        //目标shader
        public Material changeShader;

        private void OnGUI()
        {
            currentShader = EditorGUILayout.ObjectField("保险", currentShader, typeof(Material), false) as Material;
            changeShader = EditorGUILayout.ObjectField("以此材质替换", changeShader, typeof(Material), false) as Material;
            if (GUILayout.Button("Change"))
            {
                Change();
            }
            GUILayout.TextArea("此功能仅用于将所有物体完全地转用URP-Sprite-Lit Shader。破坏性极强。慎用!");
        }

        public void Change()
        {
            if (currentShader == null || changeShader == null)
            {
                return;
            }
            foreach (GameObject obj in Resources.FindObjectsOfTypeAll(typeof(GameObject)))
            {
                try
                {
                    if (PrefabUtility.IsPartOfPrefabAsset(obj)) continue;
                    SpriteRenderer spr = obj.GetComponent<SpriteRenderer>();
                    if (spr.sharedMaterial.name.Equals("Sprites-Default"))
                    {
                        Debug.Log("Changed " + spr.gameObject.name);
                        spr.sharedMaterial = changeShader;

                    }
                }
                catch (Exception)
                {

                }
            }
            Debug.Log("Done");
        }
    }
}

# endif