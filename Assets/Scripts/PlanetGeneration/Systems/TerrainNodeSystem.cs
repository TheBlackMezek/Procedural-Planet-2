using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using UnityEngine;
using Unity.Rendering;
using System.Collections.Generic;

public class TerrainNodeSystem : ComponentSystem
{

    private const float PERCENT_DIST_TO_SUBDIVIDE_AT = 200f;

    private struct MeshCreationSet
    {
        public Entity entity;
        public JobHandle jobHandle;
        public NativeArray<Vector3> verts;
        public NativeArray<int> tris;
    }
    private List<MeshCreationSet> meshCreationSets = new List<MeshCreationSet>();



    struct MeshBuildJob : IJob
    {
        public TerrainNode node;
        public float3 corner0;
        public float3 corner1;
        public float3 corner2;
        public int rez;
        public int nTris;
        public int nVerts;
        public NativeArray<Vector3> verts;
        public NativeArray<int> tris;

        public void Execute()
        {
            Vector3[] vertices = new Vector3[nVerts];
            int[] indices = new int[nTris * 3];
            
            float dist01 = Vector3.Distance(corner0, corner1);
            float dist12 = Vector3.Distance(corner1, corner2);
            
            float lenAxis01 = dist01 / (rez - 1);
            float lenAxis12 = dist12 / (rez - 1);
            
            float3 add1 = math.normalize(corner1 - corner0) * lenAxis01;
            float3 add2 = math.normalize(corner2 - corner1) * lenAxis12;
            
            int vIdx = 0;

            float octantSize = HyperposStaticReferences.OctantSize;

            HyperDistance radius = node.planetData.radius;
            if (node.hyperDistant == 1)
                radius = radius / new HyperDistance { prs = 0f, oct = 1 };
            
            for (int i = 0; i < rez; ++i)
            {
                for (int n = 0; n <= i; ++n)
                {
                    vertices[vIdx] = corner0 + add1 * i + add2 * n;
                    float3 normal = (vertices[vIdx]).normalized;
                    float noiseVal = GetValue(normal, node.noiseData, node.level);
                    HyperPosition vertPos = normal * (radius + (noiseVal * node.noiseData.finalValueMultiplier));
                    //if (n == 0)
                    //    Debug.Log(MathUtils.ToString(node.planetData.radius) + "\n" +
                    //        MathUtils.ToString(noiseVal * node.noiseData.finalValueMultiplier) + "\n" +
                    //        MathUtils.ToString(node.planetData.radius + (noiseVal * node.noiseData.finalValueMultiplier)));
                    HyperPosition nodeCenter = GetNodeCenter(node);
                    HyperPosition relativePos = vertPos - nodeCenter;
                    vertices[vIdx] = relativePos.prs + (float3)relativePos.oct * octantSize;
                    //vertices[vIdx] = normal * (node.planetData.radius + noiseVal * node.noiseData.finalValueMultiplier);
                    //vertices[vIdx] = normal * sphereRadius; //Use this line instead of above to gen a perfect sphere

                    ++vIdx;
                }
            }
            
            int indIdx = 0;
            int rowStartIdx = 1;
            int prevRowStartIdx = 0;

            for (int row = 0; row < rez - 1; ++row)
            {
                bool upright = true;
                int trisInRow = 1 + row * 2;
                int vertsInRowBottom = row + 2;

                int upTri = 0;
                int downTri = 0;

                for (int tri = 0; tri < trisInRow; ++tri)
                {
                    if (upright)
                    {
                        indices[indIdx] = rowStartIdx + upTri + 1;
                        indices[indIdx + 1] = rowStartIdx + upTri;
                        indices[indIdx + 2] = prevRowStartIdx + upTri;
                        ++upTri;
                    }
                    else
                    {
                        indices[indIdx] = prevRowStartIdx + downTri + 1;
                        indices[indIdx + 1] = rowStartIdx + downTri + 1;
                        indices[indIdx + 2] = prevRowStartIdx + downTri;
                        ++downTri;
                    }

                    indIdx += 3;
                    upright = !upright;
                }

                prevRowStartIdx = rowStartIdx;
                rowStartIdx += vertsInRowBottom;
            }
            
            verts.CopyFrom(vertices);
            tris.CopyFrom(indices);
        }
    }



