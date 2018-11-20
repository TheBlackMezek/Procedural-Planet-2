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

        EntityArray entityTempArray = nodeGroup.GetEntityArray();
        Entity[] entityArray = new Entity[entityTempArray.Length];
        for (int i = 0; i < entityArray.Length; ++i)
            entityArray[i] = entityTempArray[i];

        SharedComponentDataArray<MeshInstanceRenderer> meshCDArray = nodeGroup.GetSharedComponentDataArray<MeshInstanceRenderer>();
        MeshInstanceRenderer[] meshArray = new MeshInstanceRenderer[meshCDArray.Length];
        for (int i = 0; i < meshArray.Length; ++i)
            meshArray[i] = meshCDArray[i];

        ComponentDataArray<TerrainNode> nodeCDArray = nodeGroup.GetComponentDataArray<TerrainNode>();
        TerrainNode[] nodeArray = new TerrainNode[nodeCDArray.Length];
        for (int i = 0; i < nodeCDArray.Length; ++i)
            nodeArray[i] = nodeCDArray[i];
        
        for(int i = 0; i < meshArray.Length; ++i)
        {
            //Debug.Log("Start" + nodeArray[i].built);
            Vector3 corner1 = nodeArray[i].corner1;
            Vector3 corner2 = nodeArray[i].corner2;
            Vector3 corner3 = nodeArray[i].corner3;
            //Debug.Log("Set corners" + nodeArray[i].built);

            Mesh mesh = new Mesh();
            //Debug.Log("Made mesh" + nodeArray[i].built);

            Vector3[] vertices = new Vector3[3]
            {
                corner1,
                corner2,
                corner3
            };
            //Debug.Log("Set vertices" + nodeArray[i].built);

            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            //Debug.Log("Calculated normals" + nodeArray[i].built);

            MeshInstanceRenderer mir = meshArray[i];
            //Debug.Log("Got mesh renderer" + nodeArray[i].built);
            mir.mesh = mesh;
            Entity e = entityArray[i];
            EntityManager.SetSharedComponentData(e, mir);
            //Debug.Log("Set mesh" + nodeArray[i].built);
        }
    }

}
