﻿using System;
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
    public float3 parentCenter;
    public float parnetSubdivideDist;
    public int built;
    public int divided;
    public int childrenBuilt;
    public Entity parentEntity;
}

public class TerrainNodeComponent : ComponentDataWrapper<TerrainNode> { }