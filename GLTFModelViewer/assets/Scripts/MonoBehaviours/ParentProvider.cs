using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentProvider : MonoBehaviour
{
    [SerializeField]
    GameObject parentObject;

    public GameObject GLTFModelParent => this.parentObject;
}
