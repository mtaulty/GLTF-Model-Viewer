using UnityEngine;

public class ProgressRingManager : MonoBehaviour
{
    public void Show(string message)
    {
        if (!this.isOpen)
        {
            // MIKET: to do - we have no implementation yet.
            this.isOpen = true;
            //ProgressIndicator.Instance.Open(
            //        IndicatorStyleEnum.AnimatedOrbs,
            //        ProgressStyleEnum.None,
            //        ProgressMessageStyleEnum.Visible,
            //        message);
        }
        else
        {
            // MIKET: to do - we have no implementation yet.
            //ProgressIndicator.Instance.SetMessage(message);
        }
    }
    public void Hide()
    {
        if (this.isOpen)
        {
            this.isOpen = false;

            // MIKET: to do - we have no implementation yet.
            // ProgressIndicator.Instance.Close();
        }
    }
    bool isOpen;
}