    protected override void OnUpdate()
    {
        for(int i = meshCreationSets.Count - 1; i >= 0; --i)
        {
            if(meshCreationSets[i].jobHandle.IsCompleted)
            {
                meshCreationSets[i].jobHandle.Complete();

                if(EntityManager.Exists(meshCreationSets[i].entity))
                {
                    Mesh mesh = new Mesh();

                    mesh.vertices = meshCreationSets[i].verts.ToArray();
                    mesh.triangles = meshCreationSets[i].tris.ToArray();
                    mesh.RecalculateNormals();

                    HPMeshInstanceRenderer r = EntityManager.GetSharedComponentData<HPMeshInstanceRenderer>(meshCreationSets[i].entity);

                    r.mesh = mesh;

                    EntityManager.SetSharedComponentData(meshCreationSets[i].entity, r);

                    TerrainNode node = EntityManager.GetComponentData<TerrainNode>(meshCreationSets[i].entity);

                    if(node.level != 0 && EntityManager.Exists(node.parentEntity))
                    {
                        TerrainNode parentNode = EntityManager.GetComponentData<TerrainNode>(node.parentEntity);

                        if(parentNode.divided == 1)
                        {
                            ++parentNode.childrenBuilt;
                            if(parentNode.childrenBuilt == 4)
                            {
                                HPMeshInstanceRenderer parentR = EntityManager.GetSharedComponentData<HPMeshInstanceRenderer>(node.parentEntity);
                                parentR.mesh = null;
                                EntityManager.SetSharedComponentData(node.parentEntity, parentR);
                            }

                            EntityManager.SetComponentData(node.parentEntity, parentNode);
                        }
                    }
                }

                meshCreationSets[i].verts.Dispose();
                meshCreationSets[i].tris.Dispose();

                meshCreationSets.RemoveAt(i);
            }
        }



        ComponentGroup nodeGroup = GetComponentGroup(typeof(TerrainNode), typeof(HPMeshInstanceRenderer), typeof(PrecisePosition), typeof(OctantPosition));
        ComponentGroup camGroup = GetComponentGroup(typeof(Flycam), typeof(PrecisePosition), typeof(Rotation), typeof(OctantPosition));
        ComponentGroup dataGroup = GetComponentGroup(typeof(PlanetSharedData));

        SharedComponentDataArray<PlanetSharedData> planetDataArray = dataGroup.GetSharedComponentDataArray<PlanetSharedData>();
        PlanetSharedData[] dataArray = new PlanetSharedData[planetDataArray.Length];
        for (int i = 0; i < dataArray.Length; ++i)
            dataArray[i] = planetDataArray[i];

        EntityArray entityTempArray = nodeGroup.GetEntityArray();
        Entity[] entityArray = new Entity[entityTempArray.Length];
        for (int i = 0; i < entityArray.Length; ++i)
            entityArray[i] = entityTempArray[i];

        SharedComponentDataArray<HPMeshInstanceRenderer> meshCDArray = nodeGroup.GetSharedComponentDataArray<HPMeshInstanceRenderer>();
        HPMeshInstanceRenderer[] meshArray = new HPMeshInstanceRenderer[meshCDArray.Length];
        for (int i = 0; i < meshArray.Length; ++i)
            meshArray[i] = meshCDArray[i];

        ComponentDataArray<TerrainNode> nodeCDArray = nodeGroup.GetComponentDataArray<TerrainNode>();
        TerrainNode[] nodeArray = new TerrainNode[nodeCDArray.Length];
        for (int i = 0; i < nodeCDArray.Length; ++i)
            nodeArray[i] = nodeCDArray[i];

        ComponentDataArray<PrecisePosition> nodePosArray = nodeGroup.GetComponentDataArray<PrecisePosition>();
        PrecisePosition[] posArray = new PrecisePosition[nodePosArray.Length];
        for (int i = 0; i < nodePosArray.Length; ++i)
            posArray[i] = nodePosArray[i];

        ComponentDataArray<OctantPosition> nodeOctArray = nodeGroup.GetComponentDataArray<OctantPosition>();
        OctantPosition[] octArray = new OctantPosition[nodePosArray.Length];
        for (int i = 0; i < nodeOctArray.Length; ++i)
            octArray[i] = nodeOctArray[i];

        ComponentDataArray<PrecisePosition> camPosArray = camGroup.GetComponentDataArray<PrecisePosition>();
        float3 camPos = camPosArray[0].pos;
        ComponentDataArray<OctantPosition> camOctArray = camGroup.GetComponentDataArray<OctantPosition>();
        int3 camOct = camOctArray[0].pos;

        float octantSize = HyperposStaticReferences.OctantSize;


        for (int i = 0; i < meshArray.Length; ++i)
        {
            if (nodeArray[i].built == 1 && nodeArray[i].divided == 0)
            {
                if(nodeArray[i].level < nodeArray[i].planetData.maxNodeLevels)
                {
                    float3 corner0 = nodeArray[i].corner1;
                    float3 corner1 = nodeArray[i].corner2;
                    float3 corner2 = nodeArray[i].corner3;
                    HyperDistance sphereRadius = nodeArray[i].planetData.radius;

                    HyperPosition corner0Pos = corner0 * sphereRadius;
                    HyperPosition corner1Pos = corner1 * sphereRadius;

                    HyperDistance distToSubdivide = MathUtils.Distance(corner0Pos, corner1Pos)
                                                                    * (PERCENT_DIST_TO_SUBDIVIDE_AT / 100f);
                    
                    HyperPosition centerPos = GetNodeCenter(nodeArray[i]);
                    
                    //if (UnityEngine.Random.Range(0, 20) == 2)
                    //    Debug.Log(MathUtils.ToString(distToSubdivide) + "\n" + MathUtils.ToString(centerPos));
                    if (InSubdivideDist(camOct, camPos, centerPos.oct, centerPos.prs, distToSubdivide.oct, distToSubdivide.prs))
                    {
                        //Debug.Log(MathUtils.ToString(distToSubdivide) + "\n" + MathUtils.ToString(centerPos)
                        //    + "\n" + camOct + " " + camPos);
                        Subdivide(entityArray[i], nodeArray[i], meshArray[i], dataArray[0],
                            distToSubdivide.prs, distToSubdivide.oct, centerPos.prs, centerPos.oct);
                    }
                }
                if(nodeArray[i].level > 0 && EntityManager.Exists(nodeArray[i].parentEntity))
                {
                    HPMeshInstanceRenderer parentR
                        = EntityManager.GetSharedComponentData<HPMeshInstanceRenderer>(nodeArray[i].parentEntity);
                    //float dist = math.distance(camPos, nodeArray[i].parentCenter);
                    //HyperDistance dist = MathUtils.Distance(camOct, camPos,
                    //    nodeArray[i].parentOctantCenter, nodeArray[i].parentPreciseCenter);

                    //if (parentR.mesh != null
                    //    && (dist.octantDist < nodeArray[i].parentOctantSubdivideDist
                    //        || (dist.octantDist == nodeArray[i].parentOctantSubdivideDist && dist.preciseDist < nodeArray[i].parentPreciseSubdivideDist)))
                    if(!InSubdivideDist(camOct, camPos, nodeArray[i].parentOctantCenter, nodeArray[i].parentPreciseCenter,
                        nodeArray[i].parentOctantSubdivideDist, nodeArray[i].parentPreciseSubdivideDist))
                        EntityManager.DestroyEntity(entityArray[i]);
                }
            }
            else if(nodeArray[i].built == 0 && nodeArray[i].divided == 1)
            {
                float3 corner0 = nodeArray[i].corner1;
                float3 corner1 = nodeArray[i].corner2;
                float3 corner2 = nodeArray[i].corner3;
                HyperDistance sphereRadius = nodeArray[i].planetData.radius;

                HyperPosition corner0Pos = corner0 * sphereRadius;
                HyperPosition corner1Pos = corner1 * sphereRadius;
                
                HyperDistance distToSubdivide = MathUtils.Distance(corner0Pos, corner1Pos) * (PERCENT_DIST_TO_SUBDIVIDE_AT / 100f);
                HyperPosition centerPos = GetNodeCenter(nodeArray[i]);
                
                if (!InSubdivideDist(camOct, camPos, centerPos.oct, centerPos.prs, distToSubdivide.oct, distToSubdivide.prs))
                {
                    nodeArray[i].divided = 0;
                    nodeArray[i].childrenBuilt = 0;
                    EntityManager.SetComponentData(entityArray[i], nodeArray[i]);
                }
            }
            else if(nodeArray[i].built == 0 && nodeArray[i].divided == 0)
            {
                nodeArray[i].built = 1;

                Planet planetData = nodeArray[i].planetData;

                // rez is the number of vertices on one side of the mesh/triangle
                // the part in parentheses is called the "Mersenne Number"
                int rez = 2 + ((int)Mathf.Pow(2, planetData.meshSubdivisions) - 1);
                // nTris is the number of tris in the mesh
                int t = rez - 2;
                int nTris = (t * (t + 1)) + (rez - 1);
                // nVerts is the number of vertices in the mesh
                // it is the formula for the "Triangle Sequence" of numbers
                int nVerts = (rez * (rez + 1)) / 2;

                NativeArray<Vector3> verts = new NativeArray<Vector3>(nVerts, Allocator.Persistent);
                NativeArray<int> tris = new NativeArray<int>(nTris * 3, Allocator.Persistent);

                HyperPosition centerPos = GetNodeCenter(nodeArray[i]);
                HyperPosition camHyp = new HyperPosition { prs = camPos, oct = camOct };
                HyperDistance dist = MathUtils.Distance(centerPos, camHyp);

                if (dist > planetData.hyperdistanceThreshold)
                {
                    nodeArray[i].hyperDistant = 1;
                    if(!EntityManager.HasComponent<HyperdistantMarker>(entityArray[i]))
                        EntityManager.AddComponent(entityArray[i], typeof(HyperdistantMarker));
                }
                else
                {
                    nodeArray[i].hyperDistant = 0;
                    if (EntityManager.HasComponent<HyperdistantMarker>(entityArray[i]))
                        EntityManager.RemoveComponent(entityArray[i], typeof(HyperdistantMarker));
                }

                MeshBuildJob job = new MeshBuildJob();
                job.node = nodeArray[i];
                job.corner0 = nodeArray[i].corner3;
                job.corner1 = nodeArray[i].corner2;
                job.corner2 = nodeArray[i].corner1;
                job.rez = rez;
                job.nTris = nTris;
                job.nVerts = nVerts;
                job.verts = verts;
                job.tris = tris;
                
                JobHandle handle = job.Schedule();
                JobHandle.ScheduleBatchedJobs();
                
                MeshCreationSet mcs = new MeshCreationSet();
                mcs.entity = entityArray[i];
                mcs.jobHandle = handle;
                mcs.verts = verts;
                mcs.tris = tris;
                meshCreationSets.Add(mcs);

                EntityManager.SetComponentData(entityArray[i], nodeArray[i]);
            }
        }

    }
    
