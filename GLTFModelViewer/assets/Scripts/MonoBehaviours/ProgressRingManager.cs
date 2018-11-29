using HoloToolkit.UX.Progress;
using UnityEngine;

public class ProgressRingManager : MonoBehaviour
{
    public void Show(string message)
    {
        ProgressIndicator.Instance.Open(
                IndicatorStyleEnum.AnimatedOrbs,
                ProgressStyleEnum.None,
                ProgressMessageStyleEnum.Visible,
                "Loading...");
    }
    public void Hide()
    {
        ProgressIndicator.Instance.Close();
    }
}
