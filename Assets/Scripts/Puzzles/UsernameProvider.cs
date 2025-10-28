using DataSystem;
using TMPro;
using UnityEngine;

namespace Puzzles
{
    public class UsernameProvider : MonoBehaviour
    {
        private TMP_Text text;
        public string TargetString = "";

        public void SetString()
        {
            text = GetComponentInChildren<TMP_Text>();
            text.SetText(TargetString.Replace("{username}", GameProgressData.GetUsername()));
        }
    }
}