    protected override void OnStopRunning()
    {
        for(int i = 0; i < meshCreationSets.Count; ++i)
        {
            meshCreationSets[i].jobHandle.Complete();
            meshCreationSets[i].verts.Dispose();
            meshCreationSets[i].tris.Dispose();
        }
    }



    private bool InSubdivideDist(int3 camOct, float3 camPrc, int3 nodeOct, float3 nodePrc, int octDivDist, float prcDivDist)
    {
        HyperDistance dist = MathUtils.Distance(camOct, camPrc, nodeOct, nodePrc);
        return dist.oct < octDivDist || (dist.oct == octDivDist && dist.prs < prcDivDist);
    }

    private void Subdivide(Entity e, TerrainNode t, HPMeshInstanceRenderer r, PlanetSharedData d,
        float parentPrcSubdivideDist, int parentOctSubdivideDist, float3 parentPrcCenter, int3 parentOctCenter)
    {
        Entity[] entities = new Entity[4];
        TerrainNode[] nodes = new TerrainNode[4];
        
        float3 corner0 = t.corner1;
        float3 corner1 = t.corner2;
        float3 corner2 = t.corner3;

        for (int i = 0; i < 4; ++i)
        {
            entities[i] = EntityManager.Instantiate(d.nodePrefab);
            nodes[i] = EntityManager.GetComponentData<TerrainNode>(entities[i]);
            nodes[i].level = t.level + 1;
            nodes[i].planetData = t.planetData;
            nodes[i].noiseData = t.noiseData;
            nodes[i].built = 0;
            nodes[i].divided = 0;
            nodes[i].childrenBuilt = 0;
            nodes[i].parentEntity = e;

            nodes[i].parentPreciseCenter = parentPrcCenter;
            nodes[i].parentOctantCenter = parentOctCenter;
            nodes[i].parentPreciseSubdivideDist = parentPrcSubdivideDist;
            nodes[i].parentOctantSubdivideDist = parentOctSubdivideDist;
        }
        
        float3 mid01 = corner1 - corner0;
        mid01 = corner0 + math.normalize(mid01) * (math.length(mid01) / 2f);
        float3 mid02 = corner2 - corner0;
        mid02 = corner0 + math.normalize(mid02) * (math.length(mid02) / 2f);
        float3 mid12 = corner2 - corner1;
        mid12 = corner1 + math.normalize(mid12) * (math.length(mid12) / 2f);
        
        nodes[0].corner1 = corner0;
        nodes[0].corner2 = mid01;
        nodes[0].corner3 = mid02;

        nodes[1].corner1 = mid01;
        nodes[1].corner2 = corner1;
        nodes[1].corner3 = mid12;

        nodes[2].corner1 = mid02;
        nodes[2].corner2 = mid12;
        nodes[2].corner3 = corner2;

        nodes[3].corner1 = mid02;
        nodes[3].corner2 = mid01;
        nodes[3].corner3 = mid12;

        for (int i = 0; i < 4; ++i)
        {
            EntityManager.SetComponentData(entities[i], nodes[i]);

            HyperPosition pos = math.normalize(nodes[i].corner1 + nodes[i].corner2 + nodes[i].corner3) * t.planetData.radius;

            PrecisePosition prspos = new PrecisePosition { pos = pos.prs };
            EntityManager.SetComponentData(entities[i], prspos);

            OctantPosition octpos = new OctantPosition { pos = pos.oct };
            EntityManager.SetComponentData(entities[i], octpos);
        }

        t.divided = 1;
        t.built = 0;
        
        EntityManager.SetComponentData(e, t);
    }




