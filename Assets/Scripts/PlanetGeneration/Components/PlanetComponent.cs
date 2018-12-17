using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct Planet : IComponentData
{
    public HyperDistance radius;
    public HyperDistance hyperdistanceThreshold;
    public int maxNodeLevels;
    [Range(0, 8)]
    public int meshSubdivisions;
}

public class PlanetComponent : ComponentDataWrapper<Planet> { }
