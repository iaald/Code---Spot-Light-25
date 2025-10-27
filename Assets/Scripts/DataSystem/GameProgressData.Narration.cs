using System.Collections.Generic;
using System.Linq;
using Kuchinashi.DataSystem;
using UnityEngine;
using QFramework;

namespace DataSystem
{
    public partial class GameProgressData
    {
        public string CurrentNarrationId = "";
        public List<string> FinishedNarrationIds = new();
    }

    public partial class GameProgressData
    {
        public bool IsNarrationFinished(string narrationId) => FinishedNarrationIds.Contains(narrationId);
        public void FinishNarration(string narrationId)
        {
            if (IsNarrationFinished(narrationId)) return;
            
            FinishedNarrationIds.Add(narrationId);
            if (CurrentNarrationId == narrationId) CurrentNarrationId = "";

            Serialize();
        }
        public void ResetNarrationProgress()
        {
            FinishedNarrationIds.Clear();
            CurrentNarrationId = "";

            Serialize();
        }
    }
}