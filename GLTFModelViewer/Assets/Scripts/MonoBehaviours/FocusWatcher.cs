using HoloToolkit.Unity.InputModule;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

public class FocusWatcher : MonoBehaviour, IMixedRealityFocusHandler
{
    public void OnFocusEnter(FocusEventData eventData)
    {
        focusedObject = this.gameObject;
    }
    public void OnFocusExit(FocusEventData eventData)
    {
        focusedObject = null;
    }
    public static bool HasFocusedObject => (focusedObject != null);
    public static GameObject FocusedObject => focusedObject;
    static GameObject focusedObject;
}
