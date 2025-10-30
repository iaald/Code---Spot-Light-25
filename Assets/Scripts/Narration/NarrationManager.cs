using System;
using System.Collections;
using System.Collections.Generic;
using Kuchinashi;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DataSystem;
using UnityEngine.InputSystem;
using System.Linq;

namespace Narration
{
    public partial class NarrationManager : MonoSingleton<NarrationManager>
    {
        private PlotData m_currentPlot;
        public bool IsNarrationActive => m_currentPlot != null;
        private bool m_CanReceiveInput = true;
        private bool m_CanSkip = true;

        [SerializeField] private WindowPetDialogueBox m_DialogueBox;

        private void Awake()
        {
            TypeEventSystem.Global.Register<OnLineReadEvent>(e => {
                OnLineReceived(e);
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            TypeEventSystem.Global.Register<OnLinesReadEvent>(e => {
                OnLinesReceived(e);
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            TypeEventSystem.Global.Register<OnStoryEndEvent>(e => {
                // StateMachine.ChangeState(NarrationType.None);

                // if (!m_currentPlot.Temporary) GameProgressData.Instance.FinishPlot(m_currentPlot.Id);
                m_currentPlot = null;
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            if (m_DialogueBox == null) m_DialogueBox = GetComponentInChildren<WindowPetDialogueBox>();
        }

        public void StartNarration(string plotId)
        {
            if (IsNarrationActive) return;

            if (GameDesignData.GetData<PlotData>(plotId, out var plot))
            {
                m_currentPlot = plot;

                TypeEventSystem.Global.Send(new InitializeStoryEvent { plot = plot });
            }
            else
            {
                Debug.LogError($"Plot {plotId} not found");
            }
        }

        public void StopNarration()
        {
            // StateMachine.ChangeState(NarrationType.None);
            m_currentPlot = null;
        }

        private void Update()
        {
            if (!m_CanReceiveInput) return;

            if (InputSystem.actions["NextLine"].WasPressedThisFrame())
            {
                if (m_CanSkip && !m_DialogueBox.IsTypingComplete)
                {
                    m_DialogueBox.SkipTyping();
                    return;
                }
                TypeEventSystem.Global.Send<RequestNewLineEvent>();
            }
        }

        /// <summary>
        /// Called when a line is received from the InkReader. Mostly for normal sentences.
        /// </summary>
        /// <param name="e"></param>
        private void OnLineReceived(OnLineReadEvent e)
        {
            m_CanSkip = e.tags.TryGetValue("skippable", out var canSkip) && bool.Parse(canSkip);


            string content = e.content;
            content = content.Replace("{username}", GameProgressData.GetUsername());
            content = content.Replace("{usernameLastWord}", GameProgressData.GetUsername()[^1..].ToString());

            m_DialogueBox.ShowText(content);
        }

        private void OnLinesReceived(OnLinesReadEvent e)
        {
            m_CanSkip = e.tags.TryGetValue("skippable", out var canSkip) && bool.Parse(canSkip);

            string content = e.content;
            content = content.Replace("{username}", GameProgressData.GetUsername());
            content = content.Replace("{usernameLastWord}", GameProgressData.GetUsername()[^1..].ToString());

            m_DialogueBox.ShowText(content);
            m_DialogueBox.SetOptions(e.lines.Select((line, index) => new WindowPetDialogueBox.Option(index, line.content)).ToList());
        }
    }
}
