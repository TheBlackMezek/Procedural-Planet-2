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
            
            EntityManager.Instantiate(array[0].prefab);
        }

    }
}
