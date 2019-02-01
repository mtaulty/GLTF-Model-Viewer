using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusWatcher : MonoBehaviour, IFocusable
{
    public void OnFocusEnter()
    {
        focusedObject = this.gameObject;
    }
    public void OnFocusExit()
    {
        focusedObject = null;
    }
    public static bool HasFocusedObject => (focusedObject != null);
    public static GameObject FocusedObject => focusedObject;
    static GameObject focusedObject;
}
