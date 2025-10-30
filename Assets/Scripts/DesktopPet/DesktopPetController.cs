using Kuchinashi.Utils.Progressable;
using UnityEngine;

namespace DesktopPet
{
    public class DesktopPetController : MonoBehaviour
    {
        public ProgressableGroup progressableGroup;
        public WindowPetDialogueBox dialogueBox;

        private void Awake()
        {
            progressableGroup = GetComponent<ProgressableGroup>();
            dialogueBox = GetComponentInChildren<WindowPetDialogueBox>();
        }
    }
}
