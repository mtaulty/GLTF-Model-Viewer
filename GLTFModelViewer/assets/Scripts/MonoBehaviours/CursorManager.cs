using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public CursorManager()
    {
        this.hiddenPointers = new List<IMixedRealityPointer>();
    }
    public void Show()
    {
        // TODO: I need to understand how you are supposed to do this on V2, I just want
        // to switch all cursors off when the user cannot do anything useful with them.
        foreach (var inputSource in MixedRealityToolkit.InputSystem.DetectedInputSources)
        {
            foreach (var pointer in inputSource.Pointers)
            {
                if (pointer.IsActive)
                {
                    pointer.IsActive = false;
                    this.hiddenPointers.Add(pointer);
                }
            }
        }
    }
    public void Hide()
    {
        foreach (var pointer in this.hiddenPointers)
        {
            pointer.IsActive = true;
        }
        this.hiddenPointers.Clear();
    }
    List<IMixedRealityPointer> hiddenPointers;
}