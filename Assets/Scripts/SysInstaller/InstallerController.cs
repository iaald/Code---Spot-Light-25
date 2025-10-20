using System;
using System.Collections.Generic;
using System.Threading;
using QFramework;
using UnityEngine;

public class InstallerController : MonoBehaviour
{
    public InstallerSettingData installerSettingData;
    public GameObject installerButton;
    private List<InstallerButton> chds = new();
    void Start()
    {
        LoadNewMenu(installerSettingData);
    }
    public void LoadNewMenu(InstallerSettingData inSettingData)
    {
        this.installerSettingData = inSettingData;

        gameObject.DestroyChildren();
        chds.Clear();

        var title = Instantiate(installerButton, transform).GetComponent<InstallerButton>();
        title.Color = installerSettingData.TitleBg;
        title.TMP_L.color = installerSettingData.TitleText;
        title.TMP_L.text = installerSettingData.Title;
        title.TMP_R.text = "";

        chds.Add(title);

        for (int i = 0; i < installerSettingData.Options.Length; i++)
        {
            var temp = Instantiate(installerButton, transform).GetComponent<InstallerButton>();
            temp.SetOption(installerSettingData.Options[i]);

            temp.Color = installerSettingData.OptionBg;
            temp.TMP_L.color = installerSettingData.OptionText;
            temp.TMP_R.color = installerSettingData.OptionText;

            chds.Add(temp);
        }
    }

    public string ResolveOptionString(string optionString)
    {
        var strs = optionString.Split("$");
        for (int i = 0; i < strs.Length; i++)
        {
            try
            {
                var idx = int.Parse(strs[i]);
                strs[i] = chds[idx].GetR();
            }
            catch (Exception)
            {

            }
        }
        return string.Join("", strs);
    }

    public void RefreshAllOptionText()
    {
        for (int i = 1; i < chds.Count; i++)
        {
            chds[i].Refresh();
        }
    }
    public string GetRightValue(int idx)
    {
        return chds[idx].GetR();
    }
}
