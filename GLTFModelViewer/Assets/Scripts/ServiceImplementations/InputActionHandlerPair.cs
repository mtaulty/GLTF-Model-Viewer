using Microsoft.MixedReality.Toolkit.Input;
using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class InputActionHandlerPair
{
    [SerializeField]
    public MixedRealityInputAction action;
    [SerializeField]
    public UnityEvent handler;
}