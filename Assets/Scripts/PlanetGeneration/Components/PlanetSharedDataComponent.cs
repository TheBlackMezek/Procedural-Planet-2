using System;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[Serializable]
public struct PlanetSharedData : ISharedComponentData
{
    public GameObject nodePrefab;
    public Position playerPos;
}

public class PlanetSharedDataComponent : SharedComponentDataWrapper<PlanetSharedData> { }
