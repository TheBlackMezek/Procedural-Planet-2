using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct Flycam : IComponentData
{
    public float mouseSensitivity;
    public float rollSensitivity;
}

public class FlycamComponent : ComponentDataWrapper<Flycam> { }
