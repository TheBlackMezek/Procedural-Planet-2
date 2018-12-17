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
    public Planet planetData;
    public PlanetNoise noiseData;
    public float3 parentPreciseCenter;
    public int3 parentOctantCenter;
    public float parentPreciseSubdivideDist;
    public int parentOctantSubdivideDist;
    public int built;
    public int divided;
    public int childrenBuilt;
    public Entity parentEntity;
    public int hyperDistant;
}

public class TerrainNodeComponent : ComponentDataWrapper<TerrainNode> { }
