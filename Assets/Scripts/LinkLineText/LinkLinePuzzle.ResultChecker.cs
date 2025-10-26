using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public partial class LinkLinePuzzle : MonoBehaviour
{
    [Header("Answers")]
    [Tooltip("Accept lines whatever the content is")]
    public bool OpenAnswer = false;
    public List<string> correctAnswers;
    public int requiredNumber = 1;
    private int correctCnt = 0;
    public bool CheckLine(int lineId)
    {
        if (OpenAnswer) return true;
        var str = string.Join("", this.GetSequenceFromCommittedLineStart(lineId));
        if (str.Equals("") || correctAnswers.Contains(str))
        {
            correctCnt += 1;
            return true;
        }
        else
        {
            return false;
        }
    }
    public UnityEvent OnSolved;
    public bool CheckPuzzle()
    {
        if (correctCnt == requiredNumber)
        {
            OnSolved?.Invoke();
            return true;
        }
        return false;
    }
    public List<string> GetAllResults()
    {
        List<string> results = new();
        foreach (var item in this.committedLines)
        {
            var t = GetSequenceFromCommittedLineStart(item.lineId);
            results.Add(string.Join("", t));
        }
        return results;
    }
}
