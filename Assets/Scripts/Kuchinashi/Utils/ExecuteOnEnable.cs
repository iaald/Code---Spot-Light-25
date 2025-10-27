using UnityEngine;
using UnityEngine.Events;

namespace Kuchinashi.Utils
{
    public class ExecuteOnEnable : MonoBehaviour
    {
        [Header("Settings")]
        public UnityEvent onEnable;
        public float delay = 0f;

        private void OnEnable()
        {
            if (delay > 0f)
            {
                Invoke(nameof(Execute), delay);
            }
            else
            {
                Execute();
            }
        }
        private void Execute() => onEnable?.Invoke();
    }
}