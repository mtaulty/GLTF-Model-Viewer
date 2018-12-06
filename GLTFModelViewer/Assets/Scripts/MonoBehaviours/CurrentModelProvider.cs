using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentModelProvider : MonoBehaviour
{
    ParentProvider ParentProvider => this.gameObject.GetComponent<ParentProvider>();

    public bool HasModel => this.CurrentModel != null;

    public GameObject CurrentModel
    {
        get
        {
            GameObject currentModel = null;

            if (this.ParentProvider.GLTFModelParent.transform.childCount > 0)
            {
                currentModel = this.ParentProvider.GLTFModelParent.transform.GetChild(0).gameObject;
            }
            return (currentModel);
        }
    }
}
