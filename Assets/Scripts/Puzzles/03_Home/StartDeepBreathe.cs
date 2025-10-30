using DataSystem;
using DesktopPet;
using Narration;
using QFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StartDeepBreathe : MonoBehaviour
{
    public Button button;
    public DesktopPetController desktopPet;

    private void Awake()
    {
        desktopPet = GetComponent<DesktopPetController>();
    }

    void Start()
    {
        TypeEventSystem.Global.Register<OnStoryEndEvent>(e =>
        {
            if (e.plot.Id == "start_deep_breathe") button.onClick?.Invoke();
            else if (e.plot.Id == "demo_ending") SceneMng.Instance.SwitchScene("Opening Scene");
        }).UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    public void StartPlot()
    {
        TypeEventSystem.Global.Send(new InitializeStoryEvent() { plot = GameDesignData.GetPlot("start_deep_breathe", out var plot) ? plot : null });

        desktopPet.progressableGroup.LinearTransitionTo(0.75f, 3f);
        desktopPet.dialogueBox.ShowNarration(3f);
    }
    public void StartDemoEnding()
    {
        TypeEventSystem.Global.Send(new InitializeStoryEvent() { plot = GameDesignData.GetPlot("demo_ending", out var plot) ? plot : null });

        desktopPet.dialogueBox.ShowNarration(1f);
    }

    public void SetPetProgressTo(float target) => desktopPet.progressableGroup.Progress = target;
}
