using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using UnityEngine;

public class PlanetSystem : ComponentSystem
{

    private bool flag = false;

    private float3[] icoVerts;



    protected override void OnCreateManager()
    {
        float t = (1f + Mathf.Sqrt(5f)) / 2f;

        //(-1,  t,  0)   0
        //( 1,  t,  0)   1
        //(-1, -t,  0)   2
        //( 1, -t,  0)   3
        //
        //( 0, -1,  t)   4
        //( 0,  1,  t)   5
        //( 0, -1, -t)   6
        //( 0,  1, -t)   7
        //
        //( t,  0, -1)   8
        //( t,  0,  1)   9
        //(-t,  0, -1)   10
        //(-t,  0,  1)   11

        //( 0, 11,  5)
        //( 0,  5,  1)
        //( 0,  1,  7)
        //( 0,  7, 10)
        //( 0, 10, 11)
        //
        //
        //( 1,  5,  9)
        //( 5, 11,  4)
        //(11, 10,  2)
        //(10,  7,  6)
        //( 7,  1,  8)
        //
        //
        //( 3,  9,  4)
        //( 3,  4,  2)
        //( 3,  2,  6)
        //( 3,  6,  8)
        //( 3,  8,  9)
        //
        //
        //( 4,  9,  5)
        //( 2,  4, 11)
        //( 6,  2, 10)
        //( 8,  6,  7)
        //( 9,  8,  1)

        icoVerts = new float3[]
        {
            new float3(-1, t, 0), new float3(-t, 0, 1), new float3(0, 1, t),
            new float3(-1, t, 0), new float3(0, 1, t), new float3(1, t, 0),
            new float3(-1, t, 0), new float3(1, t, 0), new float3(0, 1, -t),
            new float3(-1, t, 0), new float3(0, 1, -t), new float3(-t, 0, -1),
            new float3(-1, t, 0), new float3(-t, 0, -1), new float3(-t, 0, 1),

            new float3(1, t, 0), new float3(0, 1, t), new float3(t, 0, 1),
            new float3(0, 1, t), new float3(-t, 0, 1), new float3(0, -1, t),
            new float3(-t, 0, 1), new float3(-t, 0, -1), new float3(-1, -t, 0),
            new float3(-t, 0, -1), new float3(0, 1, -t), new float3(0, -1, -t),
            new float3(0, 1, -t), new float3(1, t, 0), new float3(t, 0, -1),

            new float3(1, -t, 0), new float3(t, 0, 1), new float3(0, -1, t),
            new float3(1, -t, 0), new float3(0, -1, t), new float3(-1, -t, 0),
            new float3(1, -t, 0), new float3(-1, -t, 0), new float3(0, -1, -t),
            new float3(1, -t, 0), new float3(0, -1, -t), new float3(t, 0, -1),
            new float3(1, -t, 0), new float3(t, 0, -1), new float3(t, 0, 1),

            new float3(0, -1, t), new float3(t, 0, 1), new float3(0, 1, t),
            new float3(-1, -t, 0), new float3(0, -1, t), new float3(-t, 0, 1),
            new float3(0, -1, -t), new float3(-1, -t, 0), new float3(-t, 0, -1),
            new float3(t, 0, -1), new float3(0, -1, -t), new float3(0, 1, -t),
            new float3(t, 0, 1), new float3(t, 0, -1), new float3(1, t, 0)
        };

    }

    protected override void OnUpdate()
    {
        if (flag)
            return;

        flag = true;

        ComponentGroup planetGroup = GetComponentGroup(typeof(Planet), typeof(PlanetNoise));
        ComponentGroup dataGroup = GetComponentGroup(typeof(PlanetSharedData));
        
        ComponentDataArray<Planet> planetArray = planetGroup.GetComponentDataArray<Planet>();
        ComponentDataArray<PlanetNoise> noiseArray = planetGroup.GetComponentDataArray<PlanetNoise>();
        SharedComponentDataArray<PlanetSharedData> dataArray = dataGroup.GetSharedComponentDataArray<PlanetSharedData>();
        
        GameObject prefab = dataArray[0].nodePrefab;

        for(int i = 0; i < planetArray.Length; ++i)
        {
            Planet planet = planetArray[i];
            PlanetNoise noise = noiseArray[i];
            HyperDistance r = planet.radius;

            for(int n = 0; n < 20; ++n)
            {
                Entity nodeEntity = EntityManager.Instantiate(prefab);
                TerrainNode node = EntityManager.GetComponentData<TerrainNode>(nodeEntity);
                node.level = 0;
                node.planetData = planet;
                node.noiseData = noise;
                node.built = 0;
                node.divided = 0;

                int idx = n * 3;
                node.corner1 = icoVerts[idx];
                node.corner2 = icoVerts[idx+1];
                node.corner3 = icoVerts[idx+2];
                EntityManager.SetComponentData(nodeEntity, node);

                HyperPosition pos = math.normalize(node.corner1 + node.corner2 + node.corner3) * r;
                
                PrecisePosition prspos = new PrecisePosition { pos = pos.prs };
                EntityManager.SetComponentData(nodeEntity, prspos);

                OctantPosition octpos = new OctantPosition { pos = pos.oct };
                EntityManager.SetComponentData(nodeEntity, octpos);
            }
        }
    }

}
