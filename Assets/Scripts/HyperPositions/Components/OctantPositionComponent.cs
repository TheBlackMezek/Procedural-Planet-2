using System;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

[Serializable]
public struct OctantPosition : IComponentData
{
    public int3 pos;
}

public class OctantPositionComponent : ComponentDataWrapper<OctantPosition> { }
