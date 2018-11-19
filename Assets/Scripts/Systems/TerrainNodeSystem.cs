using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using UnityEngine;
using Unity.Rendering;

public class TerrainNodeSystem : ComponentSystem
{

    protected override void OnUpdate()
    {
        ComponentGroup nodeGroup = GetComponentGroup(typeof(TerrainNode), typeof(MeshInstanceRenderer));

        ComponentDataArray<TerrainNode> nodeArray = nodeGroup.GetComponentDataArray<TerrainNode>();
        //ComponentDataArray<MeshInstanceRenderer> meshGroup = nodeGroup.GetComponentDataArray<MeshInstanceRenderer>();
    }

}
