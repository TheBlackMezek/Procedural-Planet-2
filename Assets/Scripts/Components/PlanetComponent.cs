using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct Planet : IComponentData
{
    public float radius;
    public int maxNodeLevels;
    [Range(0, 8)]
    public int meshSubdivisions;
}

public class PlanetComponent : ComponentDataWrapper<Planet> { }
