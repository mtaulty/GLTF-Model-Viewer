using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootParentProvider : MonoBehaviour
{
    [SerializeField]
#pragma warning disable 0649
    GameObject parentObject;
#pragma warning restore 0649

    public GameObject RootParent => this.parentObject;

}
