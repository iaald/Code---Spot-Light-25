using UnityEngine;

[CreateAssetMenu(fileName = "DialoguesData", menuName = "Scriptable Objects/TempDialogues")]
public class TempDialogues : ScriptableObject
{
    [TextArea] public string[] content;
}
