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
    public void Hide()
    {
        // TODO: I need to understand how you are supposed to do this on V2, I just want
        // to switch all cursors off when the user cannot do anything useful with them.
        foreach (var inputSource in MixedRealityToolkit.InputSystem.DetectedInputSources)
        {
            foreach (var pointer in inputSource.Pointers)
            {
                if ((pointer.IsActive) && (pointer.BaseCursor != null))
                {
                    pointer.BaseCursor.SetVisibility(false);
                    this.hiddenPointers.Add(pointer);
                }
            }
        }
        MixedRealityToolkit.InputSystem.GazeProvider.Enabled = false;
    }
    public void Show()
    {
        foreach (var pointer in this.hiddenPointers)
        {
            pointer.BaseCursor.SetVisibility(true);
        }
        this.hiddenPointers.Clear();

        MixedRealityToolkit.InputSystem.GazeProvider.Enabled = true;
    }
    List<IMixedRealityPointer> hiddenPointers;
}