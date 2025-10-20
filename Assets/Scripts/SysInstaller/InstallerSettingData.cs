using System;
using UnityEngine;

[CreateAssetMenu(fileName = "InstallerSetting", menuName = "Scriptable Objects/InstallerSetting")]
public class InstallerSettingData : ScriptableObject
{
    [Serializable]
    public class Option
    {
        public bool UseInput = false;
        public string question;
        public string[] options;
    }
    public Color TitleBg;
    public Color TitleText;
    public Color OptionBg;
    public Color OptionText;
    public string Title;
    public Option[] Options;
}
