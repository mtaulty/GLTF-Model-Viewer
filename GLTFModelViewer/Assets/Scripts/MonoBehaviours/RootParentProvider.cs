using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootParentProvider : MonoBehaviour
{
    [SerializeField]
    GameObject parentObject;

    public GameObject RootParent => this.parentObject;

}
