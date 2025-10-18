using UnityEngine;

public class InstallerController : MonoBehaviour
{
    public InstallerSettingData installerSettingData;
    public GameObject installerButton;
    void Start()
    {
        var title = Instantiate(installerButton, transform).GetComponent<InstallerButton>();
        title.Color = installerSettingData.TitleBg;
        title.TMP_L.color = installerSettingData.TitleText;
        title.TMP_L.text = installerSettingData.Title;
        title.TMP_R.text = "";

        for (int i = 0; i < installerSettingData.Options.Length; i++)
        {
            var temp = Instantiate(installerButton, transform).GetComponent<InstallerButton>();
            temp.SetOption(installerSettingData.Options[i]);

            temp.Color = installerSettingData.OptionBg;
            temp.TMP_L.color = installerSettingData.OptionText;
            temp.TMP_R.color = installerSettingData.OptionText;
        }
    }
}
