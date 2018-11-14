using System;
using Unity.Entities;
using UnityEngine;

namespace ECS_Experiments
{

    [Serializable]
    public struct Test : IComponentData
    {
        public float moveSpeed;
    }

    public class TestComponent : ComponentDataWrapper<Test> { }

}
