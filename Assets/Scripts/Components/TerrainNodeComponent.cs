using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct TerrainNode : IComponentData
{
    public float3 corner1;
    public float3 corner2;
    public float3 corner3;
    public int level;
    public Planet planet;
}

public class TerrainNodeComponent : ComponentDataWrapper<TerrainNode> { }
