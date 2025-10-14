using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TextVFX
{
    public class UnreadableToLatin : MonoBehaviour, IUnreadableConverter
    {
        [Header("基本设置")]
        [TextArea] public string Text = "";
        private string _maskedText = "";
        [SerializeField] private TextMeshProUGUI textMeshProFace;
        [SerializeField] private TextMeshProUGUI textMeshProContent;

        #region Unreadable Converter
        public static string ConvertUnreadableLatin(string inStr, int key = 12345, int offset = 0)
        {
            const int firstChar = 0x0400;
            const int lastChar = 0x04FF;
            const int rangeSize = lastChar - firstChar + 1;

            int length = inStr.Length;
            // 使用stackalloc避免堆分配（适用于小字符串）
            Span<char> resultChars = length <= 256
                ? stackalloc char[length]
                : new char[length];

            for (int i = 0; i < length; i++)
            {
                char sourceChar = inStr[(i + offset) % length];
                int hash = (sourceChar * 397) ^ (i * key);
                int randomChar = firstChar + (math.abs(hash) % rangeSize);

                resultChars[i] = (char)randomChar;
            }

            return new string(resultChars);
        }

        public void GenerateFullLengthMask()
        {
            _maskedText = ConvertUnreadableLatin(Text);

            float rt = ComputeFaceContLenRatio();

            Debug.Log(rt);

            if ((rt - 1) * Text.Length < 0.5)
            {
                return;
            }
            else
            {
                // This will be skipped if mask is shorted than content
                int inc_copy = (int)Mathf.Ceil(rt);
                for (int i = 1; i < inc_copy; i++)
                {
                    _maskedText += ConvertUnreadableLatin(Text, offset: i);
                }
                // Buggy: if generated new Mask goes into new line, this method dosen't work,
                // for it take lask char as the farthest one.
                int rightIdx = (int)Mathf.Round(_maskedText.Length * ComputeFaceContLenRatio());
                rightIdx = math.min(rightIdx, _maskedText.Length); // Add 1 for tail
                _maskedText = _maskedText[..rightIdx];
            }
            Debug.Log(ComputeFaceContLenRatio());
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

            // TMP_CharacterInfo.index 是该字符在源字符串中的下标
            sourceIndex = textInfo.characterInfo[charIdx].index;
            return sourceIndex >= 0;
        }

        public float ComputeFaceContLenRatio()
        {
            if (string.IsNullOrEmpty(_maskedText) || string.IsNullOrEmpty(Text))
                throw new NullReferenceException("Mask or Content TMP is empty.");

            TMP_TextInfo f_textInfo = this.textMeshProFace.GetTextInfo(_maskedText);
            if (f_textInfo.characterCount == 0) return 1f;
            var f = f_textInfo.characterInfo[f_textInfo.characterCount - 1].topRight;

            TMP_TextInfo c_textInfo = this.textMeshProContent.GetTextInfo(Text);
            if (c_textInfo.characterCount == 0) return 1f;
            var c = c_textInfo.characterInfo[c_textInfo.characterCount - 1].topRight;
            var cs = c_textInfo.characterInfo[0].topLeft;

            float denominator = Mathf.Abs(f.x - cs.x);
            if (denominator < 0.0001f) return 1f;  // 防止除零

            return Mathf.Abs(c.x - cs.x) / denominator;
        }

        #endregion


        #region Binding
        public TextMeshProUGUI GetUnreadableTMP()
        {
            return textMeshProFace;
        }

        public TextMeshProUGUI GetContentTMP()
        {
            return textMeshProContent;
        }

        public bool GetPiontedIndex(IUnreadableConverter.TMP_WHERE where, out int index)
        {
            return this.TryGetMouseCharIndexInString(
                where == IUnreadableConverter.TMP_WHERE.Content ?
                textMeshProContent : textMeshProFace
                , out index
            );
        }

        public string GetTextUnreadable()
        {
            return _maskedText;
        }

        public string GetTextContent()
        {
            return Text;
        }
        #endregion
    }
}
