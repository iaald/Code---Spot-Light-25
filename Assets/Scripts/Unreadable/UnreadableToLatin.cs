using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TextVFX
{
    public class UnreadableToLatin : MonoBehaviour, IUnreadableMasker
    {
        [Header("基本设置")]
        public bool isDoUnreadable = true;
        public TMP_FontAsset Font;
        [TextArea] public string Text = "";
        private string _maskedText = "";
        [SerializeField] private TextMeshProUGUI textMeshProFace;
        [SerializeField] private TextMeshProUGUI textMeshProContent;

        void Start()
        {
            DoFullLengthMask();
            SyncTMP();
            Debug.Log(ComputeFaceContLenRatio());
        }

        bool isLastframDoUnreadable;
        void Update()
        {
            if (isLastframDoUnreadable != isDoUnreadable)
            {
                SyncTMP();
            }
            if (TryGetMouseCharIndexInString(textMeshProContent, out int srcIndex))
            {
                Debug.Log($"Mouse over char (source index): {srcIndex}");
            }
            isLastframDoUnreadable = isDoUnreadable;
        }


        #region Unreadable Mask Interface
        public void DoMask()
        {
            throw new System.NotImplementedException();
        }

        public void RemoveMask()
        {
            throw new System.NotImplementedException();
        }

        public void SetBlur(float radius)
        {
            throw new System.NotImplementedException();
        }

        public void SetIntensity(float intensity)
        {
            throw new System.NotImplementedException();
        }

        public void SetRange(int S, int E)
        {
            throw new System.NotImplementedException();
        }
        #endregion


        #region Unreadable Converter
        public static string ConvertUnreadableLatin(string inStr, int key = 12345, int offset = 0)
        {
            const int firstChar = 0x0400;
            const int lastChar = 0x04FF;
            const int rangeSize = lastChar - firstChar + 1;

            var resultChars = new char[inStr.Length];

            var iptChars = new char[inStr.Length];
            for (int i = 0; i < inStr.Length; i++)
            {
                iptChars[i] = inStr[(i + offset) % inStr.Length];
            }

            Debug.Log(new string(iptChars));

            for (int i = 0; i < iptChars.Length; i++)
            {

                int hash = (iptChars[i] * 397) ^ (i * key);
                int randomChar = firstChar + (hash % rangeSize);

                if (randomChar < firstChar) randomChar += rangeSize;
                if (randomChar > lastChar) randomChar -= rangeSize;

                resultChars[i] = (char)randomChar;
            }

            return new string(resultChars);
        }

        public void DoFullLengthMask()
        {
            _maskedText = ConvertUnreadableLatin(Text);

            float rt = ComputeFaceContLenRatio();

            if ((rt - 1) * Text.Length < 2)
            {
                return;
            }
            else
            {
                // This will skip if mask is shorted than content
                int inc_copy = (int)Mathf.Ceil(rt) - 1;
                for (int i = 1; i <= inc_copy; i++)
                {
                    _maskedText += ConvertUnreadableLatin(Text, offset: i);
                }
                // Buggy: if generated new Mask goes into new line, this method dosen't work,
                // for it take lask char as the farthest one.
                int rightIdx = (int)Mathf.Round(_maskedText.Length * ComputeFaceContLenRatio());
                rightIdx = math.min(rightIdx + 1, _maskedText.Length - 1); // Add 1 for tail
                _maskedText = _maskedText[..rightIdx];
            }
        }

        #endregion


        #region Index Char
        /// <summary>
        /// 如果鼠标当前位于 textMeshPro 上的某个字符之上，
        /// 返回 true，并通过 out 参数给出该字符在「原始字符串 Text」中的下标。
        /// </summary>
        public bool TryGetMouseCharIndexInString(TextMeshProUGUI textMeshPro, out int sourceIndex)
        {
            Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            sourceIndex = -1;
            if (textMeshPro == null || string.IsNullOrEmpty(Text)) return false;

            // 获得当前用于命中测试的摄像机（Overlay 无需摄像机）
            var canvas = textMeshPro.canvas;
            Camera cam = null;
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                cam = canvas.worldCamera;

            // 首先判断鼠标是否在 RectTransform 内
            if (!TMP_TextUtilities.IsIntersectingRectTransform(textMeshPro.rectTransform, mousePos, cam))
                return false;

            // 找到与鼠标相交的字符索引（基于可见字符序列）
            int charIdx = TMP_TextUtilities.FindIntersectingCharacter(textMeshPro, mousePos, cam, true);

            if (charIdx == -1) return false;

            // 边界保护
            var textInfo = textMeshPro.textInfo;
            if (charIdx < 0 || charIdx >= textInfo.characterCount) return false;

            // 关键：TMP_CharacterInfo.index 是该字符在源字符串中的下标
            sourceIndex = textInfo.characterInfo[charIdx].index;
            return sourceIndex >= 0;
        }

        public float ComputeFaceContLenRatio()
        {
            TMP_TextInfo f_textInfo = this.textMeshProFace.GetTextInfo(_maskedText);
            var f = f_textInfo.characterInfo[_maskedText.Length - 1].topRight;
            var fs = f_textInfo.characterInfo[0].topLeft;

            TMP_TextInfo c_textInfo = this.textMeshProContent.GetTextInfo(Text);
            var c = c_textInfo.characterInfo[Text.Length - 1].topRight;
            var cs = c_textInfo.characterInfo[0].topLeft;
            Debug.Log(f + " " + fs + "\n" + c + " " + cs);
            return Mathf.Abs(c.x - cs.x) / Mathf.Abs(f.x - fs.x);
        }

        #endregion


        #region In Editor Helper
        void SyncTMP()
        {
            if (textMeshProFace != null)
            {
                textMeshProFace.text = isDoUnreadable ? _maskedText : Text;
                textMeshProContent.text = Text;
            }
        }
        #endregion
    }
}
