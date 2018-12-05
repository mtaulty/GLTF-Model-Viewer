using HoloToolkit.UX.Progress;
using UnityEngine;

public class ProgressRingManager : MonoBehaviour
{
    public void Show(string message)
    {
        if (!this.isOpen)
        {
            this.isOpen = true;
            ProgressIndicator.Instance.Open(
                    IndicatorStyleEnum.AnimatedOrbs,
                    ProgressStyleEnum.None,
                    ProgressMessageStyleEnum.Visible,
                    message);
        }
        else
        {
            ProgressIndicator.Instance.SetMessage(message);
        }
    }
    public void Hide()
    {
        if (this.isOpen)
        {
            this.isOpen = false;
            ProgressIndicator.Instance.Close();
        }
    }
    bool isOpen;
}
