using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct Flycam : IComponentData
{
    public float mouseSensitivity;
    public float rollSensitivity;
    public float moveSpeed;
    public int octMoveSpeed;
    public float moveSpeedChangeMultiplier;
}

public class FlycamComponent : ComponentDataWrapper<Flycam> { }
