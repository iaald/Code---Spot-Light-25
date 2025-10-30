using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kuchinashi.Utils.Progressable
{
    [ExecuteInEditMode]
    public class CanvasGroupAlphaProgressable : Progressable
    {
        [SerializeField] private CanvasGroup TargetCanvasGroup;

        [Header("Settings")]
        public float InitialProgress = 0f;
        public bool IsInteractable = true;
        public bool IsBlockRaycasts = true;

        private void Awake()
        {
            if (TargetCanvasGroup == null) TargetCanvasGroup = TryGetComponent<CanvasGroup>(out var cg) ? cg : null;

            Progress = InitialProgress;
        }

        internal override void Update()
        {
            if (TargetCanvasGroup == null) return;
            base.Update();

            TargetCanvasGroup.alpha = evaluation;
            
            if (Mathf.Approximately(TargetCanvasGroup.alpha, ProgressCurve.Evaluate(1)))
            {
                TargetCanvasGroup.blocksRaycasts = IsBlockRaycasts;
                TargetCanvasGroup.interactable = IsInteractable;
            }
            else
            {
                TargetCanvasGroup.blocksRaycasts = false;
                TargetCanvasGroup.interactable = false;
            }
        }
    }
}