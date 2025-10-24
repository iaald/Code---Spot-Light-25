using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class LinkLinePuzzle : MonoBehaviour
{
    [Header("答案")]
    public List<string> correctAnswers;

    public bool CheckLine(int lineId)
    {
        var str = string.Join("", this.GetSequenceFromCommittedLineStart(lineId));
        if (str.Equals("") || correctAnswers.Contains(str))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