    /// <summary>
    /// Returns node center position relative to planet's center
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static HyperPosition GetNodeCenter(TerrainNode node)
    {
        return math.normalize(node.corner1 + node.corner2 + node.corner3) * node.planetData.radius;
    }

    public static float GetValue(float x, float y, float z, PlanetNoise noiseData, int level = 0)
    {
        float ret = GetNoiseValue(x, y, z, noiseData, level);
        
        return ret;
    }

    public static float GetNoiseValue(float x, float y, float z, PlanetNoise noiseData, int level = 0)
    {
        float localFreq = noiseData.frequency;
        float localAmp = noiseData.amplitude;
        
        float maxValue = 0f;
        float ret = 0f;

        FastNoise fastNoise = new FastNoise(noiseData.seed);

        for (int i = 0; i < noiseData.octaves + level * 1; ++i)
        {
            ret += fastNoise.GetSimplex(x * localFreq, y * localFreq, z * localFreq) * localAmp;

            maxValue += localAmp;

            localFreq *= noiseData.lacunarity;
            localAmp *= noiseData.persistence;
        }

        return ret / maxValue;
    }

    public static float GetValue(Vector3 pos, PlanetNoise noiseData, int level = 0)
        { return GetValue(pos.x, pos.y, pos.z, noiseData, level); }

}
