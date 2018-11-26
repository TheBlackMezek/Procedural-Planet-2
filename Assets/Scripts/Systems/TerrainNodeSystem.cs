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
            if (nodeArray[i].built != 0)
                continue;

            nodeArray[i].built = 1;
            
            Planet planetData = nodeArray[i].planetData;

            Vector3 corner1 = nodeArray[i].corner1 * planetData.radius;
            Vector3 corner2 = nodeArray[i].corner2 * planetData.radius;
            Vector3 corner3 = nodeArray[i].corner3 * planetData.radius;

            Vector3[] corners = new Vector3[3] { nodeArray[i].corner3, nodeArray[i].corner2, nodeArray[i].corner1 };

            Mesh mesh = BuildMesh(corners, planetData.meshSubdivisions, planetData.radius, nodeArray[i].noiseData, 0);

            //Mesh mesh = new Mesh();
            //
            //Vector3[] vertices = new Vector3[3]
            //{
            //    corner1,
            //    corner2,
            //    corner3
            //};
            //int[] tris = new int[3]
            //{
            //    0, 1, 2
            //};
            //
            //mesh.vertices = vertices;
            //mesh.triangles = tris;
            //mesh.RecalculateNormals();

            //MeshInstanceRenderer mir = meshArray[i];
            //mir.mesh = mesh;
            meshArray[i].mesh = mesh;
            Entity e = entityArray[i];
            EntityManager.SetSharedComponentData(e, meshArray[i]);
            EntityManager.SetComponentData(e, nodeArray[i]);
        }
    }

    public static Mesh BuildMesh(Vector3[] corners, int divisions, float sphereRadius, PlanetNoise noiseData, int nodeLevel)
    {
        // rez is the number of vertices on one side of the mesh/triangle
        // the part in parentheses is called the "Mersenne Number"
        int rez = 2 + ((int)Mathf.Pow(2, divisions) - 1);
        // nTris is the number of tris in the mesh
        int t = rez - 2;
        int nTris = (t * (t + 1)) + (rez - 1);
        // nVerts is the number of vertices in the mesh
        // it is the formula for the "Triangle Sequence" of numbers
        int nVerts = (rez * (rez + 1)) / 2;
        
        Vector3[] vertices = new Vector3[nVerts];
        Vector3[] normals = new Vector3[nVerts];
        Vector2[] uvs = new Vector2[nVerts];
        Color[] vColors = new Color[nVerts];
        int[] indices = new int[nTris * 3];

        float dist01 = Vector3.Distance(corners[0], corners[1]);
        float dist12 = Vector3.Distance(corners[1], corners[2]);
        float dist20 = Vector3.Distance(corners[2], corners[0]);

        float lenAxis01 = dist01 / (rez - 1);
        float lenAxis12 = dist12 / (rez - 1);
        float lenAxis20 = dist20 / (rez - 1);

        Vector3 add1 = (corners[1] - corners[0]).normalized * lenAxis01;
        Vector3 add2 = (corners[2] - corners[1]).normalized * lenAxis12;

        int vIdx = 0;

        for (int i = 0; i < rez; ++i)
        {
            for (int n = 0; n <= i; ++n)
            {
                vertices[vIdx] = corners[0] + add1 * i + add2 * n;
                Vector3 normal = (vertices[vIdx]).normalized;
                float noiseVal = GetValue(normal, noiseData, nodeLevel);
                vertices[vIdx] = normal * (sphereRadius + noiseVal);

                normals[vIdx] = normal;

                vColors[vIdx] = GetVertexColor(noiseData, noiseVal);

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

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.colors = vColors;
        //mesh.uv = uvs;
        //mesh.normals = normals;
        mesh.RecalculateNormals();
        return mesh;
    }



    private static Color GetVertexColor(PlanetNoise noiseData, float heightFromSeaLevel)
    {
        int layerIdx = 0;

        return Color.gray;

        //int len = noiseData.colorLayers.Length;
        //for (int i = 1; i < len; ++i)
        //{
        //    PlanetColorLayer layer = noiseData.colorLayers[i];
        //
        //    if (layer.heightThreshold < heightFromSeaLevel)
        //        layerIdx = i;
        //    else
        //        break;
        //}
        //
        //return new Color(noiseData.colorLayers[layerIdx].r,
        //                 noiseData.colorLayers[layerIdx].g,
        //                 noiseData.colorLayers[layerIdx].b);
    }



    public static float GetValue(float x, float y, float z, PlanetNoise noiseData, int level = 0)
    {
        float ret = GetNoiseValue(x, y, z, noiseData, level);
        //ret = settings.heightCurve.Evaluate(ret);
        if (UnityEngine.Random.Range(0, 1000) == 1) Debug.Log(ret);
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
            //ret += noiseClass.GetValue(x * localFreq, y * localFreq, z * localFreq) * localAmp;
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
