using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using UnityEngine;

namespace ECS_Experiments
{
    public class TestSystem : JobComponentSystem
    {

        [Unity.Burst.BurstCompile]
        struct MovementJob : IJobProcessComponentData<Position, Test>
        {
            public float deltaTime;

            public void Execute(ref Position position, [ReadOnly] ref Test testVal)
            {
                position.Value += new float3(0f, testVal.moveSpeed * deltaTime, 0f);
            }
        }



        //private EntityArchetype arc;
        //
        //protected override void OnCreateManager()
        //{
        //    arc = EntityManager.CreateArchetype(typeof(Position), typeof(Test), typeof(Unity.Rendering.MeshInstanceRendererComponent));
        //}

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            MovementJob job = new MovementJob
            {
                deltaTime = Time.deltaTime
            };
            
            JobHandle handle = job.Schedule(this, inputDeps);

            return handle;
        }




        //ComponentGroup mainGroup;
        //
        //protected override void OnCreateManager()
        //{
        //    mainGroup = GetComponentGroup(typeof(TestComponent), typeof(Position));
        //}
        //
        //protected override void OnUpdate()
        //{
        //    Debug.Log("mainGroup length:" + mainGroup.CalculateLength());
        //
        //    ComponentDataArray<Position> compDatArray = mainGroup.GetComponentDataArray<Position>();
        //    ComponentArray<TestComponent> compArray = mainGroup.GetComponentArray<TestComponent>();
        //    for (int i = 0; i < compDatArray.Length; ++i)
        //    {
        //        float3 prevPos = compDatArray[i].Value;
        //        compDatArray[i] = new Position
        //            { Value = prevPos + new float3(0f, Time.deltaTime * compArray[i].Value.moveSpeed, 0f) };
        //    }
        //}

    }
}
