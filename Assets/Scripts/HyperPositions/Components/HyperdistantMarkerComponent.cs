using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct HyperdistantMarker : IComponentData
{
}

public class HyperdistantMarkerComponent : ComponentDataWrapper<HyperdistantMarker> { }
