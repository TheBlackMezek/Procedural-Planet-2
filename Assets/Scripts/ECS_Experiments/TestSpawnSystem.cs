using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using UnityEngine;

namespace ECS_Experiments
{
    public class TestSpawnSystem : ComponentSystem
    {

        private bool flag = false;

        //protected override void OnCreateManager()
        //{
        //    ComponentGroup group = GetComponentGroup(typeof(TestSpawn));
        //    Debug.Log(group.CalculateLength());
        //    SharedComponentDataArray<TestSpawn> array = group.GetSharedComponentDataArray<TestSpawn>();
        //    Debug.Log(array.Length);
        //    EntityManager.Instantiate(array[0].prefab);
        //}

        protected override void OnUpdate()
        {
            if (flag)
                return;

            flag = true;

            ComponentGroup group = GetComponentGroup(typeof(TestSpawn));
            
            SharedComponentDataArray<TestSpawn> array = group.GetSharedComponentDataArray<TestSpawn>();

            int spawnCount = array[0].spawnCount;
            float spawnRange = array[0].spawnRange;
            GameObject prefab = array[0].prefab;

            for(int i = 0; i < spawnCount; ++i)
                EntityManager.Instantiate(prefab);

            ComponentGroup group2 = GetComponentGroup(typeof(Position));

            ComponentDataArray<Position> array2 = group2.GetComponentDataArray<Position>();
            UnityEngine.Random.Range(-spawnRange, spawnRange);
            for(int i = 0; i < array2.Length; ++i)
                array2[i] = new Position { Value = new float3(UnityEngine.Random.Range(-spawnRange, spawnRange),
                                                              0f,
                                                              UnityEngine.Random.Range(-spawnRange, spawnRange)) };
        }

    }
}
