using System;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

[Serializable]
public struct PrecisePosition : IComponentData
{
    public float3 pos;
}

public class PrecisePositionComponent : ComponentDataWrapper<PrecisePosition> { }
