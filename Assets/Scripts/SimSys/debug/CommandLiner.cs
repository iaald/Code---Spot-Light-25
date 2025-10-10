using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CommandLiner : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    [SerializeField] private TMP_InputField textMeshProInputField;
    void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            string cmd = textMeshProInputField.text;
            if (cmd.StartsWith(" ")) return;
            textMeshProInputField.text = "";
            textMeshProUGUI.text = "";
            string[] argv = cmd.Split(" ");


            if (argv[0] == "echo")
            {
                string cbndStr = "";

                cbndStr += argv[1];
                for (int i = 2; i < argv.Length; i++)
                {
                    cbndStr += " " + argv[i];
                }
                textMeshProUGUI.text = cbndStr;
            }
        }
    }
}
