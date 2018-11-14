using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ECS_Experiments
{
    public class TestSystem : ComponentSystem
    {

        ComponentGroup mainGroup;

        protected override void OnCreateManager()
        {
            mainGroup = GetComponentGroup(typeof(TestComponent), typeof(Position));
        }

        protected override void OnUpdate()
        {
            ComponentDataArray<Position> compDatArray = mainGroup.GetComponentDataArray<Position>();
            ComponentArray<TestComponent> compArray = mainGroup.GetComponentArray<TestComponent>();
            for (int i = 0; i < compDatArray.Length; ++i)
            {
                float3 prevPos = compDatArray[i].Value;
                compDatArray[i] = new Position
                    { Value = prevPos + new float3(0f, Time.deltaTime * compArray[i].Value.moveSpeed, 0f) };
            }
        }

    }
}
