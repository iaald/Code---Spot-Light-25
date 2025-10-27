using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Kuchinashi.Utils.Progressable
{
    [ExecuteInEditMode]
    public class TMPTextColorProgressable : Progressable
    {
        public TMP_Text TargetText;
        
        public Color StartColor;
        public Color EndColor;

        private void Awake()
        {
            if (TargetText == null) TargetText = GetComponent<TMP_Text>();
        }

        internal override void Update()
        {
            if (TargetText == null) return;
            base.Update();

            TargetText.color = Color.Lerp(StartColor, EndColor, evaluation);
        }
    }
}