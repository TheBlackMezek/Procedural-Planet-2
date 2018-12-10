using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct PlanetNoise : IComponentData
{
    public int octaves;
    public float frequency;
    public float lacunarity;
    public float amplitude;
    public float persistence;

    //public PlanetColorLayer[] colorLayers;

    public float finalValueMultiplier;

    public int seed;
}

[Serializable]
public struct PlanetColorLayer
{
    public float heightThreshold;
    public float r;
    public float g;
    public float b;
}

public class PlanetNoiseComponent : ComponentDataWrapper<PlanetNoise> { }
