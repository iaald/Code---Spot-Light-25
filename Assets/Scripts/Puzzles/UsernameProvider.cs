using DataSystem;
using TMPro;
using UnityEngine;

namespace Puzzles
{
    public class UsernameProvider : MonoBehaviour
    {
        private TMP_Text text;
        public string TargetString = "";

        private void Awake()
        {
            text = GetComponentInChildren<TMP_Text>();
        }

        public void SetString()
        {
            text.SetText(TargetString.Replace("{username}", GameProgressData.GetUsername()));
        }
    }
}