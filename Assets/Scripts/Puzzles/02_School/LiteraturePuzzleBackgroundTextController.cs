using System.Collections;
using TMPro;
using UnityEngine;

namespace Puzzle.School
{
    public class LiteraturePuzzleBackgroundTextController : MonoBehaviour
    {
        private TMP_Text text;

        private void Awake()
        {
            text = GetComponent<TMP_Text>();
        }

        public void AddText(string text)
        {
            StartCoroutine(AddTextCoroutine(text));
        }

        private IEnumerator AddTextCoroutine(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                this.text.text += text[i];
                yield return new WaitForSeconds(0.1f);
            }
            this.text.text += "\n\n";
        }
    }
}
