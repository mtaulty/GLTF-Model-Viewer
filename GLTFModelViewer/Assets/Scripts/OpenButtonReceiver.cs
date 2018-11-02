using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.Receivers;
using UnityEngine;
using UnityGLTF;

public class OpenButtonReceiver : InteractionReceiver
{
    protected override async void InputClicked(GameObject obj, InputClickedEventData eventData)
    {
        base.InputClicked(obj, eventData);

        if (obj.name == this.interactables[OPEN_BUTTON_INDEX].name)
        {
            var controller = this.gameObject.GetComponent<ModelController>();
            await controller.OpenNewModelAsync();
        }
    }
    // I don't really like hard-coding this, should do something better.
    static readonly int OPEN_BUTTON_INDEX = 0;
}
