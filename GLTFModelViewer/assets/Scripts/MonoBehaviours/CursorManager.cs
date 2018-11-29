using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField]
    private ObjectCursor cursor;

    public void Show()
    {
        this.cursor.gameObject.SetActive(true);
    }
    public void Hide()
    {
        this.cursor.gameObject.SetActive(false);
    }
}
